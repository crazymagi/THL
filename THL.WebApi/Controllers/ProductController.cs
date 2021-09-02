﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using THL.Bll.Services;
using THL.Domain;
using THL.WebApi.Dtos;

namespace THL.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductController(ILogger<ProductController> logger, IProductService productService, IMapper mapper)
        {
            _logger = logger;
            _productService = productService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAsync([FromQuery] string searchTerm, [FromQuery] int page = 0, [FromQuery] int pageSize = 10)
        {
            var products = await _productService.GetProducts(searchTerm, page, pageSize);
            var result = _mapper.Map<List<ProductDto>>(products);

            return result;
        }
        
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetByIdAsync([FromQuery] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            var result = _mapper.Map<ProductDto>(product);
            return result;
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> CreateAsync([FromBody] ProductInfoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest();
            }

            if (dto.Price <= 0)
            {
                return BadRequest();
            }

            var rawProduct = _mapper.Map<Product>(dto);
            var product = await _productService.CreateProduct(rawProduct);

            var result = _mapper.Map<ProductDto>(product);
            return result;
        }
        
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateAsync([FromRoute] Guid id, [FromBody] ProductInfoDto dto)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest();
            }

            if (dto.Price <= 0)
            {
                return BadRequest();
            }

            var product = await _productService.GetProductById(id);

            if (product == null)
            {
                return NotFound();
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;

            await _productService.UpdateProduct(product);

            var result = _mapper.Map<ProductDto>(product);

            return result;
        }
        
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("{id}")]
        public async Task<ActionResult<ProductDto>> DeleteAsync([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            await _productService.DeleteProduct(id);
            return Ok();
        }
    }
}
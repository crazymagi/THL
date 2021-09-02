using System;
using AutoMapper;
using THL.Domain;

namespace THL.WebApi.Dtos
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Product, ProductDto>();
            CreateMap<ProductInfoDto, Product>();
        }
    }
}
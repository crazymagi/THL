using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using THL.Bll.Services;
using THL.DataAccess;
using THL.Domain;
using THL.WebApi.Dtos;

namespace THL.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddAutoMapper(typeof(MapperProfile));
            services.AddDbContext<IThlDbContext, ThlDbContext>(o => o.UseInMemoryDatabase("ThlDatabase"));
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "THL.WebApi", Version = "v1"}); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "THL.WebApi v1"));
            }
            

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            
            // Seed(app);

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
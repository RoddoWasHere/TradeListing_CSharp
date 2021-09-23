using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.HttpsPolicy;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.OpenApi.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TradingAPI.Data;
using TradingAPI.Schema;
using TradingAPI.Controllers;
using HotChocolate;


using System;

using Microsoft.AspNetCore.Http;

using HotChocolate.AspNetCore;
using HotChocolate.Execution.Configuration;

namespace TradingAPI
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

            services.AddDbContext<MyDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("Default")));

            services.AddControllers();
            services.AddDbContext<TodoContext>(opt =>
                                   opt.UseInMemoryDatabase("TodoList"));


            //services.AddPooledDbContextFactory<MyDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("Default")));

            services
                .AddGraphQLServer()
                .AddProjections()
                .AddQueryType<Query>();
            //services.AddGraphQL(
            //    SchemaBuilder.New()
            //        .AddQueryType<Query>()
            //        .Create(),
            //    new QueryExecutionOptions { ForceSerialExecution = true });

            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TradingAPI", Version = "v1" });
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TradingAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGraphQL();
            });
        }
    }
}

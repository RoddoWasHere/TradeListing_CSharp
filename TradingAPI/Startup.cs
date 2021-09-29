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
using System.Threading.Tasks;
using System.Linq;
using Binance.Net.Enums;

namespace TradingAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:8080")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });

            services.AddDbContext<MyDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("Default")));

            services.AddControllers();
            services.AddDbContext<TodoContext>(opt =>
                                   opt.UseInMemoryDatabase("TodoList"));



            //services.AddPooledDbContextFactory<MyDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("Default")));

            services
                .AddGraphQLServer()
                .AddSorting()
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
            PopulateTestData(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TradingAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGraphQL();
            });
        }

        private static void PopulateTestData(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<MyDbContext>();
                //if (context.Database.EnsureCreated())
                //{
                    Console.WriteLine("Got count: " + context.Instrument.Count());
                    if (context.Instrument.Count() == 0)//populate if empty 
                    {

                        var btcInstr = new Instrument
                        {
                            Symbol = "BTC",
                            Name = "Bitcoin",
                        };

                        var usdtInstr = new Instrument
                        {
                            Symbol = "USDT",
                            Name = "USD Tether",
                        };

                        var pair = new InstrumentPair
                        {
                            Symbol = "BTCUSDT",
                            BaseInstrument = btcInstr,
                            QuoteInstrument = usdtInstr,
                        };


                        var priceHistory1 = new PriceHistory
                        {
                            InstrumentPair = pair,
                            Interval = KlineInterval.FifteenMinutes,
                            UtcOpenTime = 1632748500000,//15 min rounded
                            UtcCloseTime = 1632749400000,
                            High = 4,
                            Low = 1,
                            Open = 2,
                            Close = 3,
                        };

                        var priceHistory2 = new PriceHistory {
                            InstrumentPair = pair,
                            Interval = KlineInterval.FifteenMinutes,
                            UtcOpenTime = 1632749400000,//15 min rounded (+15 mins)
                            UtcCloseTime = 1632750300000,
                            High = 5,
                            Low = 2,
                            Open = 3,
                            Close = 4,
                        };
                    //--set 2

                    //var btcInstr = new Instrument
                    //{
                    //    Symbol = "BTC",
                    //    SymbolName = "Bitcoin",
                    //};

                    var ethInstr = new Instrument
                    {
                        Symbol = "ETH",
                        Name = "Etheream",
                    };

                    var pair2 = new InstrumentPair
                    {
                        Symbol = "ETHBTC",
                        BaseInstrument = ethInstr,
                        QuoteInstrument = btcInstr,
                    };

                    var priceHistory3 = new PriceHistory
                    {
                        InstrumentPair = pair2,
                        Interval = KlineInterval.FifteenMinutes,
                        UtcOpenTime = 1632748500000,//15 min rounded
                        UtcCloseTime = 1632749400000,
                        High = 40,
                        Low = 10,
                        Open = 20,
                        Close = 30,
                    };

                    var priceHistory4 = new PriceHistory
                    {
                        InstrumentPair = pair2,
                        Interval = KlineInterval.FifteenMinutes,
                        UtcOpenTime = 1632749400000,//15 min rounded (+15 mins)
                        UtcCloseTime = 1632750300000,
                        High = 50,
                        Low = 20,
                        Open = 30,
                        Close = 40,
                    };


                    //var course = new Course { Credits = 10, Title = "Object Oriented Programming 1" };

                    //context.InstrumentPair.Add(pair);
                    context.PriceHistory.Add(priceHistory1);
                    context.PriceHistory.Add(priceHistory2);
                    context.PriceHistory.Add(priceHistory3);
                    context.PriceHistory.Add(priceHistory4);
                        context.SaveChangesAsync();

                        Console.WriteLine("Populated DB");
                    }
                    //----

                    //context.Enrollments.Add(new Enrollment
                    //{
                    //    Course = course,
                    //    Student = new Student { FirstMidName = "Rafael", LastName = "Foo", EnrollmentDate = DateTime.UtcNow }
                    //});
                    //context.Enrollments.Add(new Enrollment
                    //{
                    //    Course = course,
                    //    Student = new Student { FirstMidName = "Pascal", LastName = "Bar", EnrollmentDate = DateTime.UtcNow }
                    //});
                    //context.Enrollments.Add(new Enrollment
                    //{
                    //    Course = course,
                    //    Student = new Student { FirstMidName = "Michael", LastName = "Baz", EnrollmentDate = DateTime.UtcNow }
                    //});
                    //context.SaveChangesAsync();
                //}
            }
        }
    }
}

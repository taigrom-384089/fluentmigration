using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using System;
using WebAppWithNHibernateCore.Migrations;

namespace WebAppWithNHibernateCore
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
            var migrator = services.AddFluentMigratorCore()
                .ConfigureRunner(
                         builder => builder
                           .AddSqlServer2016()
                           .WithGlobalConnectionString("Data Source=TAIGROM\\SQLEXPRESS;Initial Catalog=Aprovacao;Integrated Security=SSPI;")
                          // .ScanIn(typeof(object).Assembly).For.Migrations())
                          .ScanIn(typeof(CreateCityTable).Assembly).For.Migrations())
                          
                           .BuildServiceProvider(false);


            var runner = migrator.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();

            services.AddSingleton<ISessionFactory>((provider) => {
                return Fluently.Configure()
                 .Database(MsSqlConfiguration.MsSql2012.ShowSql()
                 .ConnectionString("Data Source=TAIGROM\\SQLEXPRESS;Initial Catalog=Aprovacao;Integrated Security=SSPI;"))
                 //.Mappings(m => entityMappingTypes.ForEach(e => { m.FluentMappings.Add(e); }))
                 .Mappings(m => m.FluentMappings.AddFromAssemblyOf<NHibernate.Cfg.Mappings>())
                 .CurrentSessionContext("call")
                 //.ExposeConfiguration(cfg => BuildSchema(cfg, create, update))
                 .BuildSessionFactory();
            });

            services.AddScoped<ISession>((provider) => {
                var factory = provider.GetService<ISessionFactory>();
                return factory.OpenSession();
            });
            
            services.AddMvc();
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}

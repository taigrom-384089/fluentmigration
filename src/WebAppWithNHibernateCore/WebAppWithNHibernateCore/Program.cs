using FluentMigrator.Runner;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace WebAppWithNHibernateCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();

            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            //using (var scope = CreateServices().CreateScope())
            //{
            //    UpdateDatabase(scope.ServiceProvider);
            //}
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        /// <summary>
        /// Configure the dependency injection services
        /// </sumamry>
        private static IServiceProvider CreateServices()
        {
            return new ServiceCollection()
                // Add common FluentMigrator services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    // Add SQLite support to FluentMigrator
                    .AddSqlServer2016()
                    // Set the connection string
                    .WithGlobalConnectionString("Data Source=TAIGROM\\SQLEXPRESS;Initial Catalog=Aprovacao;Integrated Security=SSPI;")
                    // Define the assembly containing the migrations
                    .ScanIn(typeof(object).Assembly).For.Migrations())
                // Enable logging to console in the FluentMigrator way
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                // Build the service provider
                .BuildServiceProvider(false);
        }

        /// <summary>
        /// Update the database
        /// </sumamry>
        private static void UpdateDatabase(IServiceProvider serviceProvider)
        {
            // Instantiate the runner
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            // Execute the migrations
            runner.MigrateUp();
        }


    }
}

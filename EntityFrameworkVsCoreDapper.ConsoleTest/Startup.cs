﻿using EntityFrameworkVsCoreDapper.ConsoleTest.Helpers;
using EntityFrameworkVsCoreDapper.ConsoleTest.Tests;
using EntityFrameworkVsCoreDapper.Context;
using EntityFrameworkVsCoreDapper.EntityFramework;
using EntityFrameworkVsCoreDapper.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkVsCoreDapper.ConsoleTest
{
    public class Startup
    {
        public ServiceProvider Initialize()
        {
            IServiceCollection services = new ServiceCollection();
            Register(services);
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        public void Register(IServiceCollection services)
        {
            services.AddDbContext<DotNetCoreContext>(_ => _.UseSqlServer(@"Data Source=(LocalDB)\MSSQLLocalDB; 
                           Initial Catalog=CamparationEntityDapper; Integrated Security=True"), ServiceLifetime.Transient);
            services.AddTransient<DapperContext>();
            services.AddScoped<Ef6Context>();
            services.AddTransient<IInserts, Inserts>();
            services.AddTransient<ISelects, Selects>();
            services.AddTransient<IDapperTests, DapperTests>();
            services.AddTransient<IEfCoreTests, EfCoreTests>();
            services.AddTransient<IEf6Tests, Ef6Tests>();
            services.AddTransient<ConsoleHelper>();
            services.AddSingleton<ResultService>();
        }
    }
}

﻿using EntityFrameworkVsCoreDapper.ConsoleTest.Helpers;
using EntityFrameworkVsCoreDapper.EntityFramework;
using EntityFrameworkVsCoreDapper.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EntityFrameworkVsCoreDapper.ConsoleTest
{
    public class EfCoreTests : IEfCoreTests
    {
        private readonly DotNetCoreContext _netcoreContext;
        private readonly ConsoleHelper _consoleHelper;
        private readonly ResultService _resultService;
        public EfCoreTests(DotNetCoreContext netcoreContext, ConsoleHelper consoleHelper, ResultService resultService)
        {
            _netcoreContext = netcoreContext;
            _consoleHelper = consoleHelper;
            _resultService = resultService;
        }

        public TimeSpan InsertAvg(int interactions)
        {
            var tempo = TimeSpan.Zero;

            for (int i = 0; i < 10; i++)
            {
                var watch = _consoleHelper.StartChrono();

                new ListTests().ObtenirListCustomersAleatoire(interactions).ForEach(_ => _netcoreContext.Customers.Add(_));
                _netcoreContext.SaveChanges();

                watch.Watch.Stop();
                tempo += watch.Watch.Elapsed;
            }

            return _consoleHelper.DisplayChrono(tempo / 10, "EFCore");
        }
        public TimeSpan InsertAvgAsNoTracking(int interactions)
        {
            var tempo = TimeSpan.Zero;

            for (int i = 0; i < 10; i++)
            {
                var watch = _consoleHelper.StartChrono();

                _netcoreContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                new ListTests().ObtenirListCustomersAleatoire(interactions).ForEach(_ => _netcoreContext.Customers.Add(_));
                _netcoreContext.SaveChanges();

                watch.Watch.Stop();
                tempo += watch.Watch.Elapsed;
            }

            return _consoleHelper.DisplayChrono(tempo / 10, "EFCore");
        }
        public TimeSpan AddCustomersSingles(int interactions)
        {
            var watch = _consoleHelper.StartChrono();

            new ListTests().ObtenirListCustomersSingles(interactions).ForEach(_ => _netcoreContext.Add(_));
            _netcoreContext.SaveChanges();

            return _consoleHelper.StopChrono(watch, "EFCore").Tempo;
        }
        public TimeSpan AddCustomersSinglesAsNoTracking(int interactions)
        {
            var watch = _consoleHelper.StartChrono();

            _netcoreContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            new ListTests().ObtenirListCustomersSingles(interactions).ForEach(_ => _netcoreContext.Add(_));
            _netcoreContext.SaveChanges();

            return _consoleHelper.StopChrono(watch, "EFCore").Tempo;
        }
        public TimeSpan AjouterCustomersAleatoires(int interactions)
        {
            var watch = _consoleHelper.StartChrono();

            new ListTests().ObtenirListCustomersAleatoire(interactions).ForEach(_ => _netcoreContext.AddAsync(_));
            _netcoreContext.SaveChanges();
            var tempoResult = _consoleHelper.StopChrono(watch, "EFCore").Tempo;
            _resultService.SaveSelect(interactions, tempoResult, watch.InitMemory, TypeTransaction.EfCore, OperationType.InsertComplex);
            return tempoResult;
        }
        public TimeSpan AjouterCustomersAleatoiresAsNoTracking(int interactions)
        {
            var watch = _consoleHelper.StartChrono();

            _netcoreContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            new ListTests().ObtenirListCustomersAleatoire(interactions).ForEach(_ => _netcoreContext.Add(_));
            _netcoreContext.SaveChanges();

            var tempoResult = _consoleHelper.StopChrono(watch, "EFCore AsNoTracking").Tempo;
            _resultService.SaveSelect(interactions, tempoResult, watch.InitMemory, TypeTransaction.EfCoreAsNoTracking, OperationType.InsertComplex);
            return tempoResult;
        }
        public TimeSpan AjouterCustomersAleatoiresOpenClose(int interactions)
        {
            var watch = _consoleHelper.StartChrono();

            foreach (var item in new ListTests().ObtenirListCustomersAleatoire(interactions))
            {
                _netcoreContext.Add(item);
                _netcoreContext.SaveChanges();
            }

            return _consoleHelper.StopChrono(watch, "EFCore").Tempo;
        }

        public TimeSpan AjouterCustomersAleatoiresAsNoTrackingOpenClose(int interactions)
        {
            var watch = _consoleHelper.StartChrono();

            foreach (var item in new ListTests().ObtenirListCustomersAleatoire(interactions))
            {
                _netcoreContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                _netcoreContext.Add(item);
                _netcoreContext.SaveChanges();
            }

            return _consoleHelper.StopChrono(watch, "EFCore AsNoTracking").Tempo;
        }
        public TimeSpan SelectProductsSingles(int take)
        {
            var watch = _consoleHelper.StartChrono();

            var teste = _netcoreContext.Products.Take(take).ToList();
            var tempoResult = _consoleHelper.StopChrono(watch, "EF Core single select").Tempo;

            watch.Watch.Stop();

            _resultService.SaveSelect(take, tempoResult, watch.InitMemory, TypeTransaction.EfCore, OperationType.SelectSingle);

            return tempoResult;
        }

        public TimeSpan SelectProductsSinglesAsNoTracking(int take)
        {
            var watch = _consoleHelper.StartChrono();

            _netcoreContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            var teste = _netcoreContext.Products.Take(take).ToList();

            var tempoResult = _consoleHelper.StopChrono(watch, "EF Core single select AsNoTracking").Tempo;

            _resultService.SaveSelect(take, tempoResult, watch.InitMemory, TypeTransaction.EfCoreAsNoTracking, OperationType.SelectSingle);

            return tempoResult;
        }
        public TimeSpan SelectProductsSinglesAsNoTrackingHardSql(int take)
        {
            var watch = _consoleHelper.StartChrono();

            _netcoreContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            var teste = _netcoreContext.Products.FromSqlRaw($"select top({take}) * from products").ToList();

            var tempoResult = _consoleHelper.StopChrono(watch, "EF Core single select AsNoTracking SqlHard").Tempo;
            _resultService.SaveSelect(take, tempoResult, watch.InitMemory, TypeTransaction.EfCoreAsNoTrackingSqlHard, OperationType.SelectSingle);
            return tempoResult;
        }
        public TimeSpan SelectCustomers(int take)
        {
            var watch = _consoleHelper.StartChrono();

            var teste = _netcoreContext.Customers
                .Include(_ => _.Address)
                .Include(_ => _.Products)
                .Take(take).ToList();
            var tempoResult = _consoleHelper.StopChrono(watch, "EF Core").Tempo;
            _resultService.SaveSelect(take, tempoResult, watch.InitMemory, TypeTransaction.EfCore, OperationType.SelectComplex);
            return tempoResult;
        }
        public TimeSpan SelectCustomersAsNoTracking(int take)
        {
            var watch = _consoleHelper.StartChrono();

            _netcoreContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            var teste = _netcoreContext.Customers
                .Where(_ => _.Address.City.StartsWith("North") && _.Products.Count(_ => _.Brand == "Intelligent") > 0)
              .Include(_ => _.Address)
              .Include(_ => _.Products)
              .Take(take)
              .ToList();

            var tempoResult = _consoleHelper.StopChrono(watch, "EF Core AsNoTracking").Tempo;
            _resultService.SaveSelect(take, tempoResult, watch.InitMemory, TypeTransaction.EfCoreAsNoTracking, OperationType.SelectComplex);
            return tempoResult;
        }

        public TimeSpan InsertProductsSingles(int interactions)
        {
            var watch = _consoleHelper.StartChrono();

            new ListTests().ObtenirListProductsAleatoire(interactions, null).ForEach(_ => _netcoreContext.Products.Add(_));
            _netcoreContext.SaveChanges();

            var tempoResult = _consoleHelper.StopChrono(watch, "EF Core").Tempo;
            _resultService.SaveSelect(interactions, tempoResult, watch.InitMemory, TypeTransaction.EfCore, OperationType.InsertSingle);
            return tempoResult;
        }

        public TimeSpan InsertProductsSinglesAsNoTracking(int interactions)
        {
            var watch = _consoleHelper.StartChrono();
           

            _netcoreContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            new ListTests().ObtenirListProductsAleatoire(interactions, null).ForEach(_ => _netcoreContext.Products.Add(_));
            _netcoreContext.SaveChanges();

            var tempoResult = _consoleHelper.StopChrono(watch, "EF Core").Tempo;
            _resultService.SaveSelect(interactions, tempoResult, watch.InitMemory, TypeTransaction.EfCoreAsNoTracking, OperationType.InsertSingle);
            return tempoResult;
        }

        public TimeSpan InsertProductSingleAsNoTrackingHardSql(int interactions)
        {
            var watch = _consoleHelper.StartChrono();
            

            _netcoreContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            AddProducts(new ListTests().ObtenirListProductsAleatoire(interactions, null));
            _netcoreContext.SaveChanges();

            var tempoResult = _consoleHelper.StopChrono(watch, "EF Core").Tempo;
            _resultService.SaveSelect(interactions, tempoResult, watch.InitMemory, TypeTransaction.EfCoreAsNoTrackingSqlHard, OperationType.InsertSingle);
            return tempoResult;
        }

        public TimeSpan InsertCustomerSingleAsNotrackingHardSql(int interactions)
        {
            var watch = _consoleHelper.StartChrono();
            

            _netcoreContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            AddCustomers(new ListTests().ObtenirListCustomersAleatoire(interactions));
            _netcoreContext.SaveChanges();

            var tempoResult = _consoleHelper.StopChrono(watch, "EF Core").Tempo;
            _resultService.SaveSelect(interactions, tempoResult, watch.InitMemory, TypeTransaction.EfCoreAsNoTrackingSqlHard, OperationType.InsertComplex);
            return tempoResult;
        }

        public void AddCustomers(IEnumerable<Customer> customers)
        {
            foreach (var customer in customers)
            {
                AddAddress(customer.Address);
                AddCustomer(customer);

                foreach (var product in customer.Products)
                    AddProduct(product);
            }
        }

        public void AddProducts(IEnumerable<Product> products)
        {
            foreach (var product in products)
                AddProduct(product);
        }

        public void AddProduct(Product product)
        {
            var sql = "INSERT INTO PRODUCTS (Id, Name, Description, Price, OldPrice, Brand, CustomerId) Values" +
                $"('{product.Id}', '{product.Name}', '{product.Description}', {product.Price.ToString(CultureInfo.CreateSpecificCulture("en-US"))}, " +
                $"{product.OldPrice.ToString(CultureInfo.CreateSpecificCulture("en-US"))}, '{product.Brand}', {FormatCustomer(product.CustomerId)})";
            _netcoreContext.Database.ExecuteSqlCommand(sql);
        }

        private string FormatCustomer(Guid? id)
        {
            if (id == null)
                return "null";
            return $"'{id}'";
        }
        public void AddAddress(Address address)
        {
            var sql = "Insert into Address (Id, Number, Street, City, Country, ZipCode, AdministrativeRegion) Values" +
                $"('{address.Id}','{address.Number}', '{address.Street.Replace("'", "")}', '{address.City.Replace("'", "")}', '{address.Country.Replace("'", "")}', '{address.ZipCode}', '{address.AdministrativeRegion}')";
            _netcoreContext.Database.ExecuteSqlCommand(sql);
        }
        public void AddCustomer(Customer customer)
        {
            var sql = "Insert into Customers (Id, FirstName, LastName, Email, Status, BirthDate, AddressId) Values" +
                $"('{customer.Id}', '{customer.FirstName.Replace("'", "")}', '{customer.LastName.Replace("'", "")}', '{customer.Email}', '{customer.Status}', '{customer.BirthDate}', '{customer.AddressId}')";
            _netcoreContext.Database.ExecuteSqlCommand(sql);
        }
    }
}

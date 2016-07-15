using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace GettingStartedWithMongoDB
{
    class Program
    {
        static void Main(string[] args)
        {
            // Mapping should be done early, before a connection with Mongo has been established.
            // Comment this line if you want to use the default mapping
            DefineClassMaps();

            // Establish connection using a Mongo client. It is threadsafe
            // and handles server connections. It is
            // reccomended to store it in a single place (singleton)
            var client = new MongoClient("mongodb://localhost:27017/EmployeeDB");

            // Get an instance to a databse. If it does not exist, 
            // it will be created automatically for us. 
            var database = client.GetDatabase("EmployeeDB");

            Console.Out.WriteLine("Connection has been established.\n");

            // Get collection. If it does not exist it shall be created automatically
            var employeeCollection = database.GetCollection<Employee>("Employees", null);


            // Before each execution, we shall delete all elements, in order 
            // to start from a clean database
            database.DropCollection("Employees");

            // Insert a single entry
            employeeCollection.InsertOne(new Employee("Peter", new DateTime(1980, 1, 20), 2000, Department.Management));

            // Insert multiple entries in a single batch
            Employee martha = new Employee("Martha", new DateTime(1990,3,23), 3000, Department.HumanResources);
            Employee andrew = new Employee("Andrew", new DateTime(1988,5,6), 4000, Department.Management);
            employeeCollection.InsertMany(new List<Employee> { martha, andrew });

            // Count existing number of records
            int totalEmployees = employeeCollection.AsQueryable().Count();

            // Applying various filters using LINQ
            List<Employee> employeesWithHighIncome = employeeCollection.AsQueryable()
                .Where(x => x.MonthlyIncome > 2500)
                .OrderBy(x => x.MonthlyIncome)
                .ToList();

            List<Employee> employeesWhoAreManagers = employeeCollection.AsQueryable()
                .Where(x => x.Department == Department.Management)
                .ToList();

            List<Employee> youngEmployees = employeeCollection.AsQueryable()
                .Where(x => x.BirthDate > new DateTime(1989, 01, 01))
                .ToList();

            Employee employeeById = employeeCollection
                .AsQueryable()
                .SingleOrDefault(x => x.Id == martha.Id);

            // Update an employee
            martha.MonthlyIncome += 200;   
            employeeCollection.ReplaceOne(x => x.Id == martha.Id, martha);

            // Delete an employee
            employeeCollection.DeleteOne(x => x.Id == andrew.Id);

            Console.ReadLine();
        }

        private static void DefineClassMaps()
        {
            // Mapping should be done once. Check if the class has
            // already been registered
            if (!BsonClassMap.IsClassMapRegistered(typeof(Employee)))
            {
                BsonClassMap.RegisterClassMap<Employee>(x =>
                {
                    x.AutoMap();
                    // Unmap this property. It will not be persisted in the db
                    x.UnmapProperty(e => e.Name);
                    // Change name of property in the actual db from MonthlyIncome to Income
                    x.MapProperty(e => e.MonthlyIncome).SetElementName("Income");
                });
            }
        }
    }
}

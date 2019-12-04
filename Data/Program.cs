namespace ent.manager.Data.Test
{
    using System;
    using Microsoft.Extensions.Configuration;

    using ent.manager.Entity.Model;
    using ent.manager.Data.Context;

    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            string connectionString = configuration.GetConnectionString("managerConnection");

            // Create an employee instance and save the entity to the database
            Employee employee = new Employee();
            employee.Name = DateTime.Now.ToString();

            Random random = new Random();
            int randomNumber = random.Next(0, 100);

            employee.Id = randomNumber;

            using (var context = ContextFactory.Create(connectionString))
            {
                context.Add(employee);
                context.SaveChanges();
            }

           
        }
    }
}
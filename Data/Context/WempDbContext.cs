namespace ent.manager.Data.Context
{
    using Microsoft.EntityFrameworkCore;
    using ent.manager.Entity.Model;

    /// <summary>
    /// The entity framework context with a Employees DbSet
    /// </summary>
    public class managerDbContext : DbContext
    {
        public managerDbContext(DbContextOptions<managerDbContext> options)
        : base(options)
        { }


        public DbSet<Partner> Partner { get; set; }
        public DbSet<ent.manager.Entity.Model.EnterpriseClient> EnterpriseClient { get; set; }
        public DbSet<ent.manager.Entity.Model.Subscription> Subscription { get; set; }
        public DbSet<ent.manager.Entity.Model.User> User { get; set; }
        public DbSet<ent.manager.Entity.Model.Product> Product { get; set; }
        public DbSet<ent.manager.Entity.Model.LicenceEnvironment> LicenceEnvironment { get; set; }
        public DbSet<ent.manager.Entity.Model.UserData> UserData { get; set; }

        public DbSet<ent.manager.Entity.Model.Reporting.UsageReport> UsageReport { get; set; }
        public DbSet<ent.manager.Entity.Model.Reporting.DeviceOsReport> DeviceOsReport { get; set; }
        public DbSet<ent.manager.Entity.Model.Reporting.DeviceTypeReport> DeviceTypeReport { get; set; }
        public DbSet<ent.manager.Entity.Model.Reporting.DeviceManufacturerReport> DeviceManufacturerReport { get; set; }
        public DbSet<ent.manager.Entity.Model.Reporting.Report> Report { get; set; }
        public DbSet<ent.manager.Entity.Model.Reporting.ReportProcessorRun> ReportProcessorRun { get; set; }
        public DbSet<ent.manager.Entity.Model.Reporting.ReportType> ReportType { get; set; }
        
        public DbSet<ent.manager.Entity.Model.Reporting.SeatDetailsReport> SeatDetailsReport { get; set; }
        public DbSet<ent.manager.Entity.Model.DeviceModel> DeviceModel { get; set; }
        public DbSet<ent.manager.Entity.Model.DeviceType> DeviceType { get; set; }
        public DbSet<ent.manager.Entity.Model.EKey> EKey { get; set; }

        public DbSet<ent.manager.Entity.Model.UserDataTemp> UserDataTemp { get; set; }
        public DbSet<ent.manager.Entity.Model.SubscriptionAuth> SubscriptionAuth { get; set; }

        public DbSet<Log> Log { get; set; }

    }

    /// <summary>
    /// Factory class for EmployeesContext
    /// </summary>
    public static class ContextFactory
    {
        public static managerDbContext Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<managerDbContext>();
            optionsBuilder.UseMySQL(connectionString);

            //Ensure database creation
            var context = new managerDbContext(optionsBuilder.Options);
            context.Database.EnsureCreated();

            return context;
        }
    }

    /// <summary>
    /// A basic class for an Employee
    /// </summary>
  
}
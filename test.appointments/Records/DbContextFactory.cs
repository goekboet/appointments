using Appointments.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Test.Records
{
    public class TestContextFactory : IDesignTimeDbContextFactory<Pgres>
    {
        public static string Cs = "Host=localhost;Database=test_appointments;Username=appointments;Password=nZva7sMNkdF7MBF5";
        public Pgres CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<Pgres>();
            optionsBuilder.UseNpgsql(Cs);

            return new Pgres(optionsBuilder.Options);
        }
    }
}
using Microsoft.EntityFrameworkCore;

namespace Appointments.Records
{
    public class Pgres : DbContext
    {
        public Pgres(DbContextOptions<Pgres> opts) : base(opts) { }
        public DbSet<Schedule> Schedules { get; set; }

        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SchedulesTable());
            modelBuilder.ApplyConfiguration(new AppointmentsTable());
            modelBuilder.ApplyConfiguration(new ParticipantsTable());
        }
    }
}
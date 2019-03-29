using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Appointments.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
            modelBuilder.ApplyConfiguration(new AppointmentTable());
            modelBuilder.ApplyConfiguration(new ParticipantsTable());
        }
    }

    public class PgresRepo : IScheduleRepository
    {
        public PgresRepo(
            Pgres ctx)
        {
            _ctx = ctx;
        }

        Pgres _ctx;

        public Task<int> Add(Schedule schedule)
        {
            _ctx.Schedules.Add(schedule);
            return _ctx.SaveChangesAsync();
        }

        public Task Delete(Schedule schedule)
        {
            _ctx.Remove(schedule);
            return _ctx.SaveChangesAsync();

        }

        public Task<string[]> List(Guid principalId)
        {
            var q = from s in _ctx.Schedules
                    where s.PrincipalId == principalId
                    select s.Name;

            return q.ToArrayAsync();
        }

        public Task<Schedule> Get(
            Guid principalId,
            string name)
        {
            var q = from s in _ctx.Schedules
                    where s.PrincipalId == principalId
                          && s.Name == name
                    select s;

            return q.SingleOrDefaultAsync();
        }
    }
}
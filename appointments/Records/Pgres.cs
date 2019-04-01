using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Appointments.Domain;
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
            modelBuilder.ApplyConfiguration(new AppointmentsTable());
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

        public Task<int> Add(Guid principalId, string name)
        {
            _ctx.Schedules.Add(new Schedule
            {
                PrincipalId = principalId,
                Name = name
            });

            return _ctx.SaveChangesAsync();
        }

        public Task Delete(Guid principalId, string name)
        {
            _ctx.Remove(new Schedule
            {
                PrincipalId = principalId,
                Name = name
            });

            return _ctx.SaveChangesAsync();

        }

        public async Task<object[]> List(Guid principalId)
        {
            var q = from s in _ctx.Schedules
                    where s.PrincipalId == principalId
                    orderby s.Name
                    select new { name = s.Name };

            return await q.ToArrayAsync();
        }

        public async Task<Booking[]> Get(
            Guid principalId,
            string name)
        {
            var q = from a in _ctx.Appointments
                    orderby a.Start
                    where a.Schedule.PrincipalId == principalId &&
                        a.ScheduleName == name
                    select new Booking
                    { 
                        Start = a.Start,
                        Duration = a.Duration,
                        Schedule = a.ScheduleName }
                    ;

            return await q.ToArrayAsync();
        }
    }

    public class ParticipantRepo : IParticipantRepository
    {
        public ParticipantRepo(
            Pgres ctx)
        {
            _ctx = ctx;
        }

        Pgres _ctx;
        public async Task<object[]> List(string subjectId)
        {
            var q = from a in _ctx.Appointments
                    orderby a.ScheduleName, a.Start
                    where a.Participants.Any(x => x.SubjectId == subjectId)
                    select new
                    {
                        schedule = a.ScheduleName,
                        start = a.Start,
                        duration = a.Duration
                    };

            return await q.ToArrayAsync();
        }

        public async Task<Participation[]> Get(
            string schedule,
            long start,
            string subjectId)
        {
            var q =
                from a in _ctx.Appointments
                from p in a.Participants
                orderby p.Name
                where
                    a.Schedule.Name == schedule &&
                    a.Start == start &&
                    a.Participants.Any(x => x.SubjectId == subjectId)
                select new Participation
                {
                    SubjectId = p.SubjectId,
                    Name = p.Name
                };

            return await q.ToArrayAsync();
        }

        public async Task Add(
            Guid principalId,
            string schedule,
            long start,
            int duration,
            Participation[] ps)
        {
            var q =
                from a in _ctx.Appointments
                where a.Schedule.PrincipalId == principalId &&
                    a.ScheduleName == schedule &&
                    a.Start == start
                select a;

            var participants =
                (from p in ps
                 select new Participant
                 {
                     SubjectId = p.SubjectId,
                     Name = p.Name
                 }).ToList();


            var appt = await q
                .Include(x => x.Participants)
                .SingleOrDefaultAsync();

            if (appt != null)
            {
                _ctx.Remove(appt);
            }

            var claim = await (
                from s in _ctx.Schedules
                where s.PrincipalId == principalId &&
                    s.Name == schedule
                select s)
                .Include(x => x.Appointments)
                .SingleOrDefaultAsync();

            if (claim == null)
                return; //The principalId does not own schedule

            claim.Appointments.Add(
                new Appointment
                {
                    ScheduleName = schedule,
                    Start = start,
                    Duration = duration,
                    Participants = participants
                });

            await _ctx.SaveChangesAsync();
        }

        public async Task Delete(
            string schedule, 
            long start, 
            string subjectId)
        {
            var q = 
                from a in _ctx.Appointments
                from p in a.Participants
                where a.ScheduleName == schedule &&
                    a.Start == start &&
                    p.SubjectId == subjectId
                select p;

            var participation = await q.SingleOrDefaultAsync();
            
            if (participation != null)
            {
                _ctx.Remove(participation);
                await _ctx.SaveChangesAsync();    
            }
        }
    }
}
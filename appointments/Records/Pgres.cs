using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Appointments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Domain = Appointments.Domain;

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

        public Task<int> Add(
            PrincipalClaim claim)
        {
            _ctx.Schedules.Add(new Schedule
            {
                PrincipalId = claim.Id,
                Name = claim.Schedule
            });

            return _ctx.SaveChangesAsync();
        }

        public Task Delete(PrincipalClaim c)
        {
            _ctx.Remove(new Schedule
            {
                PrincipalId = c.Id,
                Name = c.Schedule
            });

            return _ctx.SaveChangesAsync();

        }

        public async Task<Domain.Schedule[]> List(
            PrincipalClaim c)
        {
            var q = from s in _ctx.Schedules
                    where s.PrincipalId == c.Id
                    orderby s.Name
                    select new Domain.Schedule
                    {
                        PrincipalId = s.PrincipalId,
                        Name = s.Name
                    };

            return await q.ToArrayAsync();
        }

        public async Task<Domain.Appointment[]> Get(
            PrincipalClaim c)
        {
            var q = from a in _ctx.Appointments
                    orderby a.Start
                    where a.Schedule.PrincipalId == c.Id &&
                        a.ScheduleName == c.Schedule
                    select new Domain.Appointment
                    { 
                        Start = a.Start,
                        Duration = a.Duration,
                        Participants = a.Participants
                            .Select(x => new Domain.Participant
                            {
                                SubjectId = x.SubjectId,
                                Name = x.Name
                            })
                            .ToList() 
                    };

            return await q.ToArrayAsync();
        }

        public void Dispose() => _ctx.Dispose();
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

        public async Task<Domain.Participant[]> Get(
            ParticipantClaim c)
        {
            var q =
                from a in _ctx.Appointments
                from p in a.Participants
                orderby p.Name
                where
                    a.Schedule.Name == c.Schedule &&
                    a.Start == c.Start &&
                    a.Participants.Any(x => x.SubjectId == c.SubjectId)
                select new Domain.Participant
                {
                    SubjectId = p.SubjectId,
                    Name = p.Name
                };

            return await q.ToArrayAsync();
        }

        public async Task Add(
            PrincipalClaim c,
            Domain.Appointment ap)
        {
            var q =
                from a in _ctx.Appointments
                where a.Schedule.PrincipalId == c.Id &&
                    a.ScheduleName == c.Schedule &&
                    a.Start == ap.Start                
                select a;

            var participants =
                (from p in ap.Participants
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
                where s.PrincipalId == c.Id &&
                    s.Name == c.Schedule
                select s)
                .Include(x => x.Appointments)
                .SingleOrDefaultAsync();

            if (claim == null)
                return; //The principalId does not own schedule

            claim.Appointments.Add(
                new Appointment
                {
                    ScheduleName = c.Schedule,
                    Start = ap.Start,
                    Duration = ap.Duration,
                    Participants = participants
                });

            await _ctx.SaveChangesAsync();
        }

        public async Task Delete(
            ParticipantClaim c)
        {
            var q = 
                from a in _ctx.Appointments
                from p in a.Participants
                where a.ScheduleName == c.Schedule &&
                    a.Start == c.Start &&
                    p.SubjectId == c.SubjectId
                select p;

            var participation = await q.SingleOrDefaultAsync();
            
            if (participation != null)
            {
                _ctx.Remove(participation);
                await _ctx.SaveChangesAsync();    
            }
        }

        public void Dispose() => _ctx.Dispose();
    }
}
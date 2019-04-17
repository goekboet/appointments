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

        public async Task Add(
            PrincipalClaim claim)
        {
            var added = await (
                from s in _ctx.Schedules
                where s.PrincipalId == claim.Id &&
                      s.Name == claim.Schedule
                select s).AnyAsync();

            if (!added)
            {
                _ctx.Schedules.Add(new Schedule
                {
                    PrincipalId = claim.Id,
                    Name = claim.Schedule
                });

                await _ctx.SaveChangesAsync();
            }
        }

        public async Task Delete(PrincipalClaim c)
        {
            var added = await (
                from s in _ctx.Schedules
                where s.PrincipalId == c.Id &&
                      s.Name == c.Schedule
                select s).AnyAsync();

            if (added)
            {
                _ctx.Remove(new Schedule
                {
                    PrincipalId = c.Id,
                    Name = c.Schedule
                });

                await _ctx.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Domain.PrincipalClaim>> List(
            Guid id)
        {
            var q = from s in _ctx.Schedules
                    where s.PrincipalId == id
                    orderby s.Name
                    select new
                    {
                        Id = s.PrincipalId,
                        Schedule = s.Name
                    };
            var data = await q.ToArrayAsync();

            return data.Select(x => new Domain.PrincipalClaim(x.Id, x.Schedule));
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

        public async Task<AppointmentEvent> PutAppointment(
            PrincipalClaim c,
            Domain.Appointment ap)
        {
            var claim = await (from s in _ctx.Schedules
                where s.PrincipalId == c.Id &&
                    s.Name == c.Schedule
                select s)
                    .SingleOrDefaultAsync();
                
            AppointmentEvent evt = null;
            if (claim != null)
            {
                var appt = await (from a in _ctx.Appointments
                    where a.ScheduleName == c.Schedule &&
                        a.Start == ap.Start
                    select a)
                        .Include(x => x.Participants)
                        .SingleOrDefaultAsync();
                
                if (appt != null)
                {
                    var before = new Domain.Appointment
                    {
                        Start = appt.Start,
                        Duration = appt.Duration,
                        Participants = appt.Participants
                            .Select(x => new Domain.Participant
                            {
                                SubjectId = x.SubjectId,
                                Name = x.Name
                            }).OrderBy(x => x.Name).ToList()
                    };

                    appt.Duration = ap.Duration;
                    appt.Participants = ap.Participants
                        .Select(x => new Participant
                        {
                            SubjectId = x.SubjectId,
                            Name = x.Name
                        }).ToList();

                    evt = new AppointmentEvent(
                        before,
                        ap);
                }
                else
                {
                    _ctx.Add(new Appointment
                    {
                        Schedule = claim,
                        Start = ap.Start,
                        Duration = ap.Duration,
                        Participants = ap.Participants
                            .Select(x => new Participant
                            {
                                SubjectId = x.SubjectId,
                                Name = x.Name
                            }).ToList()
                    });

                    evt = new AppointmentEvent(
                        null,
                        ap);
                }
                await _ctx.SaveChangesAsync();
            }
           
            return evt;
        }

        public Task<AppointmentEvent> DeleteAppointment(PrincipalClaim claim, long start)
        {
            throw new NotImplementedException();
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
        public async Task<ParticipantAppointment[]> List(string subjectId)
        {
            var q = from a in _ctx.Appointments
                    orderby a.ScheduleName, a.Start
                    where a.Participants.Any(x => x.SubjectId == subjectId)
                    select new Domain.ParticipantAppointment
                    {
                        Schedule = a.ScheduleName,
                        Start = a.Start,
                        Duration = a.Duration
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
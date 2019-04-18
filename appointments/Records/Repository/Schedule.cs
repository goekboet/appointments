using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using D = Appointments.Domain.Schedule;

namespace Appointments.Records.Repository
{
    public class PgresRepo : D.IScheduleRepository
    {
        public PgresRepo(
            Pgres ctx)
        {
            _ctx = ctx;
        }

        Pgres _ctx;

        public async Task<D.CreateScheduleResult> Add(
            D.PrincipalClaim claim)
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
                return D.CreateScheduleResult.Created;
            }

            return D.CreateScheduleResult.Conflict;
        }

        public async Task<IEnumerable<D.PrincipalClaim>> List(
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

            return data.Select(x => new D.PrincipalClaim(x.Id, x.Schedule));
        }

        public async Task<D.Appointment[]> Get(
            D.PrincipalClaim c)
        {
            var q = from a in _ctx.Appointments
                    orderby a.Start
                    where a.Schedule.PrincipalId == c.Id &&
                        a.ScheduleName == c.Schedule
                    select new D.Appointment
                    {
                        Start = a.Start,
                        Duration = a.Duration,
                        Participants = a.Participants
                            .Select(x => new D.Participation
                            {
                                SubjectId = x.SubjectId,
                                Name = x.Name
                            })
                            .ToList()
                    };

            return await q.ToArrayAsync();
        }

        public async Task<D.PostAppointmentResult> PostAppointment(
            D.PrincipalClaim c,
            D.Appointment ap)
        {
            var result = D.PostAppointmentResult.ClaimNotOnRecord;

            var claim = await (from s in _ctx.Schedules
                where s.PrincipalId == c.Id &&
                    s.Name == c.Schedule
                select s)
                    .SingleOrDefaultAsync();
                
            if (claim != null)
            {
                var appt = await (from a in _ctx.Appointments
                    where a.ScheduleName == c.Schedule &&
                        a.Start == ap.Start
                    select a).AnyAsync();
                
                if (!appt)
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

                    await _ctx.SaveChangesAsync();
                    result = D.PostAppointmentResult.Created;
                }
                else
                {
                    result = D.PostAppointmentResult.Conflict;
                }
            }
           
            return result;
        }

        public async Task<D.DeleteAppointmentResult> DeleteAppointment(
            D.PrincipalClaim c, 
            long start)
        {
            var r = new D.ClaimNotOnRecord() as D.DeleteAppointmentResult; 

            var claim = await (from s in _ctx.Schedules
                where s.PrincipalId == c.Id &&
                    s.Name == c.Schedule
                select s)
                    .SingleOrDefaultAsync();

            if (claim != null)
            {
                var appt = await (from a in _ctx.Appointments
                    where a.ScheduleName == c.Schedule &&
                        a.Start == start
                    select a)
                    .Include(x => x.Participants)
                    .SingleOrDefaultAsync();
                
                if (appt == null)
                {
                    r = new D.AppointmentNotOnRecord();
                }
                else
                {
                    _ctx.Remove(appt);
                    await _ctx.SaveChangesAsync();

                    r = new D.Deleted(new D.Appointment
                    {
                        Start = appt.Start,
                        Duration = appt.Duration,
                        Participants = appt.Participants
                            .Select(x => new D.Participation
                            {
                                SubjectId = x.SubjectId,
                                Name = x.Name
                            })
                            .ToList()
                    });
                }
            }
            
            return r;
        }

        public void Dispose() => _ctx.Dispose();
    }
}
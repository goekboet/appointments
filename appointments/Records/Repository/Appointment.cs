using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using D = Appointments.Domain.Participant;

namespace Appointments.Records.Repository
{
    public class ParticipantRepo : D.IParticipantRepository
    {
        public ParticipantRepo(
            Pgres ctx)
        {
            _ctx = ctx;
        }

        Pgres _ctx;
        public async Task<D.Appointment[]> List(string subjectId)
        {
            var q = from a in _ctx.Appointments
                    orderby a.ScheduleName, a.Start
                    where a.Participants.Any(x => x.SubjectId == subjectId)
                    select new D.Appointment
                    {
                        Schedule = a.ScheduleName,
                        Start = a.Start,
                        Duration = a.Duration
                    };

            return await q.ToArrayAsync();
        }

        public async Task<D.Participation[]> Get(
            D.ParticipantClaim c)
        {
            var q =
                from a in _ctx.Appointments
                from p in a.Participants
                orderby p.Name
                where
                    a.Schedule.Name == c.Schedule &&
                    a.Start == c.Start &&
                    a.Participants.Any(x => x.SubjectId == c.SubjectId)
                select new D.Participation
                {
                    SubjectId = p.SubjectId,
                    Name = p.Name
                };

            return await q.ToArrayAsync();
        }

        public async Task Delete(
            D.ParticipantClaim c)
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
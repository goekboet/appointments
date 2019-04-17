using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Appointments.Domain
{
    public class PrincipalClaim
    {
        public PrincipalClaim(
            Guid id,
            string schedule = null)
        {
            Id = id;
            Schedule = schedule;
        }

        public Guid Id { get; }
        public string Schedule { get; }

        public override bool Equals(object obj) => 
            obj is PrincipalClaim s &&
            s.Id == Id &&
            s.Schedule == Schedule;

        public override int GetHashCode() => Id.GetHashCode();
    }

    public class AppointmentEvent 
    { 
        public static AppointmentEvent Empty() =>
            new AppointmentEvent(null, null);
        public AppointmentEvent(
            Appointment before,
            Appointment after)
        {
            Before = before;
            After = after;
        }
        public Appointment Before { get; }
        public Appointment After { get; }

        public override bool Equals(object obj) => 
            obj is AppointmentEvent e &&
                (e.Before == null ? Before == null : e.Before.Equals(Before)) &&
                (e.After == null ? After == null : e.After.Equals(After));

        public override int GetHashCode() => 
            Before.GetHashCode() ^ 
            After.GetHashCode();
    }
   

    public interface IScheduleRepository : IDisposable
    {
        Task Add(
            PrincipalClaim claim);

        Task<AppointmentEvent> PutAppointment(
            PrincipalClaim claim,
            Appointment appt);

        Task<AppointmentEvent> DeleteAppointment(
            PrincipalClaim claim,
            long start
        );

        Task<IEnumerable<PrincipalClaim>> List(Guid principalId);
        Task<Appointment[]> Get(PrincipalClaim s);
        Task Delete(PrincipalClaim s);
    }

    public class Appointment
    {
        public long Start { get; set; }
        public int Duration { get; set; }

        public List<Participant> Participants { get; set; } = 
            new List<Participant>();

        public override bool Equals(object obj) =>
            obj is Appointment a &&
                a.Start == Start;

        public override int GetHashCode() => Start.GetHashCode();    
    }

    public class Participant
    {
        public string SubjectId { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj) =>
            obj is Participant p &&
                p.Name == Name &&
                p.SubjectId == SubjectId;

        public override int GetHashCode() => $"{SubjectId}{Name}"
            .GetHashCode(); 
    }

    public class ParticipantClaim
    {
        public string Schedule { get; set; }
        public long Start { get; set; }
        public string SubjectId { get; set; }
    }

    public interface IParticipantRepository : IDisposable
    {
        Task<object[]> List(string subjectId);

        Task<Participant[]> Get(ParticipantClaim claim);

        

        Task Delete(ParticipantClaim claim);
    }
}
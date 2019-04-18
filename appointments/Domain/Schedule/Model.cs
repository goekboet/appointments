using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Appointments.Domain.Schedule
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

    public enum PostAppointmentResult
    {
        ClaimNotOnRecord,
        Conflict,
        Created
    }

    public abstract class DeleteAppointmentResult { }
    public class ClaimNotOnRecord : DeleteAppointmentResult 
    { 
        public override string ToString() => nameof(ClaimNotOnRecord);
    }
    public class AppointmentNotOnRecord : DeleteAppointmentResult
    { 
        public override string ToString() => nameof(AppointmentNotOnRecord);
    }
    public class Deleted : DeleteAppointmentResult 
    { 
        public Deleted(Appointment a)
        {
            Appointment = a;
        }
        public Appointment Appointment { get; }

        public override string ToString() => $"{nameof(Deleted)}: {Appointment}";
    }

    public enum CreateScheduleResult
    {
        Created,
        Conflict
    }

    public interface IScheduleRepository : IDisposable
    {
        Task<CreateScheduleResult> Add(
            PrincipalClaim claim);

        Task<PostAppointmentResult> PostAppointment(
            PrincipalClaim claim,
            Appointment appt);

        Task<DeleteAppointmentResult> DeleteAppointment(
            PrincipalClaim claim,
            long start
        );

        Task<IEnumerable<PrincipalClaim>> List(Guid principalId);
        Task<Appointment[]> Get(PrincipalClaim s);
    }

    public class Appointment
    {
        public long Start { get; set; }
        public int Duration { get; set; }

        public List<Participation> Participants { get; set; } =
            new List<Participation>();

        public override bool Equals(object obj) =>
            obj is Appointment a &&
                a.Start == Start;

        public override int GetHashCode() => Start.GetHashCode();
    }

    public class Participation
    {
        public string SubjectId { get; set; }
        public string Name { get; set; }

        public override string ToString() =>
            $"id: {SubjectId} n: {Name}";

        public override bool Equals(object obj) => 
            obj is Participation p &&
                p.Name == Name &&
                p.SubjectId == SubjectId;

        public override int GetHashCode() => this
            .ToString()
            .GetHashCode();
    }

    

    

    
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Appointments.Domain
{
    public class Schedule
    {
        public Guid PrincipalId { get;set;}
        public string Name {get;set;}

        public override bool Equals(object obj) => obj is Schedule s &&
            s.PrincipalId == PrincipalId &&
            s.Name == Name;

        public override int GetHashCode() => Name.GetHashCode();
    }

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
    }

    public interface IScheduleRepository : IDisposable
    {
        Task<int> Add(
            PrincipalClaim claim);
        Task<Schedule[]> List(PrincipalClaim principalId);
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

        Task Add(
            PrincipalClaim claim,
            Appointment appt);

        Task Delete(ParticipantClaim claim);
    }
}
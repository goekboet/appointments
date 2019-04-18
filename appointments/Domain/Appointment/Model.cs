using System;
using System.Threading.Tasks;

namespace Appointments.Domain.Participant
{
    public interface IParticipantRepository : IDisposable
    {
        Task<Appointment[]> List(string subjectId);

        Task<Participation[]> Get(ParticipantClaim claim);

        Task Delete(ParticipantClaim claim);
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

    public class Appointment
    {
        public string Schedule { get; set; }
        public long Start { get; set; }
        public int Duration { get; set; }

        public override string ToString() =>
            $"n: {Schedule} s: {Start} dur: {Duration}";
        
        public override bool Equals(object obj) =>
            obj != null && obj.ToString().Equals(this.ToString());

        public override int GetHashCode() => this.ToString().GetHashCode();

    }

    public class ParticipantClaim
    {
        public string Schedule { get; set; }
        public long Start { get; set; }
        public string SubjectId { get; set; }
    }
}
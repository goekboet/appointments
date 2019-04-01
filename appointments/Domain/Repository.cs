using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Appointments.Domain
{
    public interface IScheduleRepository
    {
        Task<int> Add(Guid principalId, string name);
        Task<object[]> List(Guid principalId);
        Task<Booking[]> Get(Guid principalId, string name);
        Task Delete(Guid principalId, string name);
    }

    public class Booking
    {
        public string Schedule { get;set;}
        public long Start { get;set;}
        public int Duration { get;set;}
    }

    public class Participation
    {
        public string SubjectId { get; set; }
        public string Name { get; set; }
    }

    public interface IParticipantRepository
    {
        Task<object[]> List(string subjectId);
        
        Task<Participation[]> Get(
            string schedule, 
            long start, 
            string subjectId);

        Task Add(
            Guid principalId,
            string schedule, 
            long start,
            int duration,
            Participation[] ps);

        Task Delete(
            string schedule,
            long start,
            string subjectId);
    }
}
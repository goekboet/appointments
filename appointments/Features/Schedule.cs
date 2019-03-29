using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Appointments.Features
{
    public class Schedule
    {
        public Guid PrincipalId { get; set; }
        public string Name { get; set; }

        public List<Appointment> Appointments { get; set; } =
            new List<Appointment>();
    }

    public interface IScheduleRepository
    {
        Task<int> Add(Schedule schedule);
        Task<string[]> List(Guid principalId);
        Task<Schedule> Get(Guid principalId, string name);
        Task Delete(Schedule schedule);
    }

    
}
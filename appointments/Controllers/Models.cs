using System;

namespace Appointments.Controllers.Models
{
    public class Appointment
    {
        public int Duration { get; set; }
        public Domain.Participation[] Participants { get; set; }
    }

    public class Schedule
    {
        public string Name { get;set;}
    }

}
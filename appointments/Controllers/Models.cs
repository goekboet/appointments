using System;

namespace Appointments.Controllers.Models
{
    public class Participant
    {
        public string SubjectId { get;set;}
        public string Name { get;set;}
    }
    public class Appointment
    {
        public int Duration { get; set; }
        public Domain.Participant[] Participants { get; set; }
    }

    public class Schedule
    {
        public string Name { get;set;}
    }

}
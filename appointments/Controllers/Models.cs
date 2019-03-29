using System;

namespace Appointments.Controllers.Models
{
    public class Schedule
    {
        public string Name { get; set; }
    }

    public class Appointment
    {
        public string Schedule { get; set; }
        public Participant[] Participants { get; set; } = new Participant[0];
    }

    public class Participant
    {
        public Guid SubjectId { get; set; }
        public string Name { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace Appointments.Domain
{
    public class Schedule
    {
        public Guid PrincipalId { get; set; }
        public string Name { get; set; }

        public List<Appointment> Appointments { get; set; } =
            new List<Appointment>();
    }
 
    public class Appointment
    {
        public Schedule Schedule { get; set;}
        public string ScheduleName { get; set; }
        public long Start { get;set; }
        public int Duration { get; set;}

        public List<Participant> Participants { get; set; } =
            new List<Participant>();
    }

     public class Participant 
    {
        public Appointment Appointment { get;set;}
        public string SubjectId {get;set;}
        public string Name { get; set; }
    }
}
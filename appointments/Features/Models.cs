using System;
using System.Collections.Generic;

namespace Appointments.Features
{
    

    public class Appointment
    {
        public long Start { get; set; }
        public int MinuteDuration { get; set; }

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
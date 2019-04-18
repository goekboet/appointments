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
        public Participant[] Participants { get; set; }
    }

    public class Schedule
    {
        public string Name { get;set;}
    }

    public class Result
    {
        public bool Success {get; set;}
        public string Message { get;set;} = "";
        public Link[] Links {get;set;}
    }

    public class Link
    {
        public string Href { get;set;}
        public string Ref { get; set;}
        public string Type {get;set;}
    }

}
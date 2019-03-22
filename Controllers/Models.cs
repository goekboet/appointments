namespace Appointments.Controllers.Models
{
    public class Schedule
    {
        public string Name { get; set; }
    }

    public class ScheduledAppointments
    {
        public string Schedule { get; set; }
        public Appointment[] Appointments { get; set; } = new Appointment[0];
    }

    public class Appointment
    {
        public Participant[] Participants { get; set; } = new Participant[0];
    }

    public class Participant
    {
        public string Name { get; set; }
    }
}
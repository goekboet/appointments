using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Json = Appointments.Controllers.Models;

namespace Appointments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        [HttpGet]
        public Json.ScheduledAppointments GetMyAppointments()
        {
            return new Json.ScheduledAppointments();
        }

        [HttpPost]
        public IActionResult CreateAppointment(Json.Appointment appt)
        {
            var never = DateTimeOffset.FromUnixTimeSeconds(0);

            return Created($"appointment/{appt.Schedule}/{never}", new Json.Appointment());
        }

        [HttpGet("{schedule}/{start}")]
        public Json.Appointment GetAppointment(string schedule, long start)
        {
            return new Json.Appointment();
        }
    }
}

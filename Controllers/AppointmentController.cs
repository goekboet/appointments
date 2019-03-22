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
        [HttpPost]
        public IActionResult CreateAppointment(Json.Appointment appt)
        {
            var never = DateTimeOffset.FromUnixTimeSeconds(0);

            return Created($"appointment/{never}", new Json.Appointment());
        }

        [HttpDelete]
        public IActionResult UnAppointment(Json.Appointment appt)
        {
            return Ok();
        }

        [HttpGet]
        public Json.ScheduledAppointments GetMyAppointments()
        {
            return new Json.ScheduledAppointments();
        }

        [HttpGet("{schedule}/{start}")]
        public Json.Appointment GetAppointment(string schedule, long start)
        {
            return new Json.Appointment();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Json = Appointments.Controllers.Models;

namespace Appointments.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        static string sub = System.Security.Claims.ClaimTypes.NameIdentifier;
        string GetSubjectId => HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == sub).Value;
            
        [HttpGet]
        public IActionResult GetMyAppointments()
        {
            return Ok(new object[0]);
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

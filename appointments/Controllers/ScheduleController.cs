using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Json = Appointments.Controllers.Models;
using Microsoft.AspNetCore.Mvc;

namespace Appointments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateSchedule(
            Json.Schedule schedule)
        {
            return Created(
                $"schedule/{schedule.Name}", 
                schedule);
        }

        [HttpDelete("{name}")]
        public IActionResult DeleteSchedule()
        {
            return Ok();
        }

        [HttpGet]
        public Json.Schedule[] ListSchedules()
        {
            return new Json.Schedule[0];
        }

        [HttpGet("{name}")]
        public Json.ScheduledAppointments GetSchedule(string name)
        {
            return new Json.ScheduledAppointments();
        }
    }
}

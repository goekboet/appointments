using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Json = Appointments.Controllers.Models;
using Domain = Appointments.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Appointments.Features;
using Microsoft.Extensions.Logging;

namespace Appointments.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        static string sub = System.Security.Claims.ClaimTypes.NameIdentifier;
        string GetSubjectId => HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == sub).Value;

        public ScheduleController(
            IScheduleRepository repo,
            ILogger<ScheduleController> log)
        {
            _repo = repo;
            _log = log;
        }

        IScheduleRepository _repo;
        ILogger<ScheduleController> _log;

        [HttpPost]
        public async Task<IActionResult> CreateSchedule(
            Json.Schedule schedule)
        {
            try {
                var principalId = Guid.Parse(GetSubjectId);
                var req = new Domain.Schedule
                {
                    PrincipalId = principalId,
                    Name = schedule.Name
                };

                await _repo.Add(req);
                _log.LogTrace($"{principalId} added schedule {schedule.Name}", req);

                return Created(
                    $"schedule/{schedule.Name}",
                    schedule);
            }
            catch (FormatException fmt)
            {
                _log.LogError($"Malformed sub-claim.", fmt);
                return BadRequest();
            }
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteSchedule(string name)
        {
            try
            {
                var principalId = Guid.Parse(GetSubjectId);
                var key = new Domain.Schedule
                {
                    PrincipalId = principalId,
                    Name = name   
                };

                await _repo.Delete(key);
                _log.LogTrace($"{principalId} deleted schedule {name}", key);
                
                return Ok();
            }
            catch (FormatException fmt)
            {
                _log.LogError($"Malformed sub-claim.", fmt);
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListSchedules()
        {
            try
            {
                var principalId = Guid.Parse(GetSubjectId);

                var r = await _repo.List(principalId);
                return Ok(r.Select(x => new 
                {
                    name = x
                }));
            }
            catch (FormatException fmt)
            {
                _log.LogError($"Malformed sub-claim.", fmt);
                return BadRequest();
            }
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetSchedule(string name)
        {
            try
            {
                var principalId = Guid.Parse(GetSubjectId);

                var r = await _repo.Get(principalId, name);
                
                return r != null 
                    ? Ok(new 
                        {
                            name = r.Name,
                            appointments = r.Appointments.Select(x => new
                            {
                                start = x.Start,
                                duration = x.MinuteDuration,
                                participants = x.Participants.Select(p => new
                                {
                                    name = p.Name
                                }).ToArray()
                            }).ToArray()
                        }) as IActionResult
                    : NotFound() as IActionResult;
            }
            catch (FormatException fmt)
            {
                _log.LogError($"Malformed sub-claim.", fmt);
                return BadRequest();
            }
        }
    }
}

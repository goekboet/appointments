using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Json = Appointments.Controllers.Models;
using Domain = Appointments.Domain;
using Appointments.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using S = Appointments.Domain.Schedule;

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
            S.IScheduleRepository repo,
            ILogger<ScheduleController> log)
        {
            _repo = repo;
            _log = log;
        }

        S.IScheduleRepository _repo;
        ILogger<ScheduleController> _log;

        [HttpPost]
        public async Task<IActionResult> CreateSchedule(
            Json.Schedule schedule)
        {
            try
            {
                var claim = new S.PrincipalClaim(
                    Guid.Parse(GetSubjectId),
                    schedule.Name
                );

                var r = await _repo.Add(claim);
                if (r == S.CreateScheduleResult.Created)
                {
                    _log.LogInformation(
                    LoggingEvents.ScheduleCreated,
                    "Schedule created with claim {claim}",
                    claim
                    );

                    return Created(
                        $"api/schedule/{schedule.Name}",
                        new Json.Result
                        {
                            Success = true,
                            Links = new []
                            {
                                new Json.Link
                                {
                                    Href = $"api/schedule/{schedule.Name}",
                                    Ref = "self",
                                    Type = "GET"
                                }
                            }
                        });
                }
                else
                {
                    return Conflict(new Json.Result
                    {
                        Success = false,
                        Message = "Already created.",
                        Links = new []
                        {
                            new Json.Link
                            {
                                Href = $"api/schedule/{schedule.Name}",
                                Ref = "self",
                                Type = "GET"
                            }
                        }
                    });
                }

            }
            catch (FormatException fmt)
            {
                _log.LogWarning($"Malformed sub-claim.", fmt);
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
                    name = x.Schedule
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
                var r = await _repo.Get(new S.PrincipalClaim(
                    principalId,
                    name
                ));

                return r != null
                    ? Ok(r.Select(x => new
                    {
                        start = x.Start,
                        duration = x.Duration
                    })) as IActionResult
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

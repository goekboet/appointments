using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Appointments.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public AppointmentController(
            IParticipantRepository pptRepo,
            IScheduleRepository schRepo,
            ILogger<AppointmentController> log)
        {
            _pptrepo = pptRepo;
            _schrepo = schRepo;
            _log = log;
        }

        private IParticipantRepository _pptrepo;
        private IScheduleRepository _schrepo;
        private ILogger<AppointmentController> _log;
            
        [HttpGet]
        public async Task<IActionResult> GetMyAppointments()
        {
            try
            {
                var r = await _pptrepo.List(GetSubjectId);
                
                return Ok(r);
            }
            catch (FormatException fmt)
            {
                _log.LogError($"Malformed sub-claim.", fmt);
                return BadRequest();
            }
        }

        [HttpPost("{schedule}/{start}")]
        public async Task<IActionResult> CreateAppointment(
            string schedule, 
            long start,
            Json.Appointment ps)
        {
            try
            {
                await _schrepo.PutAppointment(
                    new PrincipalClaim(
                        Guid.Parse(GetSubjectId),
                        schedule),
                    new Appointment
                    {
                        Start = start,
                        Duration = ps.Duration,
                        Participants = ps.Participants
                            .Select(x => new Participant
                            {
                                SubjectId = x.SubjectId,
                                Name = x.Name
                            }).ToList()
                    }
                );

                return Created($"api/appointment/{schedule}", new
                {
                    success = true,
                    message = ""
                });
            }
            catch (FormatException fmt)
            {
                _log.LogError($"Malformed sub-claim.", fmt);
                return BadRequest();
            }
        }

        [HttpGet("{schedule}/{start}")]
        public async Task<IActionResult> GetAppointment(
            string schedule, 
            long start)
        {
            try
            {
                var r = await _pptrepo.Get(new ParticipantClaim
                {
                    Schedule = schedule,
                    Start = start,
                    SubjectId = GetSubjectId
                });
                
                return r.Length > 1
                    ? Ok(
                        from p in r 
                        where p.SubjectId != GetSubjectId 
                        select new { name = p.Name })
                    : NotFound() as IActionResult;
            }
            catch (FormatException fmt)
            {
                _log.LogError($"Malformed sub-claim.", fmt);
                return BadRequest();
            }
        }

        [HttpDelete("{schedule}/{start}")]
        public async Task<IActionResult> DeleteMyAppointment(
            string schedule, 
            long start)
        {
            try
            {
                await _pptrepo.Delete(new ParticipantClaim
                {
                    Schedule = schedule,
                    Start = start,
                    SubjectId = GetSubjectId
                });
                
                return Ok();
            }
            catch (FormatException fmt)
            {
                _log.LogError($"Malformed sub-claim.", fmt);
                return BadRequest();
            }
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Appointments.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Json = Appointments.Controllers.Models;
using P = Appointments.Domain.Participant;
using S = Appointments.Domain.Schedule;

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
            P.IParticipantRepository pptRepo,
            S.IScheduleRepository schRepo,
            ILogger<AppointmentController> log)
        {
            _pptrepo = pptRepo;
            _schrepo = schRepo;
            _log = log;
        }

        private P.IParticipantRepository _pptrepo;
        private S.IScheduleRepository _schrepo;
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
                _log.LogWarning($"Malformed sub-claim.", fmt);
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
                var claim = new S.PrincipalClaim(
                        Guid.Parse(GetSubjectId),
                        schedule);

                var appointment = new S.Appointment
                    {
                        Start = start,
                        Duration = ps.Duration,
                        Participants = ps.Participants
                            .Select(x => new S.Participation
                            {
                                SubjectId = x.SubjectId,
                                Name = x.Name
                            }).ToList()
                    };

                var r = await _schrepo.PostAppointment(
                    claim,
                    appointment
                );

                if (r == S.PostAppointmentResult.ClaimNotOnRecord)
                {
                    _log.LogWarning("Invalid claim {claim}", claim);
                    return NotFound();
                }
                if (r == S.PostAppointmentResult.Conflict)
                {
                    return Conflict(new Json.Result
                    {
                        Success = false,
                        Message = "Already created.",
                        Links = new []
                        {
                            new Json.Link
                            {
                                Href = $"api/appointment/{schedule}",
                                Ref = "self",
                                Type = "GET"
                            }
                        }
                    });
                }
                else
                {
                    _log.LogInformation(
                        LoggingEvents.AppointmentCreated,
                        "Appointment {appointment} created with {claim}", 
                        appointment, 
                        claim);

                    return Created(
                        $"api/appointment/{schedule}", 
                        new Json.Result
                        {
                            Success = true,
                            Links = new []
                            {
                                new Json.Link
                                {
                                    Href = $"api/appointment/{schedule}",
                                    Ref = "self",
                                    Type = "GET"
                                }
                            }
                        });
                }
                
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
                var r = await _pptrepo.Get(new P.ParticipantClaim
                {
                    Schedule = schedule,
                    Start = start,
                    SubjectId = GetSubjectId
                });
                
                return Ok(
                        from p in r 
                        where p.SubjectId != GetSubjectId 
                        select new { name = p.Name });
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
                await _pptrepo.Delete(new P.ParticipantClaim
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

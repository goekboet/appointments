using System;
using System.Linq;
using Appointments.Controllers.Models;
using Appointments.Domain;
using Appointments.Records;
using Data = Appointments.Records;
using Domain = Appointments.Domain;

namespace Test.Repository
{
    public static class Cases
    {
        private static Random Rng { get; } = new Random();

        private static string SomeName() => Guid.NewGuid().ToString().Substring(0, 4);
        
        public static TestCase<Data.Schedule, string, ParticipantAppointment[]> ListAppointment(string s)
        {
            switch (s)
            {
                case "HaveNoApp":
                    return new TestCase<Data.Schedule, string, ParticipantAppointment[]>
                    {
                        Given = new Data.Schedule[0],
                        Arguments = SomeName(),
                        Expect = new ParticipantAppointment[0]
                    };
                case "HaveManyAppointments":
                    var subject = (id: Guid.NewGuid().ToString(),n: SomeName());
                    var keys = new []
                    {
                        (s: Guid.NewGuid().ToString(), appts: new [] {(long)Rng.Next(), (long)Rng.Next()}),
                        (s: Guid.NewGuid().ToString(), appts: new [] {(long)Rng.Next()})
                    };

                    return new TestCase<Data.Schedule, string, ParticipantAppointment[]>
                    {
                        Given = new []
                        {
                            new Data.Schedule
                            {
                                PrincipalId = Guid.NewGuid(),
                                Name = keys[0].s,
                                Appointments = keys[0].appts
                                    .Select(x => new Data.Appointment
                                    {
                                        Start = x,
                                        Participants =
                                        {
                                            new Data.Participant
                                            {
                                                SubjectId = subject.id,
                                                Name = subject.n
                                            },
                                            new Data.Participant
                                            {
                                                SubjectId = Guid.NewGuid().ToString(),
                                                Name = SomeName()
                                            }
                                        }
                                    }).ToList()
                            },
                            new Data.Schedule
                            {
                                PrincipalId = Guid.NewGuid(),
                                Name = keys[1].s,
                                Appointments = keys[1].appts
                                    .Select(x => new Data.Appointment
                                    {
                                        Start = x,
                                        Participants =
                                        {
                                            new Data.Participant
                                            {
                                                SubjectId = subject.id,
                                                Name = subject.n
                                            },
                                            new Data.Participant
                                            {
                                                SubjectId = Guid.NewGuid().ToString(),
                                                Name = SomeName()
                                            }
                                        }
                                    }).ToList()
                            }
                        },
                        Arguments = subject.id,
                        Expect = new []
                        {
                            new ParticipantAppointment
                            {
                                Schedule = keys[0].s,
                                Start = keys[0].appts[0]
                            },
                            new ParticipantAppointment
                            {
                                Schedule = keys[0].s,
                                Start = keys[0].appts[1]
                            },
                            new ParticipantAppointment
                            {
                                Schedule = keys[1].s,
                                Start = keys[1].appts[0]
                            }
                        }
                        .OrderBy(x => x.Schedule)
                        .ThenBy(x => x.Start)
                        .ToArray()
                    };
                
                default:
                    throw new ArgumentException(nameof(s));
            }
        }

        public static TestCase<Data.Schedule, ParticipantClaim, Domain.Participant[]> GetAppointment(string s)
        {
            switch (s)
            {
                case "NoSuchAppointment":
                    return new TestCase<Data.Schedule, ParticipantClaim, Domain.Participant[]>
                    {
                        Given = new Data.Schedule[0],
                        Expect = new Domain.Participant[0],
                        Arguments = new ParticipantClaim
                        {
                            Schedule = Guid.NewGuid().ToString(),
                            Start = Rng.Next(),
                            SubjectId = Guid.NewGuid().ToString()
                        }
                    };
                case "AppointmentOnRecord":
                    var knownClaim = new ParticipantClaim
                    {
                            Schedule = Guid.NewGuid().ToString(),
                            Start = Rng.Next(),
                            SubjectId = Guid.NewGuid().ToString()
                    };
                    var myName = SomeName();

                    var counterpart = new Data.Participant
                    {
                        SubjectId = Guid.NewGuid().ToString(),
                        Name = SomeName()
                    };

                    return new TestCase<Data.Schedule, ParticipantClaim, Domain.Participant[]>
                    {
                        Given = new []
                        {
                            new Data.Schedule
                            {
                                PrincipalId = Guid.NewGuid(),
                                Name = knownClaim.Schedule,
                                Appointments =
                                {
                                    new Data.Appointment
                                    {
                                        Start = Rng.Next(),
                                        Participants = Arbitrary.Participants().Take(2).ToList()
                                    },
                                    new Data.Appointment
                                    {
                                        Start = knownClaim.Start,
                                        Participants =
                                        {
                                            counterpart,
                                            new Data.Participant
                                            {
                                                SubjectId = knownClaim.SubjectId,
                                                Name = myName
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        Expect = new []
                        {
                            new Domain.Participant
                            {
                                SubjectId = knownClaim.SubjectId,
                                Name = myName
                            },
                            new Domain.Participant
                            {
                                SubjectId = counterpart.SubjectId,
                                Name = counterpart.Name
                            }
                        }.OrderBy(x => x.Name).ToArray(),
                        Arguments = knownClaim
                    };
                
                default:
                    throw new ArgumentException(nameof(s));
            }
        }
    }
}
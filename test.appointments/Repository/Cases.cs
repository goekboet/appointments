using System;
using System.Linq;
using Appointments.Controllers.Models;
using Appointments.Domain;

using Data = Appointments.Records;

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
    }
}
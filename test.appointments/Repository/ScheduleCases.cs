using System;
using System.Collections.Generic;
using System.Linq;
using Appointments.Records;

using Data = Appointments.Records;
using S = Appointments.Domain.Schedule;

namespace Test.Repository
{
    public static class ScheduleCases
    {
        private static Random Rng { get; } = new Random();

        private static string SomeName() => Guid.NewGuid().ToString().Substring(0, 4);

        public static TestCase<Data.Schedule, Guid, IEnumerable<S.PrincipalClaim>> ListSchedule(string s)
        {
            switch (s)
            {
                case "NotOnRecord":
                    return new TestCase<Data.Schedule, Guid, IEnumerable<S.PrincipalClaim>>
                    {
                        Given = new Data.Schedule[0],
                        Arguments = Guid.NewGuid(),
                        Expect = new S.PrincipalClaim[0]
                    };
                case "OnRecord":
                    var principal = Guid.NewGuid();
                    var names = new[]
                    {
                        "adam",
                        "bertil",
                        "ceasar"
                    };

                    return new TestCase<Data.Schedule, Guid, IEnumerable<S.PrincipalClaim>>
                    {
                        Given = names
                            .Select(x => new Data.Schedule
                            {
                                PrincipalId = principal,
                                Name = x,
                                Appointments = Arbitrary.Appointments(
                                    DateTimeOffset.Now.ToUnixTimeSeconds(),
                                    1800,
                                    2,
                                    x).Take(3).ToList()
                            }).ToArray(),
                        Arguments = principal,
                        Expect = names
                            .Select(x => new S.PrincipalClaim(principal, x))
                    };

                default:
                    throw new ArgumentException(nameof(s));
            }
        }

        public static TestCase<Data.Schedule, S.PrincipalClaim, S.Appointment[]> GetSchedule(string s)
        {
            switch (s)
            {
                case "NotOnRecord":
                    return new TestCase<Data.Schedule, S.PrincipalClaim, S.Appointment[]>
                    {
                        Given = new Data.Schedule[0],
                        Arguments = new S.PrincipalClaim(
                            Guid.NewGuid(),
                            Guid.NewGuid().ToString()
                        ),
                        Expect = new S.Appointment[0]
                    };
                case "OnRecord":
                    var claim = new S.PrincipalClaim(
                            Guid.NewGuid(),
                            Guid.NewGuid().ToString()
                        );

                    var seed = Arbitrary.Appointments(
                        DateTimeOffset.Now.ToUnixTimeSeconds(),
                        1800,
                        2,
                        claim.Schedule
                    ).Take(10).ToList();

                    return new TestCase<Data.Schedule, S.PrincipalClaim, S.Appointment[]>
                    {
                        Given = new[]
                        {
                            new Data.Schedule
                            {
                                PrincipalId = claim.Id,
                                Name = claim.Schedule,
                                Appointments = seed
                            }
                        },
                        Arguments = claim,
                        Expect = seed
                            .Select(x => new S.Appointment
                            {
                                Start = x.Start,
                                Participants = x.Participants
                                    .Select(p => new S.Participation
                                    {
                                        SubjectId = p.SubjectId,
                                        Name = p.Name
                                    }).ToList()
                            }).OrderBy(x => x.Start).ToArray()
                    };

                default:
                    throw new ArgumentException(nameof(s));
            }
        }

        public static TestCase<Data.Schedule, (S.PrincipalClaim, S.Appointment), S.PostAppointmentResult> PutAppointment(string s)
        {
            switch (s)
            {
                case "UnknownClaim":
                    var claim3 = new S.PrincipalClaim(
                            Guid.NewGuid(),
                            Guid.NewGuid().ToString());
                    var input2 = new S.Appointment
                    {
                        Start = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        Participants = Arbitrary.Participants()
                            .Select(x => new S.Participation
                            {
                                SubjectId = x.SubjectId,
                                Name = x.Name
                            }).Take(2).ToList()
                    };
                    return new TestCase<Data.Schedule, (S.PrincipalClaim, S.Appointment), S.PostAppointmentResult>
                    {
                        Given = new Data.Schedule[0],
                        Arguments = (claim3 ,input2),
                        Expect = S.PostAppointmentResult.ClaimNotOnRecord
                    };
                case "NotOnRecord":
                    var claim2 = new S.PrincipalClaim(
                            Guid.NewGuid(),
                            Guid.NewGuid().ToString());

                    var input = new S.Appointment
                    {
                        Start = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        Participants = Arbitrary.Participants()
                            .Select(x => new S.Participation
                            {
                                SubjectId = x.SubjectId,
                                Name = x.Name
                            }).Take(2).ToList()
                    };

                    return new TestCase<Data.Schedule, (S.PrincipalClaim, S.Appointment), S.PostAppointmentResult>
                    {
                        Given = new [] {
                            new Data.Schedule
                            {
                                PrincipalId = claim2.Id,
                                Name = claim2.Schedule,
                                Appointments = Arbitrary.Appointments(
                                    DateTimeOffset.Now.ToUnixTimeSeconds(),
                                    1800,
                                    2,
                                    claim2.Schedule
                                ).Take(10).ToList()
                                    
                            }
                        },
                        Arguments = (claim2, input),
                        Expect = S.PostAppointmentResult.Created
                    };
                case "OnRecord":
                    var claim = new S.PrincipalClaim(
                            Guid.NewGuid(),
                            Guid.NewGuid().ToString());

                    var old = new Data.Appointment
                    {
                        Start = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        Participants = Arbitrary.Participants()
                            .Take(2).ToList()
                    };

                    var update = new S.Appointment
                    {
                        Start = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        Participants = Arbitrary.Participants()
                            .Select(x => new S.Participation
                            {
                                SubjectId = x.SubjectId,
                                Name = x.Name
                            }).Take(2).ToList()
                    };

                    return new TestCase<Data.Schedule, (S.PrincipalClaim, S.Appointment), S.PostAppointmentResult>
                    {
                        Given = new[]
                        {
                            new Data.Schedule
                            {
                                PrincipalId = claim.Id,
                                Name = claim.Schedule,
                                Appointments = { old }
                            }
                        },
                        Arguments = (claim, update),
                        Expect = S.PostAppointmentResult.Conflict
                    };

                default:
                    throw new ArgumentException(nameof(s));
            }
        }

        public static TestCase<Data.Schedule, (S.PrincipalClaim, long), S.DeleteAppointmentResult> DeleteAppointment(string s)
        {
            switch (s)
            {
                case "UnknownClaim":
                    var claim3 = new S.PrincipalClaim(
                            Guid.NewGuid(),
                            Guid.NewGuid().ToString());
                    var input2 = DateTimeOffset.Now.ToUnixTimeSeconds();

                    return new TestCase<Data.Schedule, (S.PrincipalClaim, long), S.DeleteAppointmentResult>
                    {
                        Given = new Data.Schedule[0],
                        Arguments = (claim3 ,input2),
                        Expect = new S.ClaimNotOnRecord()
                    };
                case "OnRecord":
                    var claim2 = new S.PrincipalClaim(
                            Guid.NewGuid(),
                            Guid.NewGuid().ToString());

                    var onRecord = new S.Appointment
                    {
                        Start = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        Participants = Arbitrary.Participants()
                            .Select(x => new S.Participation
                            {
                                SubjectId = x.SubjectId,
                                Name = x.Name
                            }).Take(2).ToList()
                    };

                    return new TestCase<Data.Schedule, (S.PrincipalClaim, long), S.DeleteAppointmentResult>
                    {
                        Given = new [] {
                            new Data.Schedule
                            {
                                PrincipalId = claim2.Id,
                                Name = claim2.Schedule,
                                Appointments = Arbitrary.Appointments(
                                    DateTimeOffset.Now.ToUnixTimeSeconds(),
                                    1800,
                                    2,
                                    claim2.Schedule)
                                .Take(10)
                                .Concat(new [] 
                                    { 
                                        new Data.Appointment
                                        {
                                            Start = onRecord.Start,
                                            Participants = onRecord.Participants
                                                .Select(x => new Data.Participant
                                                {
                                                    SubjectId = x.SubjectId,
                                                    Name = x.Name
                                                }).ToList()
                                        }
                                    })
                                .ToList()
                            }
                        },
                        Arguments = (claim2, onRecord.Start),
                        Expect = new S.Deleted(onRecord)
                    };
                case "NotOnRecord":
                    var claim = new S.PrincipalClaim(
                            Guid.NewGuid(),
                            Guid.NewGuid().ToString());

                    return new TestCase<Data.Schedule, (S.PrincipalClaim, long), S.DeleteAppointmentResult>
                    {
                        Given = new[]
                        {
                            new Data.Schedule
                            {
                                PrincipalId = claim.Id,
                                Name = claim.Schedule,
                                Appointments = Arbitrary.Appointments(
                                    DateTimeOffset.Now.ToUnixTimeSeconds(),
                                    1800,
                                    2,
                                    claim.Schedule
                                ).Take(10).ToList()
                            }
                        },
                        Arguments = (claim, Rng.Next()),
                        Expect = new S.AppointmentNotOnRecord()
                    };

                default:
                    throw new ArgumentException(nameof(s));
            }
        }
    }
}
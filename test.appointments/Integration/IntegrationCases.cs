using System;
using System.Net;
using System.Net.Http;

using Data = Appointments.Records;
using S = Appointments.Domain.Schedule;
using System.Linq;
using Newtonsoft.Json;
using System.Text;

namespace Test.Integration
{
    public static class IntegrationCases
    {
        private static Random Rng { get; } = new Random();
        private static string SomeName() => Guid.NewGuid().ToString().Substring(0, 4);

        public static HttpContent Json(object o) =>
            new StringContent(
                JsonConvert.SerializeObject(o),
                Encoding.UTF8,
                "application/json");

        public static TestCase<Data.Schedule, HttpRequestMessage, HttpStatusCode> GetRequests(string key)
        {
            S.PrincipalClaim claim = null;
            Data.Schedule schedule = null;
            Data.Appointment appointment = null;
            string participantId = null;
            string scheduleName = null;
            switch (key)
            {
                case "Known principal list Schedules":
                    claim = new S.PrincipalClaim(
                        Guid.NewGuid(),
                        Guid.NewGuid().ToString()
                    );

                    return new TestCase<Data.Schedule, HttpRequestMessage, HttpStatusCode>
                    {
                        Given = new []
                        {
                            new Data.Schedule
                            {
                                PrincipalId = claim.Id,
                                Name = claim.Schedule
                            }
                        },
                        Arguments = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri("/api/schedule", UriKind.Relative),
                            Headers =
                            {
                                { "Authorization", $"Bearer {Auth.Issuer.Token(claim.Id.ToString())}" }
                            }
                        },
                        Expect = HttpStatusCode.OK
                    };
                case "Get Existing Schedule":
                    claim = new S.PrincipalClaim(
                        Guid.NewGuid(),
                        Guid.NewGuid().ToString()
                    );
                    schedule = new Data.Schedule
                    {
                        PrincipalId = claim.Id,
                        Name = claim.Schedule,
                        Appointments = Data.Arbitrary.Appointments(
                            DateTimeOffset.Now.ToUnixTimeSeconds(),
                            1800,
                            2,
                            claim.Schedule
                        ).Take(10).ToList()
                    };

                    return new TestCase<Data.Schedule, HttpRequestMessage, HttpStatusCode>
                    {
                        Given = new [] { schedule },
                        Arguments = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri($"/api/schedule/{claim.Schedule}", UriKind.Relative),
                            Headers =
                            {
                                { "Authorization", $"Bearer {Auth.Issuer.Token(claim.Id.ToString())}" }
                            }
                        },
                        Expect = HttpStatusCode.OK
                    };
                case "List my appointments":
                    participantId = Guid.NewGuid().ToString();
                    return new TestCase<Data.Schedule, HttpRequestMessage, HttpStatusCode>
                    {
                        Given = new []
                        {
                            new Data.Schedule
                            {
                                PrincipalId = Guid.NewGuid(),
                                Name = Guid.NewGuid().ToString(),
                                Appointments =
                                {
                                    new Data.Appointment
                                    {
                                        Start = DateTimeOffset.Now.ToUnixTimeSeconds(),
                                        Participants = 
                                        {
                                            new Data.Participant { SubjectId = participantId, Name = SomeName() }
                                        }
                                    }
                                }
                            }
                        },
                        Arguments = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri("/api/appointment", UriKind.Relative),
                            Headers =
                            {
                                { HttpRequestHeader.Authorization.ToString(), $"Bearer {Auth.Issuer.Token(participantId)}" }
                            }
                        },
                        Expect = HttpStatusCode.OK
                    };
                case "Get my appointment":
                    participantId = Guid.NewGuid().ToString();
                    scheduleName = Guid.NewGuid().ToString();
                    
                    appointment = new Data.Appointment
                    {
                        Start = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        Participants = 
                        {
                            new Data.Participant { SubjectId = participantId, Name = SomeName() }
                        }

                    };

                    return new TestCase<Data.Schedule, HttpRequestMessage, HttpStatusCode>
                    {
                        Given = new [] 
                        {
                            new Data.Schedule
                            {
                                PrincipalId = Guid.NewGuid(),
                                Name = scheduleName,
                                Appointments =
                                {
                                    appointment
                                }
                            }
                        },
                        Arguments = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri($"/api/appointment/{scheduleName}/{appointment.Start}", UriKind.Relative),
                            Headers =
                            {
                                { HttpRequestHeader.Authorization.ToString(), $"Bearer {Auth.Issuer.Token(participantId)}" }
                            }
                        },
                        Expect = HttpStatusCode.OK
                    };

                case "PostSchedule":
                    claim = new S.PrincipalClaim(
                        Guid.NewGuid(),
                        Guid.NewGuid().ToString()
                    );

                    var payload = Json(new { name = Guid.NewGuid().ToString() });

                    return new TestCase<Data.Schedule, HttpRequestMessage, HttpStatusCode>
                    {
                        Given = new Data.Schedule[0],
                        Arguments = new HttpRequestMessage
                        {
                            Method = HttpMethod.Post,
                            RequestUri = new Uri($"/api/schedule", UriKind.Relative),
                            Headers =
                                {
                                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {Auth.Issuer.Token(claim.Id.ToString())}" }
                                },
                            Content = payload
                        },
                        Expect = HttpStatusCode.Created
                    };
                
                case "PostAppointment":
                    claim = new S.PrincipalClaim(
                        Guid.NewGuid(),
                        Guid.NewGuid().ToString()
                    );

                    return new TestCase<Data.Schedule, HttpRequestMessage, HttpStatusCode>
                    {
                        Given = new []
                        {
                            new Data.Schedule
                            {
                                PrincipalId = claim.Id,
                                Name = claim.Schedule,
                                Appointments = Data.Arbitrary.Appointments(
                                    DateTimeOffset.Now.ToUnixTimeSeconds(),
                                    1800,
                                    2,
                                    claim.Schedule
                                ).Take(10).ToList()
                            }
                        },
                        Arguments = new HttpRequestMessage
                        {
                            Method = HttpMethod.Post,
                            RequestUri = new Uri($"/api/appointment/{claim.Schedule}/{Rng.Next()}", UriKind.Relative),
                            Headers =
                                    {
                                        { "Authorization", $"Bearer {Auth.Issuer.Token(claim.Id.ToString())}" }
                                    },
                            Content = Json(new
                            {
                                duration = 10,
                                participants = Enumerable.Range(0, 2).Select(x => new 
                                    { 
                                        name = x.ToString(), 
                                        subjectId = Guid.NewGuid().ToString()
                                    }) 
                            })
                        },
                        Expect = HttpStatusCode.Created
                    };

                case "DeleteMyAppointment":
                    participantId = Guid.NewGuid().ToString();
                    scheduleName = Guid.NewGuid().ToString();

                    appointment = new Data.Appointment
                    {
                        Start = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        Participants = 
                        {
                            new Data.Participant { SubjectId = participantId, Name = SomeName() },
                            new Data.Participant { SubjectId = Guid.NewGuid().ToString(), Name = SomeName() }
                        }
                    };

                    return new TestCase<Data.Schedule, HttpRequestMessage, HttpStatusCode>
                    {
                        Given = new []
                        {
                            new Data.Schedule
                            {
                                PrincipalId = Guid.NewGuid(),
                                Name = scheduleName,
                                Appointments = 
                                {
                                    appointment,
                                }
                            }
                        },
                        Arguments = new HttpRequestMessage
                        {
                            Method = HttpMethod.Delete,
                            RequestUri = new Uri($"/api/appointment/{scheduleName}/{appointment.Start}", UriKind.Relative),
                            Headers =
                                    {
                                        { "Authorization", $"Bearer {Auth.Issuer.Token(participantId)}" }
                                    },
                        },
                        Expect = HttpStatusCode.OK
                    };

                default:
                    throw new ArgumentOutOfRangeException(nameof(key));
            }
        }
    }
}
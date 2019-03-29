using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Appointments.Controllers.Models;
using Appointments.Records;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using DevAuth = Appointments.Auth.DevelopmentAuth;
using Domain = Appointments.Features;
using Controller = Appointments.Controllers.Models;

namespace Test.Cntrollers
{
    [TestClass]
    public class RouteTests
    {
        private readonly WebApplicationFactory<Appointments.Startup> _factory;

        public RouteTests()
        {
            _factory = new AppointmentsWebApplicationFactory<Appointments.Startup>(SeedData);
        }

        public static Guid KnownPrincipal => Guid.NewGuid();
        public static Guid KnownSubject => Guid.NewGuid();

        public static Domain.Schedule[] SeedData => new [] 
        {
            new Domain.Schedule
            {
                PrincipalId = KnownPrincipal,
                Name = "first",
                Appointments = 
                {
                    new Domain.Appointment
                    {
                        Start = 0,
                        MinuteDuration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = KnownSubject.ToString(),
                                Name = "someName"
                            }
                        }
                    },
                    new Domain.Appointment
                    {
                        Start = 10,
                        MinuteDuration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = KnownSubject.ToString(),
                                Name = "someName"
                            }
                        }
                    },
                    new Domain.Appointment
                    {
                        Start = 20,
                        MinuteDuration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = KnownSubject.ToString(),
                                Name = "someName"
                            }
                        }
                    }
                }
            },
            new Domain.Schedule
            {
                PrincipalId = KnownPrincipal,
                Name = "second",
                Appointments = 
                {
                    new Domain.Appointment
                    {
                        Start = 0,
                        MinuteDuration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = KnownSubject.ToString(),
                                Name = "someName"
                            }
                        }
                    },
                    new Domain.Appointment
                    {
                        Start = 10,
                        MinuteDuration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = KnownSubject.ToString(),
                                Name = "someName"
                            }
                        }
                    },
                    new Domain.Appointment
                    {
                        Start = 20,
                        MinuteDuration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = KnownSubject.ToString(),
                                Name = "someName"
                            }
                        }
                    }
                }
            },
            new Domain.Schedule
            {
                PrincipalId = Guid.NewGuid(),
                Name = "third",
                Appointments = 
                {
                    new Domain.Appointment
                    {
                        Start = 0,
                        MinuteDuration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "someOtherName"
                            }
                        }
                    },
                    new Domain.Appointment
                    {
                        Start = 10,
                        MinuteDuration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = KnownSubject.ToString(),
                                Name = "someName"
                            }
                        }
                    },
                    new Domain.Appointment
                    {
                        Start = 20,
                        MinuteDuration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = KnownSubject.ToString(),
                                Name = "someName"
                            }
                        }
                    }
                }
            }
        };

        public static Domain.Schedule MockSchedule = new Domain.Schedule { Name = "someSchedule" };
        public static Appointment MockAppointment = new Appointment
        {
            Schedule = "someSchedule",
            Participants = new[]
            {
                new Participant { SubjectId = Guid.NewGuid(), Name = "someDude"},
                new Participant { SubjectId = Guid.NewGuid(), Name = "someOtherDude"}
            }
        };

        static Dictionary<string, object> Expectations = new Dictionary<string, object>
        {
            {"/api/appointment", new object() }
        };

        [DataRow("/api/appointment")]
        [DataRow("/api/appointment/someSchedule/666")]
        [DataRow("/api/schedule")]
        [DataRow("/api/schedule/someSchedule")]
        [TestMethod]
        public async Task GetRoutesOk(string path)
        {
            var client = _factory.CreateClient();
            var subjectId = Guid.NewGuid().ToString();

            var testJwt = DevAuth.Token(subjectId);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(path, UriKind.Relative),
                Headers = 
                { 
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {testJwt}" }
                }
            };

            var response = await client.SendAsync(request);

            Assert.IsTrue(
                response.IsSuccessStatusCode,
                $"{response.StatusCode} {response.ReasonPhrase}");
        }

        Dictionary<string, object> Payloads = new Dictionary<string, object>
        {
            ["api/appointment"] = MockAppointment,
            ["api/schedule"] = MockSchedule
        };

        [DataRow("api/appointment")]
        [DataRow("api/schedule")]
        [TestMethod]
        public async Task PostRoutesOk(string path)
        {
            var client = _factory.CreateClient();

            var subjectId = Guid.NewGuid().ToString();

            var testJwt = DevAuth.Token(subjectId);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(path, UriKind.Relative),
                Headers = 
                { 
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {testJwt}" },
                },
                Content = new StringContent(
                    content: JsonConvert.SerializeObject(Payloads[path]),
                    encoding: Encoding.UTF8,
                    mediaType: "application/json")
            };

            var response = await client.SendAsync(request);

            Assert.IsTrue(
                response.IsSuccessStatusCode,
                $"{response.StatusCode} {response.ReasonPhrase}");
        }

        [DataRow("api/schedule/someSchedule")]
        public async Task DeleteRoutesOk(string path)
        {
            var client = _factory.CreateClient();
            var subjectId = Guid.NewGuid().ToString();

            var testJwt = DevAuth.Token(subjectId);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(path, UriKind.Relative),
                Headers = 
                { 
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {testJwt}" }
                }
            };

            var response = await client.SendAsync(request);

            Assert.IsTrue(
                response.IsSuccessStatusCode,
                $"{response.StatusCode} {response.ReasonPhrase}");
        }
    }
}

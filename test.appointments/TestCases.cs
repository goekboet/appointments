using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using DevAuth = Appointments.Auth.DevelopmentAuth;
using Domain = Appointments.Domain;

namespace Test.Controllers
{
    public class TestCase
    {
        public HttpRequestMessage Request { get; set; }
        public HttpResponseMessage Expect { get; set; }
    }

    public static class HttpHelpers
    {
        public static HttpContent Json(object o) =>
            new StringContent(
                JsonConvert.SerializeObject(o),
                Encoding.UTF8,
                "application/json");

        public static string KnownPrincipalToken => DevAuth.Token(Seed.KnownPrincipal.ToString());
        public static string KnownSubjectToken => DevAuth.Token(Seed.KnownSubject.ToString());

        public static HttpRequestMessage PostAppointment(
            string schedule,
            long start,
            int n) => new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"/api/appointment/{schedule}/{start}", UriKind.Relative),
                Headers =
                        {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {HttpHelpers.KnownPrincipalToken}" }
                        },
                Content = HttpHelpers.Json(new
                {
                    duration = 10,
                    participants = Enumerable.Range(0, n).Select(x => new 
                        { 
                            name = x.ToString(), 
                            subjectId = Guid.NewGuid().ToString()
                        }) 
                })
            };

        public static HttpRequestMessage PostSchedule(string name) => 
            new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"/api/schedule", UriKind.Relative),
                Headers =
                        {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {HttpHelpers.KnownPrincipalToken}" }
                        },
                Content = HttpHelpers.Json(new { name = name })
            };

        public static HttpRequestMessage DeleteScedule(
            Guid principalId,
            string name) => new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"/api/schedule/{name}", UriKind.Relative),
                Headers =
                        {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {DevAuth.Token(principalId.ToString())}" }
                        },
            };

        public static HttpRequestMessage DeleteMyAppointment(
            string subjectId,
            string schedule,
            long start) => new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"/api/appointment/{schedule}/{start}", UriKind.Relative),
                Headers =
                {
                    { 
                        HttpRequestHeader.Authorization.ToString(), 
                        $"Bearer {DevAuth.Token(subjectId)}" 
                    }
                },
            };
    }

    public static class Seed
    {
        public static Guid KnownPrincipal { get; } = Guid.NewGuid();
        public static Guid KnownSubject { get; } = Guid.NewGuid();
        public static Guid Cpt1 { get; } = Guid.NewGuid();
        public static Guid Cpt2 { get; } = Guid.NewGuid();

        public static Domain.Schedule[] Data => new[]
        {
            new Domain.Schedule
            {
                PrincipalId = KnownPrincipal,
                Name = "first",
                Appointments =
                {
                    new Domain.Appointment
                    {
                        Start = 100,
                        Duration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "1-first-100"
                            }
                        }
                    },
                    new Domain.Appointment
                    {
                        Start = 110,
                        Duration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "2-first-110"
                            }
                        }
                    },
                    new Domain.Appointment
                    {
                        Start = 120,
                        Duration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = KnownSubject.ToString(),
                                Name = "known-first-120"
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
                        Start = 200,
                        Duration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "3-second-200"
                            }
                        }
                    },
                    new Domain.Appointment
                    {
                        Start = 210,
                        Duration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "4-second-210"
                            }
                        }
                    },
                    new Domain.Appointment
                    {
                        Start = 220,
                        Duration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "5-second-220"
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
                        Start = 300,
                        Duration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = KnownSubject.ToString(),
                                Name = "known-third-300"
                            },
                            new Domain.Participant
                            {
                                SubjectId = Cpt1.ToString(),
                                Name = "cpt1-third-300"
                            },
                            new Domain.Participant
                            {
                                SubjectId = Cpt2.ToString(),
                                Name = "cpt2-third-300"
                            }
                        }
                    },
                    new Domain.Appointment
                    {
                        Start = 310,
                        Duration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "6-third-310"
                            }
                        }
                    },
                    new Domain.Appointment
                    {
                        Start = 320,
                        Duration = 10,
                        Participants =
                        {
                            new Domain.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "7-third-320"
                            }
                        }
                    }
                }
            }
        };
    }

    public static class TestCases
    {
        public static Dictionary<string, TestCase> Repository = new Dictionary<string, TestCase>
        {
            {
                "Known principal list Schedules",
                new TestCase
                {
                    Request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("/api/schedule", UriKind.Relative),
                        Headers =
                        {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {HttpHelpers.KnownPrincipalToken}" }
                        }
                    },
                    Expect = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = HttpHelpers.Json(new []
                            {
                                new { name = "first" },
                                new { name = "second" }
                            })
                    }
                }
            },
            {
                "Get Existing Schedule",
                new TestCase
                {
                    Request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("/api/schedule/first", UriKind.Relative),
                        Headers =
                        {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {HttpHelpers.KnownPrincipalToken}" }
                        }
                    },
                    Expect = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = HttpHelpers.Json(new []
                        {
                            new
                            {
                                start = 100,
                                duration = 10
                            },
                            new
                            {
                                start = 110,
                                duration = 10
                            },
                            new
                            {
                                start = 120,
                                duration = 10
                            }
                        })
                    }
                }
            },
            {
                "List my appointments",
                new TestCase
                {
                    Request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("/api/appointment", UriKind.Relative),
                        Headers =
                        {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {HttpHelpers.KnownSubjectToken}" }
                        }
                    },
                    Expect = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = HttpHelpers.Json(new []
                        {
                            new
                            {
                                schedule = "first",
                                start = 120,
                                duration = 10
                            },
                            new
                            {
                                schedule = "third",
                                start = 300,
                                duration = 10
                            }
                        })
                    }
                }
            },
            {
                "Get my appointment",
                new TestCase
                {
                    Request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("/api/appointment/third/300", UriKind.Relative),
                        Headers =
                        {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {HttpHelpers.KnownSubjectToken}" }
                        }
                    },
                    Expect = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = HttpHelpers.Json(new []
                        {
                            new
                            {
                                name = "cpt1-third-300"
                            },
                            new
                            {
                                name = "cpt2-third-300"
                            }
                        })
                    }
                }
            },
            {
                "Get someone elses appointment",
                new TestCase
                {
                    Request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("/api/appointment/first/100", UriKind.Relative),
                        Headers =
                        {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {HttpHelpers.KnownSubjectToken}" }
                        }
                    },
                    Expect = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Content = new StringContent("")
                    }
                }
            }
        };
    }
}
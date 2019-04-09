using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

using Domain = Appointments.Domain;
using Data = Appointments.Records;
using Suite = Test.Integration.Helpers;

namespace Test.Integration
{
    public class TestCase
    {
        public HttpRequestMessage Request { get; set; }
        public HttpResponseMessage Expect { get; set; }
    }

    public static class Seed
    {
        public static Guid Cpt1 { get; } = Guid.NewGuid();
        public static Guid Cpt2 { get; } = Guid.NewGuid();

        public static Data.Schedule[] Data => new[]
        {
            new Data.Schedule
            {
                PrincipalId = Suite.KnownPrincipal,
                Name = "first",
                Appointments =
                {
                    new Data.Appointment
                    {
                        Start = 100,
                        Duration = 10,
                        Participants =
                        {
                            new Data.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "1-first-100"
                            }
                        }
                    },
                    new Data.Appointment
                    {
                        Start = 110,
                        Duration = 10,
                        Participants =
                        {
                            new Data.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "2-first-110"
                            }
                        }
                    },
                    new Data.Appointment
                    {
                        Start = 120,
                        Duration = 10,
                        Participants =
                        {
                            new Data.Participant
                            {
                                SubjectId = Suite.KnownSubject.ToString(),
                                Name = "known-first-120"
                            }
                        }
                    }
                }
            },
            new Data.Schedule
            {
                PrincipalId = Suite.KnownPrincipal,
                Name = "second",
                Appointments =
                {
                    new Data.Appointment
                    {
                        Start = 200,
                        Duration = 10,
                        Participants =
                        {
                            new Data.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "3-second-200"
                            }
                        }
                    },
                    new Data.Appointment
                    {
                        Start = 210,
                        Duration = 10,
                        Participants =
                        {
                            new Data.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "4-second-210"
                            }
                        }
                    },
                    new Data.Appointment
                    {
                        Start = 220,
                        Duration = 10,
                        Participants =
                        {
                            new Data.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "5-second-220"
                            }
                        }
                    }
                }
            },
            new Data.Schedule
            {
                PrincipalId = Guid.NewGuid(),
                Name = "third",
                Appointments =
                {
                    new Data.Appointment
                    {
                        Start = 300,
                        Duration = 10,
                        Participants =
                        {
                            new Data.Participant
                            {
                                SubjectId = Suite.KnownSubject.ToString(),
                                Name = "known-third-300"
                            },
                            new Data.Participant
                            {
                                SubjectId = Cpt1.ToString(),
                                Name = "cpt1-third-300"
                            },
                            new Data.Participant
                            {
                                SubjectId = Cpt2.ToString(),
                                Name = "cpt2-third-300"
                            }
                        }
                    },
                    new Data.Appointment
                    {
                        Start = 310,
                        Duration = 10,
                        Participants =
                        {
                            new Data.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = "6-third-310"
                            }
                        }
                    },
                    new Data.Appointment
                    {
                        Start = 320,
                        Duration = 10,
                        Participants =
                        {
                            new Data.Participant
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
        public static string PostScheduleString1 { get; } = Guid.NewGuid().ToString();
        public static string PostAppointmentString1 { get; } = Guid.NewGuid().ToString();
        public static long PostAppointmentStart {get;} = DateTimeOffset.Now.ToUnixTimeSeconds();
        public static string DeleteScheduleString1 { get; } = Guid.NewGuid().ToString();
        public static long DeleteAppointmentStart {get;} = DateTimeOffset.Now.ToUnixTimeSeconds();
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
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {Helpers.KnownPrincipalToken}" }
                        }
                    },
                    Expect = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = Helpers.Json(new []
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
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {Helpers.KnownPrincipalToken}" }
                        }
                    },
                    Expect = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = Helpers.Json(new []
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
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {Helpers.KnownSubjectToken}" }
                        }
                    },
                    Expect = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = Helpers.Json(new []
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
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {Helpers.KnownSubjectToken}" }
                        }
                    },
                    Expect = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = Helpers.Json(new []
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
                "PostSchedule",
                new TestCase
                {
                    Request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri($"/api/schedule", UriKind.Relative),
                        Headers =
                            {
                                { HttpRequestHeader.Authorization.ToString(), $"Bearer {Helpers.KnownPrincipalToken}" }
                            },
                        Content = Helpers.Json(new { name = PostScheduleString1 })
                    },
                    Expect = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Created,
                        Content = Helpers.Json(new 
                            {
                                success = true,
                                message = ""
                            }),
                        Headers = { {"Location", $"api/schedule/{PostScheduleString1}"} }
                    }
                }
            },
            {
                "PostAppointment",
                new TestCase
                {
                    Request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri($"/api/appointment/{PostAppointmentString1}/{PostAppointmentStart}", UriKind.Relative),
                        Headers =
                                {
                                    { "Authorization", $"Bearer {Helpers.KnownPrincipalToken}" }
                                },
                        Content = Helpers.Json(new
                        {
                            duration = 10,
                            participants = Enumerable.Range(0, 2).Select(x => new 
                                { 
                                    name = x.ToString(), 
                                    subjectId = Guid.NewGuid().ToString()
                                }) 
                        })
                    },
                    Expect = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Created,
                        Content = Helpers.Json(new 
                            {
                                success = true,
                                message = ""
                            }),
                        Headers = { {"Location", $"api/appointment/{PostAppointmentString1}"} }
                    }
                }
            },
            {
                "DeleteMyAppointment",
                new TestCase
                {
                    Request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Delete,
                        RequestUri = new Uri($"/api/appointment/{DeleteScheduleString1}/{DeleteAppointmentStart}", UriKind.Relative),
                        Headers =
                                {
                                    { "Authorization", $"Bearer {Helpers.KnownSubjectToken}" }
                                },
                    },
                    Expect = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("")
                    }
                }
            }
        };
    }
}
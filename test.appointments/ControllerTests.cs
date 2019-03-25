using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Appointments.Controllers.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using DevAuth = Appointments.Auth.DevelopmentAuth;

namespace Test.Cntrollers
{
    [TestClass]
    public class RouteTests
    {
        private readonly WebApplicationFactory<Appointments.Startup> _factory = new WebApplicationFactory<Appointments.Startup>();

        public static Schedule MockSchedule = new Schedule { Name = "someSchedule" };
        public static Appointment MockAppointment = new Appointment
        {
            Schedule = "someSchedule",
            Participants = new[]
            {
                new Participant { SubjectId = Guid.NewGuid(), Name = "someDude"},
                new Participant { SubjectId = Guid.NewGuid(), Name = "someOtherDude"}
            }
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

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
using Domain = Appointments.Domain;
using Controller = Appointments.Controllers.Models;
using Test.Cntrollers.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Test.Controllers
{
    [TestClass]
    public class RouteTests
    {
        private readonly WebApplicationFactory<Appointments.Startup> _factory;

        public static readonly Dictionary<string, string> _dict =
        new Dictionary<string, string>
        {
            {"ConnectionStrings:Psql", TestContextFactory.Cs},
        };

        public RouteTests()
        {
            _factory = new WebApplicationFactory<Appointments.Startup>().WithWebHostBuilder(host =>
            {
                host.ConfigureAppConfiguration((h, appcfg) =>
                {
                    appcfg.AddInMemoryCollection(_dict);
                });
            });
        }

        [ClassInitialize]
        public static async Task Up(TestContext testctx)
        {
            using (var ctx = new TestContextFactory().CreateDbContext(new string[0]))
            {
                await ctx.Database.MigrateAsync();
                ctx.AddRange(Seed.Data);
                await ctx.SaveChangesAsync();
            }
        }

        [ClassCleanup]
        public static async Task Down()
        {
            using (var ctx = new TestContextFactory().CreateDbContext(new string[0]))
            {
                await ctx.Database.EnsureDeletedAsync();
            }
        }

        [DataRow("Known principal list Schedules")]
        [DataRow("Get Existing Schedule")]
        [DataRow("List my appointments")]
        [DataRow("Get my appointment")]
        [DataRow("Get someone elses appointment")]
        [TestMethod]
        public async Task GetRoutesOk(string key)
        {
            var c = TestCases.Repository[key];
            var client = _factory.CreateClient();

            var response = await client.SendAsync(c.Request);

            Assert.AreEqual(c.Expect.StatusCode, response.StatusCode);

            var expectedContent = await c.Expect.Content.ReadAsStringAsync();
            var actualContent = await response.Content.ReadAsStringAsync();

            Assert.IsTrue(!response.IsSuccessStatusCode ||
                expectedContent == actualContent);
        }

        [TestMethod]
        public async Task PostAppointment()
        {
            var client = _factory.CreateClient();
            var s = "first";
            var start = DateTimeOffset.Now.ToUnixTimeSeconds();
            var n = 4;
            var req = HttpHelpers.PostAppointment(s, start, n);

            var response = await client.SendAsync(req);

            var location = response.Headers.GetValues("Location")
                .First();

            var v = await VerifyAppointmentPost(s, start);
            
            Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);
            Assert.AreEqual($"api/appointment/{s}", location);
            Assert.AreEqual(n, v);
        }

        private async Task<int> VerifyAppointmentPost(
            string schedule,
            long start)
        {
            using (var ctx = new TestContextFactory().CreateDbContext(new string[0]))
            {
                return await ctx.Schedules
                    .Where(x => x.Name == schedule)
                    .SelectMany(x => x.Appointments)
                    .CountAsync();
            }
        }

        [TestMethod]
        public async Task PostSchedule()
        {
            var client = _factory.CreateClient();
            var s = Guid.NewGuid().ToString();
            var req = HttpHelpers.PostSchedule(s);

            var response = await client.SendAsync(req);

            var location = response.Headers.GetValues("Location")
                .First();

            var v = await CountSchedules(s);
            
            Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);
            Assert.AreEqual($"api/schedule/{s}", location);
            Assert.AreEqual(1, v);
        }

        private async Task<int> CountSchedules(
            string name)
        {
            using (var ctx = new TestContextFactory().CreateDbContext(new string[0]))
            {
                return await ctx.Schedules
                    .Where(x => x.Name == name)
                    .CountAsync();
            }
        }

        [TestMethod]
        public async Task DeleteMyAppointment()
        {
            var client = _factory.CreateClient();
            var schedule = Guid.NewGuid().ToString();
            
            var principal = Guid.NewGuid();
            var subject = Guid.NewGuid().ToString();
            var start = DateTimeOffset.Now.ToUnixTimeSeconds();
            await AddSchedule(principal, schedule);
            await AddAppointment(principal, schedule, start, subject);
            var before = await CountParticipations(subject);

            var req = HttpHelpers.DeleteMyAppointment(
                subject, schedule, start);
            var response = await client.SendAsync(req);

            var after = await CountParticipations(subject);
            
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(before.others, after.others);
            Assert.AreEqual(before.mine, after.mine + 1);
        }

        private async Task AddSchedule(
            Guid principalId,
            string name)
        {
            using (var ctx = new TestContextFactory().CreateDbContext(new string[0]))
            {
                ctx.Add(new Domain.Schedule
                {
                    PrincipalId = principalId,
                    Name = name,
                    Appointments = Enumerable
                        .Range(0, 10)
                        .Select(x => new Domain.Appointment
                        {
                            ScheduleName = name,
                            Start = x * 100,
                            Duration = 90,
                            Participants = Enumerable.Range(0, 2)
                            .Select(p => new Domain.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = p.ToString()
                            })
                            .ToList()
                        })
                        .ToList()
                });

                await ctx.SaveChangesAsync();
            }
        }

        public async Task AddAppointment(
                Guid principal,
                string schedule,
                long start,
                string subjectId)
            {
                using (var ctx = new TestContextFactory().CreateDbContext(new string[0]))
                {
                    var s = ctx.Schedules
                        .Where(x => 
                            x.Name == schedule &&
                            x.PrincipalId == principal)
                        .Include(x => x.Appointments)
                        .Single();

                    s.Appointments.Add(new Domain.Appointment
                    {
                        ScheduleName = schedule,
                        Start = start,
                        Duration = 90,
                        Participants = new []
                        {
                            new Domain.Participant
                            {
                                SubjectId = subjectId,
                                Name = subjectId
                            },
                            new Domain.Participant
                            {
                                SubjectId = Guid.NewGuid().ToString(),
                                Name = Guid.NewGuid().ToString()
                            }
                        }.ToList()
                    });

                    await ctx.SaveChangesAsync();
                }
            }
        
        public async Task<(int mine, int others)> CountParticipations(
            string subjectId)
        {
            using (var ctx = new TestContextFactory().CreateDbContext(new string[0]))
            {
                var q = await ctx.Appointments
                    .SelectMany(x => x.Participants)
                    .GroupBy(
                        x => x.SubjectId == subjectId)
                    .Select(x => new { mine = x.Key, participants = x.Count() })
                    .ToArrayAsync();

                return 
                (
                    q.Where(x => x.mine).Count(),
                    q.Where(x => !x.mine).Count()
                );
            }
        }
    }
}

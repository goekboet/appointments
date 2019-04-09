using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Appointments.Controllers.Models;
using Appointments.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Integration;
using Test.Records;

using Arbitrary = Appointments.Records.Arbitrary;
using Data = Appointments.Records;
using Domain = Appointments.Domain;

namespace Test.Repository
{
    [TestClass]
    public class Suite
    {
        static string DBCreds { get; } = TestContextFactory.GetFresh();
        static Pgres dbConn() => 
            new TestContextFactory().CreateDbContext(new [] { DBCreds });

        [ClassInitialize]
        public static async Task Up(TestContext testctx)
        {
            using (var ctx = dbConn())
            {
                await ctx.Database.MigrateAsync();
                var seed = Arbitrary.Schedules(
                    0,
                    7200,
                    2,
                    3
                )
                .Take(3)
                .Concat(new [] {KnownSchedule})
                .ToArray();
                
                ctx.AddRange(seed);
                await ctx.SaveChangesAsync();
            }
        }

        [ClassCleanup]
        public static async Task Down()
        {
            using (var ctx = dbConn())
            {
                await ctx.Database.EnsureDeletedAsync();
            }
        }

        private static async Task Seed(Data.Schedule[] s)
        {
            using (var ctx = dbConn())
            {
                ctx.AddRange(s);
                await ctx.SaveChangesAsync();
            }
        }

        [TestMethod]
        public async Task ListSchedules()
        {
            var somePrincipalId = Guid.NewGuid();
            var someNames = new []
            {
                "adam",
                "ceasar",
                "bertil"
            };
            
            await Seed(someNames.Select(x => new Data.Schedule
            {
                PrincipalId = somePrincipalId,
                Name = x,
                Appointments = Arbitrary
                    .Appointments(0, 7800, 2, x)
                    .Take(3).ToList()
            }).ToArray());

            using (var ctx = dbConn())
            {
                var sut = new PgresRepo(ctx);
                var r = await sut.List(new Domain.PrincipalClaim(somePrincipalId));
                var e = someNames.Select(x => new Domain.Schedule
                {
                    Name = x,
                    PrincipalId = somePrincipalId
                }).OrderBy(x => x.Name);

                Assert.IsTrue(e.SequenceEqual(r));
            }
        }

        private static Random Rng { get; } = new Random();
        private static long Start { get; } = (long)Rng.Next();
        private static Guid KnownPrincipal { get; } = 
            Guid.NewGuid();

        private static string KnownName { get; } =
            "known";

        private static List<Data.Appointment> KnownAppointments { get; } =
            Arbitrary.Appointments(
                Start,
                1000,
                2,
                KnownName).Take(3).ToList();

        private static Domain.Appointment[] Expected { get; } =
            KnownAppointments.Select(x => new Domain.Appointment
            {
                Start = x.Start,
                Duration = x.Duration,
                Participants = x.Participants
                    .Select(p => new Domain.Participant
                    {
                        SubjectId = p.SubjectId,
                        Name = p.Name
                    }).ToList()
            }).OrderBy(x => x.Start).ToArray();

        private static Data.Schedule KnownSchedule =
            new Data.Schedule
            {
                PrincipalId = KnownPrincipal,
                Name = KnownName,
                Appointments = KnownAppointments
            };
        

        private static string SomeName() => Guid.NewGuid().ToString().Substring(0, 4);

        private static Dictionary<string, (Guid pId, string s, bool hit)> Cases =
            new Dictionary<string, (Guid pId, string s, bool hit)>
            {
                ["UK-UK"] = (Guid.NewGuid(), SomeName(), false),
                ["UK-KN"] = (Guid.NewGuid(), KnownName, false),
                ["KN-KN"] = (KnownPrincipal, KnownName, true)
            };

        [TestMethod]
        [DataRow("UK-UK")]
        [DataRow("UK-KN")]
        [DataRow("KN-KN")]
        public async Task GetSchedule(string caseId)
        {
            var input = Cases[caseId];

            using (var sut = new PgresRepo(dbConn()))
            {
                var r = await sut.Get(new Domain.PrincipalClaim(
                    input.pId,
                    input.s
                ));

                var expect = input.hit
                    ? Expected
                    : new Domain.Appointment[0];

                Assert.IsTrue(expect.SequenceEqual(r));
            }
        }
    }
}
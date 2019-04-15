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
            new TestContextFactory().CreateDbContext(new[] { DBCreds });

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
                .Concat(new[]
                {
                    KnownSchedule,
                    PutScheduleSeed
                })
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
            var someNames = new[]
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
                var r = await sut.List(somePrincipalId);
                var e = someNames.Select(x =>
                    new Domain.PrincipalClaim(
                        somePrincipalId, x)
                ).OrderBy(x => x.Schedule);

                Assert.IsTrue(e.SequenceEqual(r));
            }
        }

        private static Random Rng { get; } = new Random();
        private static long Start { get; } = (long)Rng.Next();
        private static long SomeStart() => (long)Rng.Next();
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

        Dictionary<string, (Guid id, string name)> AddCases =
            new Dictionary<string, (Guid id, string name)>
            {
                { "Existing", (KnownPrincipal, KnownName) },
                { "New", (KnownPrincipal, SomeName()) }
            };

        [TestMethod]
        [DataRow("Existing")]
        [DataRow("New")]
        public async Task CreateSchedule(string key)
        {
            var input = AddCases[key];

            using (var sut = new PgresRepo(dbConn()))
            {
                await sut.Add(
                    new Domain.PrincipalClaim(
                        input.id,
                        input.name));
            }

            using (var c = dbConn())
            {
                var added = await c.Schedules.Where(x =>
                    x.PrincipalId == input.id &&
                    x.Name == input.name)
                    .AnyAsync();

                Assert.IsTrue(added);
            }
        }

        [TestMethod]
        public async Task DeleteSchedule()
        {
            var claim = new Domain.PrincipalClaim(
                Guid.NewGuid(),
                SomeName()
            );

            using (var ctx = dbConn())
            {
                ctx.Add(new Data.Schedule
                {
                    PrincipalId = claim.Id,
                    Name = claim.Schedule
                });
                await ctx.SaveChangesAsync();
            }

            using (var sut = new PgresRepo(dbConn()))
            {
                await sut.Delete(claim);
            }

            using (var c = dbConn())
            {
                var added = await c.Schedules.Where(x =>
                    x.PrincipalId == claim.Id &&
                    x.Name == claim.Schedule)
                    .AnyAsync();

                Assert.IsFalse(added);
            }
        }

        public static Data.Participant[] ParticipantSeed =
            new[]
            {
                new Data.Participant
                {
                    SubjectId = Guid.NewGuid().ToString(),
                    Name = SomeName()
                },
                new Data.Participant
                {
                    SubjectId = Guid.NewGuid().ToString(),
                    Name = SomeName()
                }
            };

        public static long StartSeed { get; } = SomeStart();

        public static Domain.PrincipalClaim PutKnownClaim { get; } =
            new Domain.PrincipalClaim(
                Guid.NewGuid(),
                SomeName()
            );

        public static Data.Appointment AppointmentSeed = new Data.Appointment
        {
            Start = StartSeed,
            Participants = ParticipantSeed.ToList()
        };

        public static Domain.Appointment ExistingAppointment = new Domain.Appointment
        {
            Start = AppointmentSeed.Start,
            Participants = AppointmentSeed.Participants
                .Select(x => new Domain.Participant
                {
                    SubjectId = x.SubjectId,
                    Name = x.Name
                }).OrderBy(x => x.Name).ToList()
        };

        public static Data.Schedule PutScheduleSeed = new Data.Schedule
        {
            PrincipalId = PutKnownClaim.Id,
            Name = PutKnownClaim.Schedule,
            Appointments =
                    {
                        AppointmentSeed
                    }
        };


        public static Domain.Appointment Payload { get; } = new Domain.Appointment
        {
            Start = SomeStart(),
            Participants = Arbitrary.Participants()
                        .Select(x => new Domain.Participant
                        {
                            SubjectId = x.SubjectId,
                            Name = x.Name
                        })
                        .Take(2)
                        .ToList()
        };

        public static Dictionary<string, (Domain.PrincipalClaim c, Domain.Appointment appointments, Domain.AppointmentEvent e)> PutCases =
            new Dictionary<string, (Domain.PrincipalClaim c, Domain.Appointment appointments, Domain.AppointmentEvent e)>
            {
                ["UnknownClaim"] = (new Domain.PrincipalClaim(
                Guid.NewGuid(),
                SomeName()),
                new Domain.Appointment(),
                null),
                ["Created"] = (
                PutKnownClaim,
                Payload,
                new Domain.AppointmentEvent(
                    null,
                    Payload
                )),
                ["Updated"] = (
                PutKnownClaim,
                new Domain.Appointment
                {
                    Start = StartSeed,
                    Participants = Payload.Participants
                },
                new Domain.AppointmentEvent(
                    ExistingAppointment,
                    new Domain.Appointment
                    {
                        Start = StartSeed,
                        Participants = Payload.Participants
                            .ToList()
                    }
                )
            )
            };

        [TestMethod]
        [DataRow("UnknownClaim")]
        [DataRow("Created")]
        [DataRow("Updated")]
        public async Task PutAppointment(string key)
        {
            var testcase = PutCases[key];

            using (var sut = new PgresRepo(dbConn()))
            {
                var r = await sut.PutAppointment(
                    testcase.c,
                    testcase.appointments);

                Assert.AreEqual(testcase.e, r);
            }
        }
    }
}
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
using Db = Test.Records.DbLifeCycle;
using Appointments.Domain;

namespace Test.Repository
{
    [TestClass]
    public class Suite
    {
        public IScheduleRepository SutFactory() => new PgresRepo(Db.dbConn());

        [ClassInitialize]
        public static async Task Up(TestContext testctx)
        {
            using (var ctx = Db.dbConn())
            {
                ctx.AddRange(new[]
                {
                    Seed.ListSchedules(SchedulesOwner, KnownSchedules),
                    Seed.GetSchedules(
                        GetSchedulesPrincipal,
                        GetSchedulesName,
                        GetSchedulesAppointments),
                    Seed.PutSchedules(
                        PutKnownClaim.Id,
                        PutKnownClaim.Schedule,
                        new [] { AppointmentSeed }
                    )
                }.SelectMany(x => x));

                await ctx.SaveChangesAsync();
            }
        }
        static Guid SchedulesOwner { get; } = Guid.NewGuid();
        static string[] KnownSchedules { get; } = new[]
            {
                "adam",
                "bertil",
                "ceasar"
            };

        [TestMethod]
        public async Task ListSchedules()
        {
            using (var sut = SutFactory())
            {
                var r = await sut.List(SchedulesOwner);
                var e = KnownSchedules.Select(x =>
                    new Domain.PrincipalClaim(
                        SchedulesOwner, x)
                );

                Assert.IsTrue(e.SequenceEqual(r));
            }
        }

        private static Random Rng { get; } = new Random();
        private static long Start { get; } = (long)Rng.Next();
        private static long SomeStart() => (long)Rng.Next();
        private static Guid GetSchedulesPrincipal { get; } =
            Guid.NewGuid();

        private static string GetSchedulesName { get; } =
            "known";

        private static Data.Appointment[] GetSchedulesAppointments { get; } =
            Arbitrary.Appointments(
                Start,
                1000,
                2,
                GetSchedulesName).Take(3).ToArray();

        private static Domain.Appointment[] Expected { get; } =
            GetSchedulesAppointments.Select(x => new Domain.Appointment
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



        private static string SomeName() => Guid.NewGuid().ToString().Substring(0, 4);

        private static Dictionary<string, (Guid pId, string s, bool hit)> Cases =
            new Dictionary<string, (Guid pId, string s, bool hit)>
            {
                ["UK-UK"] = (Guid.NewGuid(), SomeName(), false),
                ["UK-KN"] = (Guid.NewGuid(), GetSchedulesName, false),
                ["KN-KN"] = (GetSchedulesPrincipal, GetSchedulesName, true)
            };

        [TestMethod]
        [DataRow("UK-UK")]
        [DataRow("UK-KN")]
        [DataRow("KN-KN")]
        public async Task GetSchedule(string caseId)
        {
            var input = Cases[caseId];

            using (var sut = SutFactory())
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

            using (var sut = SutFactory())
            {
                var r = await sut.PutAppointment(
                    testcase.c,
                    testcase.appointments);

                Assert.AreEqual(testcase.e, r);
            }
        }
    }
}
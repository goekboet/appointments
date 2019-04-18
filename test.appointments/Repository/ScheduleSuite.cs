using System;
using System.Linq;
using System.Threading.Tasks;
using Appointments.Records.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Db = Test.Records.DbLifeCycle;
using S = Appointments.Domain.Schedule;

namespace Test.Repository
{
    [TestClass]
    public class Suite
    {
        public S.IScheduleRepository SutFactory() => new PgresRepo(Db.dbConn());

        [DataRow("NotOnRecord")]
        [DataRow("OnRecord")]
        [TestMethod]
        public async Task ListSchedules(string key)
        {
            var testCase = ScheduleCases.ListSchedule(key);

            using (var db = Db.dbConn())
            {
                db.AddRange(testCase.Given);
                await db.SaveChangesAsync();
            }

            using (var sut = SutFactory())
            {
                var r = await sut.List(testCase.Arguments);

                Assert.IsTrue(testCase.Expect.SequenceEqual(r));
            }
        }

        [DataRow("NotOnRecord")]
        [DataRow("OnRecord")]
        [TestMethod]
        public async Task GetSchedule(string key)
        {
            var testCase = ScheduleCases.GetSchedule(key);

            using (var db = Db.dbConn())
            {
                db.AddRange(testCase.Given);
                await db.SaveChangesAsync();
            }

            using (var sut = SutFactory())
            {
                var r = await sut.Get(testCase.Arguments);

                Assert.IsTrue(testCase.Expect.SequenceEqual(r));
            }
        }

        [DataRow("UnknownClaim")]
        [DataRow("NotOnRecord")]
        [DataRow("OnRecord")]
        [TestMethod]
        public async Task PutSchedule(string key)
        {
            var testCase = ScheduleCases.PutAppointment(key);

            using (var db = Db.dbConn())
            {
                db.AddRange(testCase.Given);
                await db.SaveChangesAsync();
            }

            using (var sut = SutFactory())
            {
                var r = await sut.PostAppointment(
                    testCase.Arguments.Item1,
                    testCase.Arguments.Item2);

                Assert.AreEqual(testCase.Expect, r);
            }
        }

        [DataRow("UnknownClaim")]
        [DataRow("NotOnRecord")]
        [DataRow("OnRecord")]
        [TestMethod]
        public async Task DeleteAppointment(string key)
        {
            var testCase = ScheduleCases.DeleteAppointment(key);

            using (var db = Db.dbConn())
            {
                db.AddRange(testCase.Given);
                await db.SaveChangesAsync();
            }

            using (var sut = SutFactory())
            {
                var r = await sut.DeleteAppointment(
                    testCase.Arguments.Item1,
                    testCase.Arguments.Item2);

                Assert.AreEqual(testCase.Expect.ToString(), r.ToString());
            }

            using (var db = Db.dbConn())
            {
                var onRecord = await (from a in db.Appointments
                    where
                        a.Schedule.PrincipalId == testCase.Arguments.Item1.Id &&
                        a.ScheduleName == testCase.Arguments.Item1.Schedule &&
                        a.Start == testCase.Arguments.Item2
                    select a).AnyAsync();

                Assert.IsFalse(onRecord);   
            }
        }
    }
}
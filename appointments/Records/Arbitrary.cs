using System;
using System.Collections.Generic;
using System.Linq;

namespace Appointments.Records
{
    public static class Arbitrary
    {
        public static IEnumerable<Guid> PrincipalIds()
        {
            while (true)
            {
                yield return Guid.NewGuid();
            }
        }

        public static IEnumerable<string> UniqueNames()
        {
            while (true)
            {
                yield return Guid.NewGuid().ToString();
            }
        }

        public static IEnumerable<Participant> Participants()
        {
            return Enumerable.Zip(
                first: UniqueNames(),
                second: UniqueNames().Select(x => x.Substring(0, 4)),
                resultSelector: (id, name) => new Participant
                {
                    SubjectId = id,
                    Name = name
                });
        }

        public static IEnumerable<long> Starts(
            long from,
            int meanUntilNext
        )
        {
            var start = from;
            var next = 0;
            var span = meanUntilNext * 2;
            var rng = new Random();

            while (true)
            {
                next = rng.Next(span);
                start += next;
                yield return start;
            }
        }

        public static IEnumerable<Appointment> Appointments(
            long f,
            int meanUntilNext,
            int participants,
            string scheduleName
        )
        {
            return from s in Starts(f, meanUntilNext)
                   select new Appointment
                   {
                       ScheduleName = scheduleName,
                       Start = s,
                       Duration = 100,
                       Participants = Participants()
                           .Take(participants)
                           .ToList()
                   };

        }

        public static IEnumerable<Schedule> Schedules(
            long f,
            int meanUntilNext,
            int participants,
            int appointmentCount)
        {
            return Enumerable.Zip(
                first: PrincipalIds(),
                second: UniqueNames(),
                resultSelector: (id, name) => new Schedule
                {
                    PrincipalId = id,
                    Name = name,
                    Appointments = Appointments(f, meanUntilNext, participants, name)
                        .Take(appointmentCount)
                        .ToList()
                }
            );
        }
    }

}
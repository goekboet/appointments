using Appointments.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appointments.Records
{
    public sealed class SchedulesTable : IEntityTypeConfiguration<Schedule>
    {
        public void Configure(EntityTypeBuilder<Schedule> builder)
        {
            builder.ToTable("Schedule");

            builder.Property<long>("Id").UseNpgsqlIdentityAlwaysColumn();
            builder.HasKey("Id");

            builder.HasAlternateKey(x => new { x.PrincipalId, x.Name });

            builder.HasMany(x => x.Appointments)
                .WithOne()
                .HasForeignKey("ScheduleId");

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(256);
        }
    }

    public sealed class AppointmentTable : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointment");
            builder.Property<long>("Id").UseNpgsqlIdentityAlwaysColumn();
            builder.Property<long>("ScheduleId");

            builder.HasKey("Id");
            builder.HasMany(x => x.Participants)
                .WithOne(x => x.Appointment)
                .HasForeignKey("AppointmentId");

            builder.HasAlternateKey("ScheduleId", "Start");
        }
    }

    public sealed class ParticipantsTable : IEntityTypeConfiguration<Participant>
    {
        public void Configure(EntityTypeBuilder<Participant> builder)
        {
            builder.ToTable("Participant");
            builder.Property<long>("AppointmentId");
            builder.HasKey("AppointmentId", "SubjectId");
            builder.HasIndex("SubjectId");

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(256);
        }
    }
}
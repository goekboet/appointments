using Appointments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appointments.Records
{
    public sealed class SchedulesTable : IEntityTypeConfiguration<Schedule>
    {
        public void Configure(EntityTypeBuilder<Schedule> builder)
        {
            builder.HasKey(x => x.Name);
            
            builder.HasAlternateKey(x => x.Name);

            builder.Property(x => x.PrincipalId)
                .IsRequired();
            builder.HasIndex(x => x.PrincipalId);

            builder.Property(x => x.Name)
                .HasMaxLength(256);

            builder.HasMany(x => x.Appointments)
                .WithOne(x => x.Schedule)
                .HasForeignKey(x => x.ScheduleName);
        }
    }

    public sealed class AppointmentsTable : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.Property<long>("Id")
                .UseNpgsqlIdentityAlwaysColumn();
            builder.HasKey("Id");

            builder.HasAlternateKey(x => new 
            { 
                x.ScheduleName, 
                x.Start 
            });
            builder.HasIndex(x => x.Start);
            
            builder.Property(x => x.Duration)
                .IsRequired();

            builder.HasMany(x => x.Participants)
                .WithOne(x => x.Appointment)
                .HasForeignKey("AppointmentId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public sealed class ParticipantsTable : IEntityTypeConfiguration<Participant>
    {
        public void Configure(EntityTypeBuilder<Participant> builder)
        {
            builder.ToTable("Participant");
            builder.Property<long>("AppointmentId");
            
            builder.HasKey("AppointmentId", "SubjectId");
            
            builder.Property(x => x.Name)
                .HasMaxLength(256)
                .IsRequired();
            builder.HasIndex(x => x.Name);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SCG.Notification.Domain.Entities;

namespace SCG.Notification.Infrastructure.Persistence.Configurations;

internal sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("NotificationLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Channel).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Recipient).HasMaxLength(256).IsRequired();
        builder.Property(x => x.TemplateKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(500);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(x => x.Channel);
        builder.HasIndex(x => x.CreatedAt);
    }
}

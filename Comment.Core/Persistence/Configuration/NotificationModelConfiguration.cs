using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Comment.Core.Persistence.Configuration
{
    public class NotificationModelConfiguration : IEntityTypeConfiguration<NotificationModel>
    {
        public void Configure(EntityTypeBuilder<NotificationModel> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title).IsRequired();
            builder.Property(c => c.Message).IsRequired().HasMaxLength(50);
            builder.Property(c => c.Type).HasMaxLength(50);

            builder.Property(c => c.CreateAt);

            builder.HasIndex(c => c.RecipientId);
            builder.HasIndex(c => c.RecipientId, "IX_Notification_Recipient_IsRead")
           .IncludeProperties(c => c.IsRead);

            builder.HasOne(c => c.CreatorUser)
               .WithMany()
               .HasForeignKey(c => c.CreatorId)
               .OnDelete(DeleteBehavior.Restrict);
        }

        
    }
}

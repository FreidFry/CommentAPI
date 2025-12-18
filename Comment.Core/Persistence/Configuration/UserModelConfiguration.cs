using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Comment.Core.Persistence.Configuration
{
    public class UserModelConfiguration : IEntityTypeConfiguration<UserModel>
    {
        public void Configure(EntityTypeBuilder<UserModel> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
            builder.Property(u => u.HomePage);
            builder.Property(u => u.IsBanned).IsRequired();
            builder.Property(u => u.IsDeleted).IsRequired();
            builder.Property(u => u.AvatarUrl).IsRequired();
            builder.Property(u => u.AvatarTumbnailUrl).IsRequired();
            builder.Property(u => u.HashPassword).IsRequired();
            builder.Property(u => u.UserName).IsRequired().HasMaxLength(50);
            builder.Property(u => u.CreatedAt).IsRequired();
            builder.Property(builder => builder.LastActive).IsRequired();

            builder.HasMany(u => u.Comments)
                   .WithOne(c => c.User)
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.Threads)
                     .WithOne(t => t.OwnerUser)
                     .HasForeignKey(t => t.OwnerId)
                     .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.UserName).IsUnique();
        }
    }
}

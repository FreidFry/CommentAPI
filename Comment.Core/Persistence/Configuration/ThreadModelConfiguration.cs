using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Comment.Core.Persistence.Configuration
{
    public class ThreadModelConfiguration : IEntityTypeConfiguration<ThreadModel>
    {
        public void Configure(EntityTypeBuilder<ThreadModel> builder)
        {
            builder.ToTable("Threads");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Title).IsRequired();
            builder.Property(t => t.Context).IsRequired();
            builder.Property(t => t.CreatedAt).IsRequired();
            builder.HasOne(t => t.OwnerUser)
                   .WithMany(u => u.Threads)
                   .HasForeignKey(t => t.OwnerId);

            builder.HasMany(t => t.Comments)
                   .WithOne(c => c.Thread)
                   .HasForeignKey(c => c.ThreadId);
        }

    }
}

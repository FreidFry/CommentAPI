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
            builder.Property(t => t.LastUpdatedAt).IsRequired(false);
            builder.HasOne(t => t.OwnerUser)
                   .WithMany(u => u.Threads)
                   .HasForeignKey(t => t.OwnerId);
        }

    }
}

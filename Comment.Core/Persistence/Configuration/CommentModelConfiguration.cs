using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Comment.Core.Persistence.Configuration
{
    public class CommentModelConfiguration : IEntityTypeConfiguration<CommentModel>
    {
        public void Configure(EntityTypeBuilder<CommentModel> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Content).IsRequired();
            builder.Property(c => c.CreatedAt).IsRequired();

            builder.Property(c => c.IsDeleted).IsRequired();

            builder.HasOne(c => c.User)
                   .WithMany()
                   .HasForeignKey(c => c.UserId)
                   .IsRequired();
            builder.HasMany(c => c.Replyes)
                   .WithOne(c => c.ParentComment)
                   .HasForeignKey(c => c.ParentCommentId)
                   .IsRequired(false);

            builder.HasIndex(c => c.ThreadId);
            builder.HasIndex(c => c.ParentCommentId);
            builder.HasIndex(c => c.UserId);
        }
    }
}

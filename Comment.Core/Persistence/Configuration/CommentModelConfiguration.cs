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
            builder.Property(t => t.UpdatedAt).IsRequired(false);

            builder.Property(c => c.IsDeleted).IsRequired();

            builder.HasOne(c => c.User)
                   .WithMany()
                   .HasForeignKey(c => c.UserId)
                   .IsRequired();

            builder.HasIndex(c => c.ThreadId);
            builder.HasIndex(c => c.ParentCommentId);
            builder.HasIndex(c => c.UserId);
        }
    }
}

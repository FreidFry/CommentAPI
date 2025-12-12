using Comment.Core.Persistence;
using Comment.Core.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Comment.Core.Data
{
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the AppDbContext class using the specified options.
        /// </summary>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<ThreadModel> Threads { get; set; }
        public DbSet<CommentModel> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserModelConfiguration());
            modelBuilder.ApplyConfiguration(new ThreadModelConfiguration());
            modelBuilder.ApplyConfiguration(new CommentModelConfiguration());
        }
    }
}

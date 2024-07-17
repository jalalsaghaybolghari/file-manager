using FileServer.Models;
using FileServer.ViewModels.Interface;
using Microsoft.EntityFrameworkCore;

namespace FileServer.Database
{
    public class ApplicationDbContext : DbContext, IScopedDependency
    {
        public ApplicationDbContext(DbContextOptions options)
        : base(options)
        {

        }

        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Application> Applications { get; set; }
        public virtual DbSet<ApplicationApiKey> ApplicationApiKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Document>()
                .HasOne(bc => bc.Application)
                .WithMany(b => b.Documents)
                .HasForeignKey(bc => bc.ApplicationId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationApiKey>()
                .HasOne(bc => bc.Application)
                .WithMany(b => b.ApplicationApiKeys)
                .HasForeignKey(bc => bc.ApplicationId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Application>()
                .HasOne(bc => bc.User)
                .WithMany(b => b.Applications)
                .HasForeignKey(bc => bc.UserId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

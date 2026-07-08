using AI.TestCaseGenerator.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AI.TestCaseGenerator.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ==============================
        // DbSets
        // ==============================

        public DbSet<User> Users { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<DocumentChunk> DocumentChunks { get; set; }

        public DbSet<TestCase> TestCases { get; set; }

        public DbSet<ChatHistory> ChatHistories { get; set; }

        // ==============================
        // Fluent API
        // ==============================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------
            // User
            // -------------------------

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // -------------------------
            // Project
            // -------------------------

            modelBuilder.Entity<Project>()
                .HasOne(p => p.User)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // Document
            // -------------------------

            modelBuilder.Entity<Document>()
                .HasOne(d => d.Project)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // Document Chunk
            // -------------------------

            modelBuilder.Entity<DocumentChunk>()
                .HasOne(dc => dc.Document)
                .WithMany(d => d.Chunks)
                .HasForeignKey(dc => dc.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // Test Case
            // -------------------------

            modelBuilder.Entity<TestCase>()
                .HasOne(tc => tc.Project)
                .WithMany(p => p.TestCases)
                .HasForeignKey(tc => tc.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // Chat History
            // -------------------------

            modelBuilder.Entity<ChatHistory>()
                .HasOne(ch => ch.Project)
                .WithMany(p => p.ChatHistories)
                .HasForeignKey(ch => ch.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // Default Values
            // -------------------------

            

            modelBuilder.Entity<TestCase>()
                .Property(t => t.IsAiGenerated)
                .HasDefaultValue(true);
        }
    }
}
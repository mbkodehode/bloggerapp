using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<User> Users { get; set; }
    public DbSet<Blog> Blogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //  Explicitly configure the one-to-many relationship
        modelBuilder.Entity<User>()
          .HasMany(u => u.Blogs)
          .WithOne(b => b.Author)
          .HasForeignKey(b => b.AuthorId)
          .OnDelete(DeleteBehavior.Cascade); // Optional: Cascade delete when a user is removed

        base.OnModelCreating(modelBuilder);
    }

}
using Exam.Models;
using Microsoft.EntityFrameworkCore;

namespace Exam;

public class ExamContext : DbContext
{
    public ExamContext()
    {
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EFCoreExamDB;Integrated Security=True");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MyUser>()
            .HasIndex(x => x.Login)
            .IsUnique();

        modelBuilder.Entity<MyUser>()
                .Property(x => x.BDate)
                .HasConversion(x => x.ToDateTime(TimeOnly.MinValue), x => DateOnly.FromDateTime(x));

        modelBuilder.Entity<MyUser>()
                 .Property(x => x.RegDate)
                 .HasConversion(x => x.ToDateTime(TimeOnly.MinValue), x => DateOnly.FromDateTime(x));
    }
    public DbSet<MyUser> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
}

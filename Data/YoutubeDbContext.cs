using Microsoft.EntityFrameworkCore;
using YouTube.Models;

namespace YouTube.Data;


public class YouTubeDbContext : DbContext
{
    public YouTubeDbContext(DbContextOptions<YouTubeDbContext> options)
        : base(options)
    {
    }


    public DbSet<Account> Accounts { get; set; }
    public DbSet<Video> Videos { get; set; }
    public DbSet<Comment> Comments { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<Account>(entity =>
        {
            // Chave primária
            entity.HasKey(a => a.Id);

            // Propriedades obrigatórias
            entity.Property(a => a.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.CreatedAt)
                .IsRequired();


            entity.HasIndex(a => a.Username)
                .IsUnique();


            entity.HasIndex(a => a.Email)
                .IsUnique();


            entity.HasMany(a => a.Videos)
                .WithOne(v => v.Account)
                .HasForeignKey("AccountId")
                .OnDelete(DeleteBehavior.Cascade); 


            entity.HasMany(a => a.Comments)
                .WithOne(c => c.Author)
                .HasForeignKey("AuthorId")
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<Video>(entity =>
        {

            entity.HasKey(v => v.Id);


            entity.Property(v => v.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(v => v.Description)
                .HasMaxLength(5000); 

            entity.Property(v => v.DurationInSeconds)
                .IsRequired();

            entity.Property(v => v.Views)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(v => v.PublishedAt)
                .IsRequired();


            entity.HasIndex(v => v.Title);


            entity.HasIndex(v => v.Views);


            entity.HasMany(v => v.Comments)
                .WithOne(c => c.Video)
                .HasForeignKey("VideoId")
                .OnDelete(DeleteBehavior.Cascade); 
        });


        modelBuilder.Entity<Comment>(entity =>
        {
           
            entity.HasKey(c => c.Id);


            entity.Property(c => c.Text)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(c => c.CreatedAt)
                .IsRequired();

            entity.Property(c => c.Likes)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(c => c.Dislikes)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(c => c.IsEdited)
                .IsRequired()
                .HasDefaultValue(false);


            entity.HasIndex(c => c.CreatedAt);


        });


        SeedData(modelBuilder);
    }


    private void SeedData(ModelBuilder modelBuilder)
    {
        var account1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var account2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<Account>().HasData(
            new Account
            {
                Id = account1Id,
                Username = "TechMaster",
                Email = "techmaster@youtube.com",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Account
            {
                Id = account2Id,
                Username = "CodeQueen",
                Email = "codequeen@youtube.com",
                CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            }
        );


    }
}
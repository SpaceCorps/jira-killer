using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JiraKiller
{
    [Table("author")]
    public class Author
    {
        public Author()
        {
            Posts = new HashSet<Post>();
            Comments = new HashSet<Comment>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("email")]
        public string Email { get; set; } = null!;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Post> Posts { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
    }

    [Table("post")]
    public class Post
    {
        public Post()
        {
            Comments = new HashSet<Comment>();
            PostTags = new HashSet<PostTag>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("author_id")]
        public int AuthorId { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; } = null!;

        [Required]
        [Column("content")]
        public string Content { get; set; } = null!;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("AuthorId")]
        public virtual Author Author { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<PostTag> PostTags { get; set; }
    }

    [Table("comment")]
    public class Comment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("post_id")]
        public int PostId { get; set; }

        [Required]
        [Column("author_id")]
        public int AuthorId { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; } = null!;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("AuthorId")]
        public virtual Author Author { get; set; } = null!;
    }

    [Table("tag")]
    public class Tag
    {
        public Tag()
        {
            PostTags = new HashSet<PostTag>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<PostTag> PostTags { get; set; }
    }

    [Table("post_tag")]
    public class PostTag
    {
        [Required]
        [Column("post_id")]
        public int PostId { get; set; }

        [Required]
        [Column("tag_id")]
        public int TagId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; } = null!;
    }

    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<PostTag> PostTags { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.HasOne(d => d.Author)
                      .WithMany(p => p.Posts)
                      .HasForeignKey(d => d.AuthorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.HasOne(d => d.Post)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(d => d.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Author)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(d => d.AuthorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
            });

            modelBuilder.Entity<PostTag>(entity =>
            {
                entity.HasKey(e => new { e.PostId, e.TagId });
                entity.HasOne(d => d.Post)
                      .WithMany(p => p.PostTags)
                      .HasForeignKey(d => d.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.Tag)
                      .WithMany(p => p.PostTags)
                      .HasForeignKey(d => d.TagId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
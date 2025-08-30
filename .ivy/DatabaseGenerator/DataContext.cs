using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JiraKiller
{
    public enum Priority
    {
        low,
        medium,
        high,
        critical
    }

    public enum TicketStatus
    {
        open,
        in_progress,
        completed,
        blocked,
        closed
    }

    [Table("user")]
    public class User
    {
        public User()
        {
            AssignedTickets = new HashSet<Ticket>();
            TimeLogs = new HashSet<TimeLog>();
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

        public virtual ICollection<Ticket> AssignedTickets { get; set; }
        public virtual ICollection<TimeLog> TimeLogs { get; set; }
    }

    [Table("project")]
    public class Project
    {
        public Project()
        {
            Tickets = new HashSet<Ticket>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }
    }

    [Table("ticket")]
    public class Ticket
    {
        public Ticket()
        {
            TimeLogs = new HashSet<TimeLog>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("project_id")]
        public int ProjectId { get; set; }

        [Column("assigned_user_id")]
        public int? AssignedUserId { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("priority")]
        public Priority Priority { get; set; }

        [Required]
        [Column("status")]
        public TicketStatus Status { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;

        [ForeignKey("AssignedUserId")]
        public virtual User? AssignedUser { get; set; }

        public virtual ICollection<TimeLog> TimeLogs { get; set; }
    }

    [Table("time_log")]
    public class TimeLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("ticket_id")]
        public int TicketId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("hours")]
        public decimal Hours { get; set; }

        [Required]
        [Column("logged_at")]
        public DateTime LoggedAt { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }

    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<TimeLog> TimeLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("project");
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("ticket");
                entity.HasOne(d => d.Project)
                      .WithMany(p => p.Tickets)
                      .HasForeignKey(d => d.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.AssignedUser)
                      .WithMany(p => p.AssignedTickets)
                      .HasForeignKey(d => d.AssignedUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<TimeLog>(entity =>
            {
                entity.ToTable("time_log");
                entity.Property(e => e.Hours).HasColumnType("decimal(5,2)");
                entity.HasOne(d => d.Ticket)
                      .WithMany(p => p.TimeLogs)
                      .HasForeignKey(d => d.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(d => d.User)
                      .WithMany(p => p.TimeLogs)
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
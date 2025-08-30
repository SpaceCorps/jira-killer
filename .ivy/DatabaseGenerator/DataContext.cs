using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Test5
{
    public enum TicketPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum TicketStatus
    {
        Open,
        InProgress,
        Resolved,
        Closed,
        Blocked
    }

    [Index(nameof(Email), IsUnique = true)]
    [Table("user")]
    public class User
    {
        public User()
        {
            ProjectsCreated = new HashSet<Project>();
            TicketsCreated = new HashSet<Ticket>();
            TicketsAssigned = new HashSet<Ticket>();
            TimeEntries = new HashSet<TimeEntry>();
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
        [Column("role")]
        public string Role { get; set; } = null!;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [ConcurrencyCheck]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Project> ProjectsCreated { get; set; }
        public virtual ICollection<Ticket> TicketsCreated { get; set; }
        public virtual ICollection<Ticket> TicketsAssigned { get; set; }
        public virtual ICollection<TimeEntry> TimeEntries { get; set; }
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

        [Required]
        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [ConcurrencyCheck]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; } = null!;

        public virtual ICollection<Ticket> Tickets { get; set; }
    }

    [Table("ticket")]
    public class Ticket
    {
        public Ticket()
        {
            TimeEntries = new HashSet<TimeEntry>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("project_id")]
        public int ProjectId { get; set; }

        [Required]
        [Column("priority")]
        public TicketPriority Priority { get; set; }

        [Required]
        [Column("status")]
        public TicketStatus Status { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Required]
        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("assigned_to")]
        public int? AssignedTo { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [ConcurrencyCheck]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; } = null!;

        [ForeignKey("AssignedTo")]
        public virtual User? AssignedToUser { get; set; }

        public virtual ICollection<TimeEntry> TimeEntries { get; set; }
    }

    [Table("time_entry")]
    public class TimeEntry
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
        [ConcurrencyCheck]
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
        public DbSet<TimeEntry> TimeEntries { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .HasOne(p => p.CreatedByUser)
                .WithMany(u => u.ProjectsCreated)
                .HasForeignKey(p => p.CreatedBy)
                .HasConstraintName("fk_project_created_by");

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.ProjectId)
                .HasConstraintName("fk_ticket_project_id");

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.CreatedByUser)
                .WithMany(u => u.TicketsCreated)
                .HasForeignKey(t => t.CreatedBy)
                .HasConstraintName("fk_ticket_created_by");

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.AssignedToUser)
                .WithMany(u => u.TicketsAssigned)
                .HasForeignKey(t => t.AssignedTo)
                .HasConstraintName("fk_ticket_assigned_to");

            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.Ticket)
                .WithMany(t => t.TimeEntries)
                .HasForeignKey(te => te.TicketId)
                .HasConstraintName("fk_time_entry_ticket_id");

            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.User)
                .WithMany(u => u.TimeEntries)
                .HasForeignKey(te => te.UserId)
                .HasConstraintName("fk_time_entry_user_id");
        }
    }
}
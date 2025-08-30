using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace JiraKiller.Connections.JiraKiller;

public partial class JiraKillerContext : DbContext
{
    public JiraKillerContext(DbContextOptions<JiraKillerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TimeLog> TimeLogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasOne(d => d.AssignedUser).WithMany(p => p.Tickets).OnDelete(DeleteBehavior.SetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

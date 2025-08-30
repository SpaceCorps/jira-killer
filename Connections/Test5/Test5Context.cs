using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Test5.Connections.Test5;

public partial class Test5Context : DbContext
{
    public Test5Context(DbContextOptions<Test5Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TimeEntry> TimeEntries { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

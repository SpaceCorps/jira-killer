using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Test5.Connections.Test5;

[Table("project")]
[Index("CreatedBy", Name = "IX_project_created_by")]
public partial class Project
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Projects")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [InverseProperty("Project")]
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

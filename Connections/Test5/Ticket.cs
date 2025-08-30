using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Test5.Connections.Test5;

[Table("ticket")]
[Index("AssignedTo", Name = "IX_ticket_assigned_to")]
[Index("CreatedBy", Name = "IX_ticket_created_by")]
[Index("ProjectId", Name = "IX_ticket_project_id")]
public partial class Ticket
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("project_id")]
    public int ProjectId { get; set; }

    [Column("priority")]
    public int Priority { get; set; }

    [Column("status")]
    public int Status { get; set; }

    [Column("due_date")]
    public DateTime? DueDate { get; set; }

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("assigned_to")]
    public int? AssignedTo { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("AssignedTo")]
    [InverseProperty("TicketAssignedToNavigations")]
    public virtual User? AssignedToNavigation { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("TicketCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; } = null!;

    [ForeignKey("ProjectId")]
    [InverseProperty("Tickets")]
    public virtual Project Project { get; set; } = null!;

    [InverseProperty("Ticket")]
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}

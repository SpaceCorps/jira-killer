using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace JiraKiller.Connections.JiraKiller;

[Table("ticket")]
[Index("AssignedUserId", Name = "IX_ticket_assigned_user_id")]
[Index("ProjectId", Name = "IX_ticket_project_id")]
public partial class Ticket
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("project_id")]
    public int ProjectId { get; set; }

    [Column("assigned_user_id")]
    public int? AssignedUserId { get; set; }

    [Column("title")]
    public string Title { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("priority")]
    public int Priority { get; set; }

    [Column("status")]
    public int Status { get; set; }

    [Column("due_date")]
    public DateTime? DueDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("AssignedUserId")]
    [InverseProperty("Tickets")]
    public virtual User? AssignedUser { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("Tickets")]
    public virtual Project Project { get; set; } = null!;

    [InverseProperty("Ticket")]
    public virtual ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
}

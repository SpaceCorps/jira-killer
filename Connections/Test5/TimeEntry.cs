using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Test5.Connections.Test5;

[Table("time_entry")]
[Index("TicketId", Name = "IX_time_entry_ticket_id")]
[Index("UserId", Name = "IX_time_entry_user_id")]
public partial class TimeEntry
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("ticket_id")]
    public int TicketId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("hours")]
    public decimal Hours { get; set; }

    [Column("logged_at")]
    public DateTime LoggedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("TicketId")]
    [InverseProperty("TimeEntries")]
    public virtual Ticket Ticket { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("TimeEntries")]
    public virtual User User { get; set; } = null!;
}

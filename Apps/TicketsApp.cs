using System.ComponentModel.DataAnnotations;
using Test5.Connections.Test5;

namespace Test5.Apps;

[App(icon: Icons.Ticket, path: ["Apps"])]
public class TicketsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new TicketListBlade(), "Search");
    }
}

public class TicketListBlade : ViewBase
{
    private record TicketListRecord(int Id, string Title, string? ProjectName);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<Test5ContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int ticketId)
            {
                blades.Pop(this, true);
                blades.Push(this, new TicketDetailsBlade(ticketId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var ticket = (TicketListRecord)e.Sender.Tag!;
            blades.Push(this, new TicketDetailsBlade(ticket.Id), ticket.Title);
        });

        ListItem CreateItem(TicketListRecord record) =>
            new(title: record.Title, subtitle: record.ProjectName, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("Create Ticket").ToTrigger((isOpen) => new TicketCreateDialog(isOpen, refreshToken));

        return new FilteredListView<TicketListRecord>(
            fetchRecords: (filter) => FetchTickets(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<TicketListRecord[]> FetchTickets(Test5ContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Tickets.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Title.Contains(filter) || e.Project.Name.Contains(filter));
        }

        return await linq
            .OrderByDescending(e => e.CreatedAt)
            .Take(50)
            .Select(e => new TicketListRecord(e.Id, e.Title, e.Project.Name))
            .ToArrayAsync();
    }
}

public class TicketDetailsBlade(int ticketId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<Test5ContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var ticket = UseState<Ticket?>();
        var timeEntryCount = UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            ticket.Set(await db.Tickets.Include(e => e.Project).Include(e => e.AssignedToNavigation).SingleOrDefaultAsync(e => e.Id == ticketId));
            timeEntryCount.Set(await db.TimeEntries.CountAsync(e => e.TicketId == ticketId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (ticket.Value == null) return null;

        var ticketValue = ticket.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this ticket?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Ticket", AlertButtonSet.OkCancel);
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new TicketEditSheet(isOpen, refreshToken, ticketId));

        var detailsCard = new Card(
            content: new
                {
                    ticketValue.Id,
                    ticketValue.Title,
                    ticketValue.Description,
                    ProjectName = ticketValue.Project.Name,
                    AssignedTo = ticketValue.AssignedToNavigation?.Name,
                    ticketValue.Priority,
                    ticketValue.Status,
                    ticketValue.DueDate
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Ticket Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Time Entries", onClick: _ =>
                {
                    blades.Push(this, new TicketTimeEntriesBlade(ticketId), "Time Entries");
                }, badge: timeEntryCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(Test5ContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var ticket = db.Tickets.FirstOrDefault(e => e.Id == ticketId)!;
        db.Tickets.Remove(ticket);
        db.SaveChanges();
    }
}

public class TicketCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record TicketCreateRequest
    {
        [Required]
        public string Title { get; init; } = "";

        public string? Description { get; init; }

        [Required]
        public int? ProjectId { get; init; } = null;

        [Required]
        public int Priority { get; init; }

        [Required]
        public int Status { get; init; }

        public DateTime? DueDate { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<Test5ContextFactory>();
        var ticket = UseState(() => new TicketCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                var ticketId = CreateTicket(factory, ticket.Value);
                refreshToken.Refresh(ticketId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [ticket]);

        return ticket
            .ToForm()
            .Builder(e => e.ProjectId, e => e.ToAsyncSelectInput(QueryProjects(factory), LookupProject(factory), placeholder: "Select Project"))
            .Builder(e => e.Priority, e => e.ToFeedbackInput())
            .Builder(e => e.Status, e => e.ToFeedbackInput())
            .ToDialog(isOpen, title: "Create Ticket", submitTitle: "Create");
    }

    private int CreateTicket(Test5ContextFactory factory, TicketCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var ticket = new Ticket()
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId!.Value,
            Priority = request.Priority,
            Status = request.Status,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = 0 
        };

        db.Tickets.Add(ticket);
        db.SaveChanges();

        return ticket.Id;
    }

    private static AsyncSelectQueryDelegate<int?> QueryProjects(Test5ContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Projects
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupProject(Test5ContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var project = await db.Projects.FirstOrDefaultAsync(e => e.Id == id);
            if (project == null) return null;
            return new Option<int?>(project.Name, project.Id);
        };
    }
}

public class TicketEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int ticketId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<Test5ContextFactory>();
        var ticket = UseState(() => factory.CreateDbContext().Tickets.FirstOrDefault(e => e.Id == ticketId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                ticket.Value.UpdatedAt = DateTime.UtcNow;
                db.Tickets.Update(ticket.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [ticket]);

        return ticket
            .ToForm()
            .Builder(e => e.Priority, e => e.ToFeedbackInput())
            .Builder(e => e.Description, e => e.ToTextAreaInput())
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt)
            .Builder(e => e.ProjectId, e => e.ToAsyncSelectInput(QueryProjects(factory), LookupProject(factory), placeholder: "Select Project"))
            .Builder(e => e.AssignedTo, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select User"))
            .ToSheet(isOpen, "Edit Ticket");
    }

    private static AsyncSelectQueryDelegate<int?> QueryProjects(Test5ContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Projects
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupProject(Test5ContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var project = await db.Projects.FirstOrDefaultAsync(e => e.Id == id);
            if (project == null) return null;
            return new Option<int?>(project.Name, project.Id);
        };
    }

    private static AsyncSelectQueryDelegate<int?> QueryUsers(Test5ContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Users
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupUser(Test5ContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var user = await db.Users.FirstOrDefaultAsync(e => e.Id == id);
            if (user == null) return null;
            return new Option<int?>(user.Name, user.Id);
        };
    }
}

public class TicketTimeEntriesBlade(int ticketId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<Test5ContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var timeEntries = this.UseState<TimeEntry[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            timeEntries.Set(await db.TimeEntries.Include(e => e.User).Where(e => e.TicketId == ticketId).ToArrayAsync());
        }, [ EffectTrigger.AfterInit(), refreshToken ]);

        Action OnDelete(int id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this time entry?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete Time Entry", AlertButtonSet.OkCancel);
            };
        }

        if (timeEntries.Value == null) return null;

        var table = timeEntries.Value.Select(e => new
            {
                UserName = e.User.Name,
                Hours = e.Hours,
                LoggedAt = e.LoggedAt,
                _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(e.Id)))
                    | Icons.Pencil
                        .ToButton()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new TicketTimeEntriesEditSheet(isOpen, refreshToken, e.Id))
            })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Time Entry").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new TicketTimeEntriesCreateDialog(isOpen, refreshToken, ticketId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(Test5ContextFactory factory, int timeEntryId)
    {
        using var db = factory.CreateDbContext();
        db.TimeEntries.Remove(db.TimeEntries.Single(e => e.Id == timeEntryId));
        db.SaveChanges();
    }
}

public class TicketTimeEntriesCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, int ticketId) : ViewBase
{
    private record TimeEntryCreateRequest
    {
        [Required]
        public int UserId { get; init; }

        [Required]
        public decimal Hours { get; init; }

        [Required]
        public DateTime LoggedAt { get; init; } = DateTime.UtcNow;
    }

    public override object? Build()
    {
        var factory = UseService<Test5ContextFactory>();
        var timeEntry = UseState(() => new TimeEntryCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                var timeEntryId = CreateTimeEntry(factory, timeEntry.Value);
                refreshToken.Refresh(timeEntryId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [timeEntry]);

        return timeEntry
            .ToForm()
            .Builder(e => e.UserId, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select User"))
            .ToDialog(isOpen, title: "Create Time Entry", submitTitle: "Create");
    }

    private int CreateTimeEntry(Test5ContextFactory factory, TimeEntryCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var timeEntry = new TimeEntry
        {
            TicketId = ticketId,
            UserId = request.UserId,
            Hours = request.Hours,
            LoggedAt = request.LoggedAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.TimeEntries.Add(timeEntry);
        db.SaveChanges();

        return timeEntry.Id;
    }

    private static AsyncSelectQueryDelegate<int> QueryUsers(Test5ContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Users
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int> LookupUser(Test5ContextFactory factory)
    {
        return async id =>
        {
            await using var db = factory.CreateDbContext();
            var user = await db.Users.FirstOrDefaultAsync(e => e.Id == id);
            if (user == null) return null;
            return new Option<int>(user.Name, user.Id);
        };
    }
}

public class TicketTimeEntriesEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int timeEntryId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<Test5ContextFactory>();
        var timeEntry = UseState(() => factory.CreateDbContext().TimeEntries.FirstOrDefault(e => e.Id == timeEntryId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                timeEntry.Value.UpdatedAt = DateTime.UtcNow;
                db.TimeEntries.Update(timeEntry.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [timeEntry]);

        return timeEntry
            .ToForm()
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.TicketId)
            .Builder(e => e.Hours, e => e.ToFeedbackInput())
            .ToSheet(isOpen, "Edit Time Entry");
    }
}
using Scheduler.Application;
using Scheduler.Domain.Event;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Scheduler.Api.Tests;

/// <summary>
/// End-to-end tests through the full HTTP pipeline: routing, model binding,
/// validation, exception handling, EF Core and SQL Server.
/// They focus on behaviour the domain tests can't see: status codes,
/// persistence, filtering and optimistic concurrency.
/// </summary>
public class EventsApiTests(SchedulerApiFactory factory) : IClassFixture<SchedulerApiFactory>
{
    private readonly HttpClient _http = factory.CreateClient();

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static object NewEvent(string title = "Check-up", string email = "jane@example.com") => new
    {
        title,
        description = "Annual check-up",
        start = "2026-11-01T10:00:00+02:00",
        end = "2026-11-01T10:30:00+02:00",
        attendees = new[] { new { name = "Jane Doe", email } }
    };

    private async Task<EventDto> CreateEventAsync(object body)
    {
        var response = await _http.PostAsJsonAsync("/api/events", body, Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<EventDto>(Json))!;
    }

    // ---------- Create / Get ----------

    [Fact]
    public async Task Create_ThenGet_ReturnsPersistedEvent()
    {
        var created = await CreateEventAsync(NewEvent());

        var fetched = await _http.GetFromJsonAsync<EventDto>($"/api/events/{created.Id}", Json);

        Assert.NotNull(fetched);
        Assert.Equal("Check-up", fetched.Title);
        Assert.Single(fetched.Attendees);
    }

    [Fact]
    public async Task Create_ReturnsLocationHeader()
    {
        var response = await _http.PostAsJsonAsync("/api/events", NewEvent(), Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Create_WithMissingTitle_Returns400()
    {
        var body = new { description = "x", start = "2026-11-01T10:00:00Z", end = "2026-11-01T11:00:00Z" };

        var response = await _http.PostAsJsonAsync("/api/events", body, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithTitleTooLong_Returns400()
    {
        var response = await _http.PostAsJsonAsync("/api/events", NewEvent(title: new string('a', 500)), Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithEndBeforeStart_Returns400()
    {
        var body = new
        {
            title = "Backwards",
            description = "x",
            start = "2026-11-01T11:00:00Z",
            end = "2026-11-01T10:00:00Z",
            attendees = Array.Empty<object>()
        };

        var response = await _http.PostAsJsonAsync("/api/events", body, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_UnknownId_Returns404()
    {
        var response = await _http.GetAsync($"/api/events/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- Update & concurrency ----------

    [Fact]
    public async Task Update_WithCurrentVersion_Succeeds()
    {
        var created = await CreateEventAsync(NewEvent());

        var response = await _http.PutAsJsonAsync($"/api/events/{created.Id}", new
        {
            title = "Rescheduled",
            description = "Moved",
            start = "2026-11-02T10:00:00Z",
            end = "2026-11-02T10:30:00Z",
            version = created.Version
        }, Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<EventDto>(Json);
        Assert.Equal("Rescheduled", updated!.Title);
        Assert.True(updated.Version > created.Version);
    }

    [Fact]
    public async Task Update_WithStaleVersion_Returns409()
    {
        // Two users read the same version; the first save wins, the second is rejected.
        var created = await CreateEventAsync(NewEvent());
        object Update(string title) => new
        {
            title,
            description = "x",
            start = "2026-11-02T10:00:00Z",
            end = "2026-11-02T10:30:00Z",
            version = created.Version
        };

        var first = await _http.PutAsJsonAsync($"/api/events/{created.Id}", Update("First"), Json);
        var second = await _http.PutAsJsonAsync($"/api/events/{created.Id}", Update("Second"), Json);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    // ---------- Cancel ----------

    [Fact]
    public async Task Cancel_SoftDeletes_EventStillRetrievable()
    {
        var created = await CreateEventAsync(NewEvent());

        var response = await _http.DeleteAsync($"/api/events/{created.Id}");
        var fetched = await _http.GetFromJsonAsync<EventDto>($"/api/events/{created.Id}", Json);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(EventStatus.Cancelled, fetched!.Status);
    }

    // ---------- List / search ----------

    [Fact]
    public async Task List_Search_ReturnsOnlyMatchingEvents()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        await CreateEventAsync(NewEvent(title: $"Dentist {marker}"));
        await CreateEventAsync(NewEvent(title: "Something else"));

        var results = await _http.GetFromJsonAsync<List<EventDto>>($"/api/events?search={marker}", Json);

        Assert.Single(results!);
        Assert.Contains(marker, results![0].Title);
    }

    [Fact]
    public async Task List_ExcludesCancelledByDefault()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        var created = await CreateEventAsync(NewEvent(title: $"Cancel me {marker}"));
        await _http.DeleteAsync($"/api/events/{created.Id}");

        var hidden = await _http.GetFromJsonAsync<List<EventDto>>($"/api/events?search={marker}", Json);
        var shown = await _http.GetFromJsonAsync<List<EventDto>>($"/api/events?search={marker}&includeCancelled=true", Json);

        Assert.Empty(hidden!);
        Assert.Single(shown!);
    }

    [Fact]
    public async Task List_WithTakeOverLimit_Returns400()
    {
        var response = await _http.GetAsync("/api/events?take=1000");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- Respond ----------


    [Fact]
    public async Task Respond_WithUnknownEmail_Returns400()
    {
        var created = await CreateEventAsync(NewEvent());

        var response = await _http.PostAsJsonAsync($"/api/events/{created.Id}/respond",
            new { email = "stranger@example.com", accept = true }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
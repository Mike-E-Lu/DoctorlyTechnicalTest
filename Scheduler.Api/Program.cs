using Scalar.AspNetCore;
using Scheduler.Api;
using Scheduler.Infrastructure;
using Scheduler.Infrastructure.Persistence;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Scheduler"));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionToProblemDetailsHandler>();
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.SerializerOptions.NumberHandling = JsonNumberHandling.Strict; // keeps numbers typed as plain integers in OpenAPI
});

builder.Services.AddOpenApi(o =>
{
    // 3.0 rather than 3.1 so that client generators (NSwag, Kiota, openapi-generator) all handle it.
    o.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
    // Enums are serialised as strings; say so in the schema so generated clients do the same.
    o.AddSchemaTransformer((schema, context, _) =>
    {
        if (context.JsonTypeInfo.Type.IsEnum) schema.Type = Microsoft.OpenApi.JsonSchemaType.String;
        return Task.CompletedTask;
    });
    o.AddDocumentTransformer((doc, _, _) =>
    {
        doc.Info.Title = "Scheduler API";
        doc.Info.Description = "Manage a practice's calendar: events, attendees and their responses.";
        return Task.CompletedTask;
    });
});


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeDatabaseAsync();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapOpenApi();                 // /openapi/v1.json
app.MapScalarApiReference();      // /scalar
app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

app.Run();

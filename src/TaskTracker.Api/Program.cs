using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.Errors;
using TaskTracker.Api.Services;
using TaskTracker.Application;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Infrastructure;
using TaskTracker.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found.");

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)
    // Model binding failures (malformed JSON, wrong types) use the same Error shape as every other 400.
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory =
        context => new BadRequestObjectResult(ErrorMapper.FromModelState(context.ModelState)));
// Responses written outside MVC (the exception handler) use these options.
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

// First in the pipeline so it catches exceptions from everything after it.
// The empty delegate is required because ProblemDetails is not registered; GlobalExceptionHandler does the work.
app.UseExceptionHandler(_ => { });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

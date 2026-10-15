using Mundialito.Api.Endpoints;
using Mundialito.Application.Features.Matches.Commands;
using Mundialito.Application.Features.Matches.Queries;
using Mundialito.Application.Features.Players.Commands;
using Mundialito.Application.Features.Players.Queries;
using Mundialito.Application.Features.Teams.Commands;
using Mundialito.Application.Features.Teams.Queries;
using Mundialito.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<CreateTeamCommandHandler>();
builder.Services.AddScoped<UpdateTeamCommandHandler>();
builder.Services.AddScoped<DeleteTeamCommandHandler>();
builder.Services.AddScoped<CreatePlayerCommandHandler>();
builder.Services.AddScoped<UpdatePlayerCommandHandler>();
builder.Services.AddScoped<DeletePlayerCommandHandler>();
builder.Services.AddScoped<CreateMatchCommandHandler>();
builder.Services.AddScoped<UpdateMatchCommandHandler>();
builder.Services.AddScoped<DeleteMatchCommandHandler>();
builder.Services.AddScoped<RegisterGoalCommandHandler>();
builder.Services.AddScoped<GetTeamsQueryHandler>();
builder.Services.AddScoped<GetTeamByIdQueryHandler>();
builder.Services.AddScoped<GetPlayersQueryHandler>();
builder.Services.AddScoped<GetPlayerByIdQueryHandler>();
builder.Services.AddScoped<GetMatchesQueryHandler>();
builder.Services.AddScoped<GetMatchByIdQueryHandler>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins(builder.Configuration["WebOrigin"] ?? "http://localhost:3001")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (HttpMethods.IsPost(context.Request.Method))
    {
        context.Request.EnableBuffering();
    }

    await next(context);
});

if (builder.Configuration.GetValue<bool>("InitializeDatabase"))
{
    await app.Services.InitializeDatabaseAsync();
}

app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapTeamEndpoints();
app.MapPlayerEndpoints();
app.MapMatchEndpoints();

app.Run();


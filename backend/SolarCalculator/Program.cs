using SolarCalculator.Services;
using SolarCalculator.Models;

var builder = WebApplication.CreateBuilder(args);

// REGISTRATION
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// CUSTOM SERVICES
builder.Services.AddScoped<ISolarCalculationService, SolarCalculationService>();
builder.Services.AddScoped<ISolarLeadService, SolarLeadService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient("OsmClient", c => c.DefaultRequestHeaders.Add("User-Agent", "SolarWidget/1.0"));

var app = builder.Build();

// MIDDLEWARE
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

// ENDPOINTS
app.MapPost("/api/solar/calculate", async (MapRequest req, ISolarLeadService solarService) =>
{
    var result = await solarService.GetPotentialAsync(req.Latitude, req.Longitude);
    return result != null ? Results.Ok(result) : Results.NotFound("Building not found.");
});

app.MapPost("/api/solar/submit-lead", async (LeadRequest req, IEmailService emailService) =>
{
    await emailService.SendLeadEmailAsync(req);
    return Results.Ok();
});

app.Run();
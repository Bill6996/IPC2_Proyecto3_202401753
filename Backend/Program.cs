using Backend.Services;
using QuestPDF.Infrastructure;

// Licencia gratuita de QuestPDF (community)
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ITGSA API",
        Version = "v1",
        Description = "Backend del sistema de facturación y pagos — Industria Típica Guatemalteca S.A."
    });
});

builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5000")
              .AllowAnyMethod()
              .AllowAnyHeader()));

// Inyección de dependencias
builder.Services.AddSingleton<XmlDataService>();
builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<TransaccionService>();
builder.Services.AddScoped<EstadoCuentaService>();
builder.Services.AddScoped<PdfService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ITGSA API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
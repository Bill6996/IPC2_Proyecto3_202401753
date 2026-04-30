using Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Registrar HttpClient apuntando al backend
builder.Services.AddHttpClient("Backend", client =>
{
    var backendUrl = builder.Configuration["BackendUrl"]
                    ?? "http://localhost:5001/api/";
    client.BaseAddress = new Uri(backendUrl);
});

builder.Services.AddScoped<ApiService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
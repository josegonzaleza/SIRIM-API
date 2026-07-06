var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

app.MapGet("/", () => Results.Ok(new
{
    app = "SIRIM API",
    estado = "funcionando",
    endpoints = new[]
    {
        "/api/manglares/ping",
        "/api/manglares",
        "/api/manglares/sync"
    }
}));

app.MapControllers();

app.Run();

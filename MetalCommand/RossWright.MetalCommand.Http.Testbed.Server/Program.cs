var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5100");

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));
app.MapGet("/ping", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));
app.MapPost("/login", () => Results.Ok(new { token = "test-token" }));

app.Run();

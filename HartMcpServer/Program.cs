using HartMcpServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "HART GitHub MCP Server", 
        Version = "v1",
        Description = "GitHub Model Context Protocol (MCP) Server with Server-Sent Events support for HART-IP Client"
    });
});

// Add CORS for browser clients
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register MCP service
builder.Services.AddSingleton<GitHubMcpService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HART GitHub MCP Server v1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });
}

// Enable static files
app.UseStaticFiles();

app.UseCors();
app.UseRouting();
app.MapControllers();

// Add some basic endpoints for testing
app.MapGet("/", () => new
{
    name = "HART GitHub MCP Server",
    version = "1.0.0",
    description = "GitHub Model Context Protocol Server with Server-Sent Events support",
    endpoints = new
    {
        capabilities = "/api/mcp/capabilities",
        tools = "/api/mcp/tools",
        sse = "/api/mcp/sse",
        callTool = "/api/mcp/call-tool",
        sseCallTool = "/api/mcp/sse/call-tool"
    },
    testPage = "/test.html",
    documentation = "/swagger"
});

app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("HART GitHub MCP Server starting up...");

app.Run();

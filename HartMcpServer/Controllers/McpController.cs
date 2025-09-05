using HartMcpServer.Models;
using HartMcpServer.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace HartMcpServer.Controllers
{
    /// <summary>
    /// MCP Controller that handles GitHub MCP server requests via Server-Sent Events
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class McpController : ControllerBase
    {
        private readonly GitHubMcpService _mcpService;
        private readonly ILogger<McpController> _logger;

        public McpController(GitHubMcpService mcpService, ILogger<McpController> logger)
        {
            _mcpService = mcpService;
            _logger = logger;
        }

        /// <summary>
        /// Get MCP server capabilities and available tools
        /// </summary>
        [HttpGet("capabilities")]
        public IActionResult GetCapabilities()
        {
            var capabilities = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    tools = new { },
                    logging = new { }
                },
                serverInfo = new
                {
                    name = "hart-github-mcp-server",
                    version = "1.0.0"
                }
            };

            return Ok(capabilities);
        }

        /// <summary>
        /// Get list of available tools
        /// </summary>
        [HttpGet("tools")]
        public IActionResult GetTools()
        {
            var tools = _mcpService.GetTools();
            var response = new McpResponse
            {
                Id = Guid.NewGuid().ToString(),
                Result = new { tools = tools }
            };

            return Ok(response);
        }

        /// <summary>
        /// Server-Sent Events endpoint for MCP communication
        /// </summary>
        [HttpGet("sse")]
        public async Task SseEndpoint()
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";
            Response.Headers["Access-Control-Allow-Origin"] = "*";
            Response.Headers["Access-Control-Allow-Headers"] = "Cache-Control";

            _logger.LogInformation("SSE connection established");

            try
            {
                // Send initial server info
                await SendSseEventAsync("server-info", new
                {
                    name = "hart-github-mcp-server",
                    version = "1.0.0",
                    protocolVersion = "2024-11-05",
                    timestamp = DateTime.UtcNow
                });

                // Send available tools
                await SendSseEventAsync("tools", new { tools = _mcpService.GetTools() });

                // Keep connection alive and handle disconnection
                var cancellationToken = HttpContext.RequestAborted;
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Send periodic heartbeat
                    await SendSseEventAsync("heartbeat", new { timestamp = DateTime.UtcNow });
                    
                    try
                    {
                        await Task.Delay(30000, cancellationToken); // 30 seconds
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SSE endpoint");
            }
            finally
            {
                _logger.LogInformation("SSE connection closed");
            }
        }

        /// <summary>
        /// Execute MCP tool call via POST
        /// </summary>
        [HttpPost("call-tool")]
        public async Task<IActionResult> CallTool([FromBody] McpRequest request)
        {
            _logger.LogInformation("Tool call request: {Method}", request.Method);

            if (request.Method != "tools/call")
            {
                return BadRequest(new McpResponse
                {
                    Id = request.Id,
                    Error = new McpError
                    {
                        Code = -32601,
                        Message = "Method not found"
                    }
                });
            }

            try
            {
                if (request.Params is JsonElement paramsElement)
                {
                    var toolCall = paramsElement.Deserialize<McpToolCall>();
                    if (toolCall == null)
                    {
                        return BadRequest(new McpResponse
                        {
                            Id = request.Id,
                            Error = new McpError
                            {
                                Code = -32602,
                                Message = "Invalid params"
                            }
                        });
                    }

                    var result = await _mcpService.ExecuteToolAsync(toolCall);
                    
                    return Ok(new McpResponse
                    {
                        Id = request.Id,
                        Result = result
                    });
                }

                return BadRequest(new McpResponse
                {
                    Id = request.Id,
                    Error = new McpError
                    {
                        Code = -32602,
                        Message = "Invalid params format"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool call");
                return StatusCode(500, new McpResponse
                {
                    Id = request.Id,
                    Error = new McpError
                    {
                        Code = -32603,
                        Message = "Internal error",
                        Data = ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// Execute MCP tool call via SSE
        /// </summary>
        [HttpPost("sse/call-tool")]
        public async Task CallToolViaSSE([FromBody] McpRequest request)
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";
            Response.Headers["Access-Control-Allow-Origin"] = "*";

            try
            {
                if (request.Params is JsonElement paramsElement)
                {
                    var toolCall = paramsElement.Deserialize<McpToolCall>();
                    if (toolCall == null)
                    {
                        await SendSseEventAsync("error", new McpResponse
                        {
                            Id = request.Id,
                            Error = new McpError
                            {
                                Code = -32602,
                                Message = "Invalid params"
                            }
                        });
                        return;
                    }

                    // Send progress event
                    await SendSseEventAsync("progress", new { 
                        id = request.Id, 
                        status = "executing", 
                        tool = toolCall.Name 
                    });

                    var result = await _mcpService.ExecuteToolAsync(toolCall);
                    
                    // Send result event
                    await SendSseEventAsync("result", new McpResponse
                    {
                        Id = request.Id,
                        Result = result
                    });
                }
                else
                {
                    await SendSseEventAsync("error", new McpResponse
                    {
                        Id = request.Id,
                        Error = new McpError
                        {
                            Code = -32602,
                            Message = "Invalid params format"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SSE tool call");
                await SendSseEventAsync("error", new McpResponse
                {
                    Id = request.Id,
                    Error = new McpError
                    {
                        Code = -32603,
                        Message = "Internal error",
                        Data = ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// Send Server-Sent Event
        /// </summary>
        private async Task SendSseEventAsync(string eventType, object data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            var eventData = $"event: {eventType}\ndata: {json}\n\n";
            var bytes = Encoding.UTF8.GetBytes(eventData);
            
            await Response.Body.WriteAsync(bytes);
            await Response.Body.FlushAsync();
        }
    }
}
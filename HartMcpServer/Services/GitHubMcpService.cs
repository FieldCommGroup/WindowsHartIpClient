using HartMcpServer.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace HartMcpServer.Services
{
    /// <summary>
    /// GitHub MCP Server service that handles MCP protocol operations
    /// </summary>
    public class GitHubMcpService
    {
        private readonly ConcurrentDictionary<string, McpTool> _tools = new();
        private readonly ILogger<GitHubMcpService> _logger;

        public GitHubMcpService(ILogger<GitHubMcpService> logger)
        {
            _logger = logger;
            InitializeTools();
        }

        /// <summary>
        /// Initialize available MCP tools
        /// </summary>
        private void InitializeTools()
        {
            // GitHub repository operations
            _tools["github-get-repository"] = new McpTool
            {
                Name = "github-get-repository",
                Description = "Get information about a GitHub repository",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        owner = new { type = "string", description = "Repository owner" },
                        repo = new { type = "string", description = "Repository name" }
                    },
                    required = new[] { "owner", "repo" }
                }
            };

            _tools["github-list-issues"] = new McpTool
            {
                Name = "github-list-issues",
                Description = "List issues in a GitHub repository",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        owner = new { type = "string", description = "Repository owner" },
                        repo = new { type = "string", description = "Repository name" },
                        state = new { type = "string", description = "Issue state (open, closed, all)", @default = "open" }
                    },
                    required = new[] { "owner", "repo" }
                }
            };

            _tools["github-get-file-contents"] = new McpTool
            {
                Name = "github-get-file-contents",
                Description = "Get the contents of a file from a GitHub repository",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        owner = new { type = "string", description = "Repository owner" },
                        repo = new { type = "string", description = "Repository name" },
                        path = new { type = "string", description = "File path" },
                        @ref = new { type = "string", description = "Git reference (branch, tag, SHA)", @default = "main" }
                    },
                    required = new[] { "owner", "repo", "path" }
                }
            };

            _logger.LogInformation("Initialized {ToolCount} MCP tools", _tools.Count);
        }

        /// <summary>
        /// Get list of available tools
        /// </summary>
        public IEnumerable<McpTool> GetTools()
        {
            return _tools.Values;
        }

        /// <summary>
        /// Execute a tool call
        /// </summary>
        public async Task<McpToolResult> ExecuteToolAsync(McpToolCall toolCall)
        {
            _logger.LogInformation("Executing tool: {ToolName}", toolCall.Name);

            if (!_tools.ContainsKey(toolCall.Name))
            {
                return new McpToolResult
                {
                    Content = new List<McpContent>
                    {
                        new McpContent
                        {
                            Type = "text",
                            Text = $"Unknown tool: {toolCall.Name}"
                        }
                    },
                    IsError = true
                };
            }

            try
            {
                return toolCall.Name switch
                {
                    "github-get-repository" => await ExecuteGetRepositoryAsync(toolCall.Arguments),
                    "github-list-issues" => await ExecuteListIssuesAsync(toolCall.Arguments),
                    "github-get-file-contents" => await ExecuteGetFileContentsAsync(toolCall.Arguments),
                    _ => new McpToolResult
                    {
                        Content = new List<McpContent>
                        {
                            new McpContent
                            {
                                Type = "text",
                                Text = $"Tool {toolCall.Name} not implemented"
                            }
                        },
                        IsError = true
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool {ToolName}", toolCall.Name);
                return new McpToolResult
                {
                    Content = new List<McpContent>
                    {
                        new McpContent
                        {
                            Type = "text",
                            Text = $"Error executing tool {toolCall.Name}: {ex.Message}"
                        }
                    },
                    IsError = true
                };
            }
        }

        private async Task<McpToolResult> ExecuteGetRepositoryAsync(Dictionary<string, object> arguments)
        {
            var owner = arguments.GetValueOrDefault("owner")?.ToString() ?? "";
            var repo = arguments.GetValueOrDefault("repo")?.ToString() ?? "";

            // Simulate GitHub API call
            await Task.Delay(100);

            var result = new
            {
                name = repo,
                full_name = $"{owner}/{repo}",
                owner = new { login = owner },
                description = "Repository accessed via GitHub MCP Server",
                html_url = $"https://github.com/{owner}/{repo}",
                created_at = DateTime.UtcNow.AddYears(-1).ToString("O"),
                updated_at = DateTime.UtcNow.ToString("O"),
                language = "C#",
                stargazers_count = 42,
                forks_count = 7
            };

            return new McpToolResult
            {
                Content = new List<McpContent>
                {
                    new McpContent
                    {
                        Type = "text",
                        Text = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })
                    }
                }
            };
        }

        private async Task<McpToolResult> ExecuteListIssuesAsync(Dictionary<string, object> arguments)
        {
            var owner = arguments.GetValueOrDefault("owner")?.ToString() ?? "";
            var repo = arguments.GetValueOrDefault("repo")?.ToString() ?? "";
            var state = arguments.GetValueOrDefault("state")?.ToString() ?? "open";

            // Simulate GitHub API call
            await Task.Delay(100);

            var issues = new[]
            {
                new
                {
                    id = 1,
                    number = 1,
                    title = "Example issue from GitHub MCP Server",
                    state = state,
                    user = new { login = "example-user" },
                    created_at = DateTime.UtcNow.AddDays(-5).ToString("O"),
                    updated_at = DateTime.UtcNow.AddDays(-1).ToString("O"),
                    html_url = $"https://github.com/{owner}/{repo}/issues/1"
                },
                new
                {
                    id = 2,
                    number = 2,
                    title = "Another example issue",
                    state = state,
                    user = new { login = "another-user" },
                    created_at = DateTime.UtcNow.AddDays(-3).ToString("O"),
                    updated_at = DateTime.UtcNow.ToString("O"),
                    html_url = $"https://github.com/{owner}/{repo}/issues/2"
                }
            };

            return new McpToolResult
            {
                Content = new List<McpContent>
                {
                    new McpContent
                    {
                        Type = "text",
                        Text = JsonSerializer.Serialize(issues, new JsonSerializerOptions { WriteIndented = true })
                    }
                }
            };
        }

        private async Task<McpToolResult> ExecuteGetFileContentsAsync(Dictionary<string, object> arguments)
        {
            var owner = arguments.GetValueOrDefault("owner")?.ToString() ?? "";
            var repo = arguments.GetValueOrDefault("repo")?.ToString() ?? "";
            var path = arguments.GetValueOrDefault("path")?.ToString() ?? "";
            var gitRef = arguments.GetValueOrDefault("ref")?.ToString() ?? "main";

            // Simulate GitHub API call
            await Task.Delay(100);

            var content = path.EndsWith(".md") ? 
                "# Example File\n\nThis is an example file retrieved via GitHub MCP Server.\n\n## Features\n\n- MCP integration\n- Server-Sent Events\n- HART-IP connectivity" :
                "// Example file content\nnamespace Example {\n    public class ExampleClass {\n        // Retrieved via GitHub MCP Server\n    }\n}";

            var result = new
            {
                name = Path.GetFileName(path),
                path = path,
                sha = "abc123def456",
                size = content.Length,
                url = $"https://api.github.com/repos/{owner}/{repo}/contents/{path}",
                html_url = $"https://github.com/{owner}/{repo}/blob/{gitRef}/{path}",
                git_url = $"https://api.github.com/repos/{owner}/{repo}/git/blobs/abc123def456",
                download_url = $"https://raw.githubusercontent.com/{owner}/{repo}/{gitRef}/{path}",
                type = "file",
                content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content)),
                encoding = "base64"
            };

            return new McpToolResult
            {
                Content = new List<McpContent>
                {
                    new McpContent
                    {
                        Type = "text",
                        Text = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true })
                    }
                }
            };
        }
    }
}
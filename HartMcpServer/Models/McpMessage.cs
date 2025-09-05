using System.Text.Json.Serialization;

namespace HartMcpServer.Models
{
    /// <summary>
    /// Base class for MCP (Model Context Protocol) messages
    /// </summary>
    public abstract class McpMessage
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("method")]
        public string? Method { get; set; }
    }

    /// <summary>
    /// MCP Request message
    /// </summary>
    public class McpRequest : McpMessage
    {
        [JsonPropertyName("params")]
        public object? Params { get; set; }
    }

    /// <summary>
    /// MCP Response message
    /// </summary>
    public class McpResponse : McpMessage
    {
        [JsonPropertyName("result")]
        public object? Result { get; set; }

        [JsonPropertyName("error")]
        public McpError? Error { get; set; }
    }

    /// <summary>
    /// MCP Error details
    /// </summary>
    public class McpError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public object? Data { get; set; }
    }

    /// <summary>
    /// MCP Tool definition
    /// </summary>
    public class McpTool
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("inputSchema")]
        public object InputSchema { get; set; } = new { };
    }

    /// <summary>
    /// MCP Tool call parameters
    /// </summary>
    public class McpToolCall
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("arguments")]
        public Dictionary<string, object> Arguments { get; set; } = new();
    }

    /// <summary>
    /// MCP Tool result
    /// </summary>
    public class McpToolResult
    {
        [JsonPropertyName("content")]
        public List<McpContent> Content { get; set; } = new();

        [JsonPropertyName("isError")]
        public bool IsError { get; set; } = false;
    }

    /// <summary>
    /// MCP Content item
    /// </summary>
    public class McpContent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
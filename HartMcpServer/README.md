# HART GitHub MCP Server

This is a **GitHub Model Context Protocol (MCP) Server** with **Server-Sent Events (SSE)** support, designed to integrate with the existing Windows HART-IP Client.

## Overview

The MCP server provides GitHub API functionality through the Model Context Protocol, allowing clients to interact with GitHub repositories, issues, and files through a standardized interface. It supports both traditional HTTP API calls and real-time Server-Sent Events for streaming responses.

## Features

- ✅ **MCP Protocol Support**: Implements the Model Context Protocol 2024-11-05 specification
- ✅ **Server-Sent Events**: Real-time streaming of MCP responses via SSE
- ✅ **GitHub Integration**: Pre-built tools for GitHub repository operations
- ✅ **CORS Enabled**: Cross-origin requests supported for web clients
- ✅ **Swagger Documentation**: Interactive API documentation
- ✅ **Test Interface**: Built-in web interface for testing SSE functionality

## Available GitHub MCP Tools

1. **github-get-repository**: Get information about a GitHub repository
2. **github-list-issues**: List issues in a GitHub repository  
3. **github-get-file-contents**: Get the contents of a file from a GitHub repository

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- Any modern web browser (for testing the SSE interface)

### Running the Server

```bash
cd HartMcpServer
dotnet run
```

The server will start on `http://localhost:5129` by default.

### Available Endpoints

- **Root**: `http://localhost:5129/` - Server information and endpoint listing
- **Health**: `http://localhost:5129/health` - Health check endpoint
- **MCP Capabilities**: `http://localhost:5129/api/mcp/capabilities` - MCP server capabilities
- **MCP Tools**: `http://localhost:5129/api/mcp/tools` - List of available tools
- **SSE Stream**: `http://localhost:5129/api/mcp/sse` - Server-Sent Events endpoint
- **Tool Execution**: `http://localhost:5129/api/mcp/call-tool` - Execute MCP tools via POST
- **SSE Tool Execution**: `http://localhost:5129/api/mcp/sse/call-tool` - Execute tools via SSE
- **Test Interface**: `http://localhost:5129/test.html` - Interactive web test page
- **Documentation**: `http://localhost:5129/swagger` - Swagger/OpenAPI documentation

## Usage Examples

### 1. Server-Sent Events Connection

Open a connection to the SSE endpoint to receive real-time updates:

```javascript
const eventSource = new EventSource('http://localhost:5129/api/mcp/sse');

eventSource.addEventListener('server-info', function(e) {
    const data = JSON.parse(e.data);
    console.log('Server info:', data);
});

eventSource.addEventListener('tools', function(e) {
    const data = JSON.parse(e.data);
    console.log('Available tools:', data.tools);
});
```

### 2. Calling MCP Tools via HTTP

```bash
curl -X POST http://localhost:5129/api/mcp/call-tool \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": "1",
    "method": "tools/call",
    "params": {
      "name": "github-get-repository",
      "arguments": {
        "owner": "microsoft",
        "repo": "vscode"
      }
    }
  }'
```

### 3. Using the Test Interface

Navigate to `http://localhost:5129/test.html` to access the interactive web interface that demonstrates:
- SSE connection management
- Real-time event streaming
- Tool execution with live results
- GitHub API simulation

## Integration with HART-IP Client

This MCP server is designed to complement the existing HART-IP Client by providing:

1. **GitHub Integration**: Access to GitHub repositories for documentation, issue tracking, and code management
2. **Real-time Updates**: SSE-based streaming for live updates on GitHub activities
3. **Standardized Protocol**: MCP compliance for interoperability with MCP-compatible clients
4. **Web Interface**: Browser-based access to HART device information via GitHub

## Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Web Client    │    │  HART-IP Client  │    │  GitHub API     │
│   (Browser)     │    │  (Windows Forms) │    │  (External)     │
└─────────┬───────┘    └─────────┬────────┘    └─────────────────┘
          │                      │                       │
          │ SSE/HTTP             │                       │
          │                      │                       │
    ┌─────▼──────────────────────▼───────┐               │
    │     HART GitHub MCP Server         │               │
    │                                    │               │
    │  ┌──────────────┐ ┌─────────────┐  │               │
    │  │ MCP Service  │ │ SSE Handler │  │               │
    │  └──────────────┘ └─────────────┘  │               │
    │                                    │               │
    │  ┌──────────────┐ ┌─────────────┐  │               │
    │  │ Controllers  │ │ Models      │  │               │
    │  └──────────────┘ └─────────────┘  │               │
    └────────────────────────────────────┘               │
                         │                               │
                         └─────────────────────────────◄─┘
                           GitHub API Calls (Future)
```

## Configuration

The server uses standard ASP.NET Core configuration. Key settings:

- **Port**: Default 5129 (configurable via `--urls` or `appsettings.json`)
- **CORS**: Enabled for all origins (development mode)
- **Logging**: Console and debug output
- **Environment**: Development mode includes Swagger UI

## Future Enhancements

- [ ] Real GitHub API integration (currently simulated)
- [ ] Authentication and authorization
- [ ] HART device status integration
- [ ] Persistent connections management
- [ ] Rate limiting and throttling
- [ ] Custom GitHub webhook support
- [ ] Integration with existing HART-IP Client UI

## Development

### Project Structure

```
HartMcpServer/
├── Controllers/
│   └── McpController.cs      # MCP API endpoints
├── Models/
│   └── McpMessage.cs         # MCP protocol models
├── Services/
│   └── GitHubMcpService.cs   # GitHub MCP business logic
├── wwwroot/
│   └── test.html             # Test interface
├── Program.cs                # Application startup
└── HartMcpServer.csproj      # Project file
```

### Building

```bash
dotnet build
```

### Testing

1. Start the server: `dotnet run`
2. Open browser to `http://localhost:5129/test.html`
3. Click "Connect to SSE" to establish the Server-Sent Events connection
4. Use the tool forms to test GitHub MCP functionality
5. Monitor the events log for real-time responses

## License

This project follows the same Apache 2.0 license as the parent HART-IP Client project.
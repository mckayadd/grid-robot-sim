using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace GridRobotMcpBridge;

// Same JSON contract used by GridRobotClient and RobotApp's pipe handler.
public record CommandRequest(string Command);
public record CommandResponse(bool Success, int X, int Y, string? Error);

public static class RobotPipeClient
{
    private const string PipeName = "GridRobotMcpPipe";

    public static async Task<CommandResponse> SendCommandAsync(string command)
    {
        using var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

        try
        {
            await pipeClient.ConnectAsync(3000);
        }
        catch (TimeoutException)
        {
            return new CommandResponse(false, 0, 0,
                "Could not connect to RobotApp. Is it running with MCP enabled ('enable mcp')?");
        }

        using var writer = new StreamWriter(pipeClient, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
        using var reader = new StreamReader(pipeClient, Encoding.UTF8, leaveOpen: true);

        var request = new CommandRequest(command);
        await writer.WriteLineAsync(JsonSerializer.Serialize(request));

        var responseJson = await reader.ReadLineAsync();
        if (responseJson is null)
            return new CommandResponse(false, 0, 0, "No response from RobotApp.");

        var response = JsonSerializer.Deserialize<CommandResponse>(responseJson);
        return response ?? new CommandResponse(false, 0, 0, "Failed to parse response from RobotApp.");
    }
}
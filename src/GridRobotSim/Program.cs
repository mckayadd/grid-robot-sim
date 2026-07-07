using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using GridRobotSim;

var world = new World(16, 16);
var random = new Random();
var robot = new Robot(world, random.Next(world.Width), random.Next(world.Height));
var robotLock = new object();
var mcpEnabled = false;

Console.WriteLine($"Robot started at: ({robot.X}, {robot.Y})");
Console.WriteLine("Server running. Type 'exit' to stop, or 'enable mcp' to start the MCP pipe.");

// Human-facing IPC pipe, runs on its own thread
var ipcThread = new Thread(() => RunPipeLoop("GridRobotPipe"));
ipcThread.IsBackground = true;
ipcThread.Start();

// Console command listener (exit / enable mcp), runs on the main thread
while (true)
{
    var input = Console.ReadLine();
    if (input is null)
        continue;

    var trimmed = input.Trim();

    if (trimmed.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Shutting down server...");
        Environment.Exit(0);
    }
    else if (trimmed.Equals("enable mcp", StringComparison.OrdinalIgnoreCase))
    {
        if (mcpEnabled)
        {
            Console.WriteLine("MCP pipe is already running.");
            continue;
        }

        mcpEnabled = true;
        var mcpThread = new Thread(() => RunPipeLoop("GridRobotMcpPipe"));
        mcpThread.IsBackground = true;
        mcpThread.Start();

        Console.WriteLine("MCP pipe enabled. Listening on 'GridRobotMcpPipe'.");
    }
}

void RunPipeLoop(string pipeName)
{
    while (true)
    {
        using var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut);
        pipeServer.WaitForConnection();

        using var reader = new StreamReader(pipeServer, Encoding.UTF8, leaveOpen: true);
        using var writer = new StreamWriter(pipeServer, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

        var json = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(json))
            continue;

        var request = JsonSerializer.Deserialize<CommandRequest>(json);
        if (request is null)
            continue;

        var response = HandleRequest(request);
        writer.WriteLine(JsonSerializer.Serialize(response));
    }
}

CommandResponse HandleRequest(CommandRequest request)
{
    lock (robotLock)
    {
        var command = request.Command.Trim();

        if (command.Equals("STATE", StringComparison.OrdinalIgnoreCase))
        {
            return new CommandResponse(true, robot.X, robot.Y, null);
        }

        try
        {
            var (direction, steps) = CommandParser.Parse(command);
            robot.Move(direction, steps);

            Console.WriteLine($"{command} -> ({robot.X}, {robot.Y})");
            return new CommandResponse(true, robot.X, robot.Y, null);
        }
        catch (Exception ex)
        {
            return new CommandResponse(false, robot.X, robot.Y, ex.Message);
        }
    }
}

record CommandRequest(string Command);
record CommandResponse(bool Success, int X, int Y, string? Error);
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using GridRobotSim;

var world = new World(16, 16);
var random = new Random();
var robot = new Robot(world, random.Next(world.Width), random.Next(world.Height));

Console.WriteLine($"Robot started at: ({robot.X}, {robot.Y})");
Console.WriteLine("Server running. Type 'exit' and press Enter to stop.");

// Listen for exit on the server's own console, on a separate thread,
// so it doesn't interfere with commands coming through the pipe
var consoleListener = new Thread(() =>
{
    while (true)
    {
        var input = Console.ReadLine();
        if (input is not null && input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Shutting down server...");
            Environment.Exit(0);
        }
    }
});
consoleListener.IsBackground = true;
consoleListener.Start();

while (true)
{
    using var pipeServer = new NamedPipeServerStream("GridRobotPipe", PipeDirection.InOut);
    pipeServer.WaitForConnection();

    using var reader = new StreamReader(pipeServer, Encoding.UTF8, leaveOpen: true);
    using var writer = new StreamWriter(pipeServer, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

    var json = reader.ReadLine();
    if (string.IsNullOrWhiteSpace(json))
        continue;

    var request = JsonSerializer.Deserialize<CommandRequest>(json);
    if (request is null)
        continue;

    CommandResponse response;

    try
    {
        var (direction, steps) = CommandParser.Parse(request.Command);
        robot.Move(direction, steps);

        Console.WriteLine($"{request.Command} -> ({robot.X}, {robot.Y})");
        response = new CommandResponse(true, robot.X, robot.Y, null);
    }
    catch (Exception ex)
    {
        response = new CommandResponse(false, robot.X, robot.Y, ex.Message);
    }

    writer.WriteLine(JsonSerializer.Serialize(response));
}

record CommandRequest(string Command);
record CommandResponse(bool Success, int X, int Y, string? Error);
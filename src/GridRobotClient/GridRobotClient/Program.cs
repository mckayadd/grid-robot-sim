using System.IO.Pipes;
using System.Text;
using System.Text.Json;

Console.WriteLine("Enter a command (e.g. R 4), type 'exit' to quit:");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    // exit only closes the client, it is not sent to the server
    if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    try
    {
        using var pipeClient = new NamedPipeClientStream(".", "GridRobotPipe", PipeDirection.InOut);
        pipeClient.Connect(3000);

        using var writer = new StreamWriter(pipeClient, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
        using var reader = new StreamReader(pipeClient, Encoding.UTF8, leaveOpen: true);

        var request = new CommandRequest(input.Trim());
        writer.WriteLine(JsonSerializer.Serialize(request));

        var responseJson = reader.ReadLine();
        var response = JsonSerializer.Deserialize<CommandResponse>(responseJson!);

        if (response!.Success)
            Console.WriteLine($"OK -> ({response.X}, {response.Y})");
        else
            Console.WriteLine($"ERROR: {response.Error}");
    }
    catch (TimeoutException)
    {
        Console.WriteLine("Could not connect to server. Is RobotApp running?");
    }
}

record CommandRequest(string Command);
record CommandResponse(bool Success, int X, int Y, string? Error);
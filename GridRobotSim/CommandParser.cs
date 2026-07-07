namespace GridRobotSim;

public static class CommandParser
{
    public static (Direction Direction, int Steps) Parse(string command)
    {
        var parts = command.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            throw new FormatException($"Invalid Command: '{command}'");

        var direction = parts[0].ToUpperInvariant() switch
        {
            "U" => Direction.Up,
            "D" => Direction.Down,
            "L" => Direction.Left,
            "R" => Direction.Right,
            _ => throw new FormatException($"Unknown direction: '{parts[0]}'")
        };

        if (!int.TryParse(parts[1], out var steps) || steps < 0)
            throw new FormatException($"Invalid step size: '{parts[1]}'");

        return (direction, steps);
    }
}
namespace GridRobotSim;

public class World
{
    public int Width { get; }
    public int Height { get; }

    public World(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Values must be positive.");

        Width = width;
        Height = height;
    }

    public (int X, int Y) Clamp(int x, int y)
    {
        var clampedX = Math.Clamp(x, 0, Width - 1);
        var clampedY = Math.Clamp(y, 0, Height - 1);
        return (clampedX, clampedY);
    }
}
namespace GridRobotSim;

public class Robot
{
    private readonly World _world;
    public int X { get; private set; }
    public int Y { get; private set; }

    public Robot(World world, int startX, int startY)
    {
        _world = world;
        (X, Y) = _world.Clamp(startX, startY);
    }

    public void Move(Direction direction, int steps)
    {
        var targetX = X;
        var targetY = Y;

        switch (direction)
        {
            case Direction.Up:
                targetY += steps; // (0,0) is bottom-left, so Up increases Y
                break;
            case Direction.Down:
                targetY -= steps;
                break;
            case Direction.Left:
                targetX -= steps;
                break;
            case Direction.Right:
                targetX += steps;
                break;
        }

        (X, Y) = _world.Clamp(targetX, targetY);
    }
}
using System.ComponentModel;
using ModelContextProtocol.Server;

namespace GridRobotMcpBridge;

[McpServerToolType]
public static class RobotTools
{
    [McpServerTool, Description("Moves the robot on the grid in a direction by a number of steps. Direction must be one of: U (up), D (down), L (left), R (right). If the robot would go past the grid boundary, it stops at the edge.")]
    public static async Task<string> MoveRobot(
        [Description("Direction to move: U, D, L, or R")] string direction,
        [Description("Number of grid units to move")] int steps)
    {
        var command = $"{direction} {steps}";
        var response = await RobotPipeClient.SendCommandAsync(command);

        return response.Success
            ? $"Robot moved. New position: ({response.X}, {response.Y})"
            : $"Move failed: {response.Error}";
    }

    [McpServerTool, Description("Gets the robot's current position on the grid, without moving it.")]
    public static async Task<string> GetRobotState()
    {
        var response = await RobotPipeClient.SendCommandAsync("STATE");

        return response.Success
            ? $"Robot is at ({response.X}, {response.Y})"
            : $"Failed to get state: {response.Error}";
    }
}
# GridRobotSim

A 2D grid robot simulation. A robot starts at a random position on a fixed-size grid and moves based on discrete commands (Up, Down, Left, Right + steps). Communication with the running simulation happens over Named Pipes (IPC), and an MCP bridge exposes the same functionality to AI agents like Claude Code.

## Project structure

- `GridRobotSim` — the core simulation. Contains `World`, `Robot`, `CommandParser`, and the process that hosts everything over a named pipe.
- `GridRobotClient` — a simple console client for sending commands manually via the named pipe.
- `GridRobotMcpBridge` — an MCP server (stdio transport) that translates MCP tool calls into named pipe commands, so AI agents can control the robot using natural language.

## Coordinate system

- Grid is fixed at 16x16 by default (configurable via the `World` constructor).
- `(0,0)` is the bottom-left corner.
- `Up` increases Y, `Down` decreases Y, `Right` increases X, `Left` decreases X.
- Moves that would go past the grid boundary stop at the edge instead of failing.

## Running the simulation
```bash
cd GridRobotSim
dotnet run
```
Type commands like `R 4`, `U 20`, `D 3`. Type `exit` to quit the client (this does not stop the server).

## Connecting an AI agent via MCP

The MCP bridge lets any MCP-compatible AI agent (e.g. Claude Code) control the robot using natural language.

### 1. Start the simulation and enable MCP

```bash
cd GridRobotSim
dotnet run
```
Then type `enable mcp` in that terminal.

### 2. Publish the bridge for your machine

From the repo root:
```bash
./publish.sh
```
This detects your OS/architecture and produces a self-contained executable at `./publish/grid-robot-mcp-bridge/GridRobotMcpBridge`.

### 3. Register the bridge with Claude Code
Run this from the repo root:
```bash
claude mcp add grid-robot -- $(pwd)/publish/grid-robot-mcp-bridge/GridRobotMcpBridge
```

Use `--scope user` instead of the default local scope if you want it available from any directory:

```bash
claude mcp add grid-robot --scope user -- $(pwd)/publish/grid-robot-mcp-bridge/GridRobotMcpBridge
```

Check it was registered:
```bash
claude mcp list
```

### 4. Use it
```bash
claude
```
Inside the session, run `/mcp` to confirm `grid-robot` is connected and its tools are listed. Then just talk to it naturally, e.g.:

> Where is the robot right now?
> Move it 3 units right and 2 units up.

## Updating the bridge after code changes

Whenever `GridRobotMcpBridge` is changed (e.g. a new tool is added), re-run:

```bash
./publish.sh
```

Then restart any active Claude Code session (or run `/mcp` to reconnect) so it picks up the new build.

## Requirements

- .NET SDK (8.0 or later)
- Claude Code CLI, for the MCP integration
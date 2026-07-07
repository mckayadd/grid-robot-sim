#!/usr/bin/env bash
set -e

PROJECT_PATH="src/GridRobotMcpBridge"
OUTPUT_PATH="publish/grid-robot-mcp-bridge"

OS_NAME="$(uname -s)"
ARCH_NAME="$(uname -m)"

if [ "$OS_NAME" = "Darwin" ]; then
    if [ "$ARCH_NAME" = "arm64" ]; then
        RID="osx-arm64"
    else
        RID="osx-x64"
    fi
elif [ "$OS_NAME" = "Linux" ]; then
    RID="linux-x64"
else
    echo "Unsupported OS: $OS_NAME"
    echo "For Windows, run manually:"
    echo "  dotnet publish $PROJECT_PATH -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o $OUTPUT_PATH"
    exit 1
fi

echo "Detected OS: $OS_NAME, Arch: $ARCH_NAME -> RID: $RID"
echo "Publishing GridRobotMcpBridge..."

dotnet publish "$PROJECT_PATH" \
    -c Release \
    -r "$RID" \
    --self-contained \
    -p:PublishSingleFile=true \
    -o "$OUTPUT_PATH"

chmod +x "$OUTPUT_PATH/GridRobotMcpBridge"

echo ""
echo "Done. Executable at: $OUTPUT_PATH/GridRobotMcpBridge"
echo ""
echo "If this is the first time, register it with Claude Code:"
echo "  claude mcp add grid-robot -- \$(pwd)/$OUTPUT_PATH/GridRobotMcpBridge"
echo ""
echo "If you already registered it before, just restart your Claude Code session"
echo "(or run /mcp inside it) to pick up the new build."
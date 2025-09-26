#!/bin/bash

# Mini Game Hub Project Launcher
echo "🎮 Mini Game Hub Project Launcher"
echo "=================================="

PROJECT_PATH="/Users/zzz/hub-game-mini"
UNITY_PATH="/Applications/Unity/Hub/Editor/6000.2.5f1/Unity.app/Contents/MacOS/Unity"

echo "Project Path: $PROJECT_PATH"
echo "Unity Path: $UNITY_PATH"

# Check if Unity exists
if [ ! -f "$UNITY_PATH" ]; then
    echo "❌ Unity not found at: $UNITY_PATH"
    echo "Please install Unity 6000.2.5f1 or update the path in this script"
    exit 1
fi

# Check if project exists
if [ ! -d "$PROJECT_PATH" ]; then
    echo "❌ Project not found at: $PROJECT_PATH"
    exit 1
fi

echo "✅ Unity found: $UNITY_PATH"
echo "✅ Project found: $PROJECT_PATH"
echo ""
echo "🚀 Launching Unity with Mini Game Hub project..."
echo "   This may take a few moments to open..."

# Launch Unity with the project
"$UNITY_PATH" -projectPath "$PROJECT_PATH" &

echo "✅ Unity launched! The project should open shortly."
echo ""
echo "📝 Next Steps:"
echo "   1. Wait for Unity to finish importing assets"
echo "   2. Check the Console for any errors"
echo "   3. Open the SampleScene if not already open"
echo "   4. Press Play to test the boot sequence"
echo ""
echo "🎯 To build for mobile:"
echo "   1. File → Build Settings"
echo "   2. Switch Platform to Android or iOS" 
echo "   3. Configure Player Settings"
echo "   4. Build and Run"
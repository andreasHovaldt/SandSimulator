using System.Numerics;
using Raylib_cs;

namespace SandSimulator;

internal static class Program
{

    // STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
    [System.STAThread]
    public static void Main()
    {
        // Window settings
        int windowWidth = 400;
        int windowHeight = 250;
        float windowScale = 4.0f;

        // Initilize raylib window
        Raylib.InitWindow(width: (int)(windowWidth * windowScale), height: (int)(windowHeight * windowScale), title: "Sand Simulator");
        Raylib.SetTargetFPS(fps: 60);

        // Initilize simulator instance
        Simulator simulator = new Simulator(width: windowWidth, height: windowHeight, scale: windowScale);
        SandType[] sandTypes = simulator.GetSandTypes;

        // Benchmark state
        string benchmarkResult = "";

        // Run main simulation loop
        while (!Raylib.WindowShouldClose())
        {
            // Press B to run benchmark
            if (Raylib.IsKeyPressed(KeyboardKey.B))
            {
                var (ms, frames) = simulator.RunBenchmark(sandCount: 50000, frames: 500);
                benchmarkResult = $"{frames} frames: {ms:F0}ms ({ms / frames:F2}ms/frame)";
                Console.WriteLine(benchmarkResult);
            }

            // Press C to clear the screen
            if (Raylib.IsKeyPressed(KeyboardKey.C))
            {
                Array.Fill(simulator.GetColorArray, Color.White);
                benchmarkResult = "";
            }

            // Simulation logic
            simulator.MousePaint(color: sandTypes[0].GetColor);
            simulator.MousePaint(color: sandTypes[1].GetColor, triggerButton: MouseButton.Right);
            simulator.MousePaint(color: sandTypes[2].GetColor, triggerButton: MouseButton.Middle);
            simulator.SimulateScene();


            // Update the texture with the newest colorArray data
            simulator.UpdateTexture();

            // Simulation vizualization
            Raylib.BeginDrawing();
            Raylib.DrawTextureEx(texture: simulator.GetTexture, position: new Vector2(0, 0), rotation: 0.0f, scale: windowScale, tint: Color.White);

            // Display runtime info
            Raylib.DrawRectangle(10, 10, 154, 47, Color.LightGray);
            Raylib.DrawText("Sand Simulator", posX: 12, posY: 12, fontSize: 20, color: Color.Black);
            Raylib.DrawText($"FPS: {Raylib.GetFPS():F0}", posX: 12, posY: 36, fontSize: 20, color: Color.Black);
            if (!string.IsNullOrEmpty(benchmarkResult))
            {
                Raylib.DrawRectangle(200, 10, (int)(200 * windowScale), (int)(12 * windowScale), Color.LightGray);
                Raylib.DrawText(benchmarkResult, posX: 203, posY: 20, fontSize: (int)(10 * windowScale), color: Color.DarkGray);
            }

            Raylib.EndDrawing();
        }

        simulator.UnloadTexture();
        Raylib.CloseWindow();
    }
}



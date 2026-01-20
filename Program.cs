using System.Numerics;
using Raylib_cs;

namespace SandSimulator;

internal static class Program
{
    private static Color[] GenWhiteNoise(Color[] array, int width, int height)
    {
        Random rng = new Random();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color rngColor = rng.Next(0, 2) switch
                {
                    0 => Color.White,
                    1 => Color.Black,
                    _ => Color.Red,
                };
                array[y * width + x] = rngColor;
            }
        }
        return array;
    }


    // STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
    [System.STAThread]
    public static void Main()
    {
        // Window settings
        int windowWidth = 400;
        int windowHeight = 250;
        float windowScale = 4.0f;

        // Initilize a flat color array for texture data 
        Color[] colorArray = new Color[windowWidth * windowHeight];
        Array.Fill(colorArray, Color.Red);

        // Initilize raylib window
        Raylib.InitWindow(width: (int)(windowWidth * windowScale), height: (int)(windowHeight * windowScale), title: "Sand Simulator");
        Raylib.SetTargetFPS(fps: 200);

        // Initilize blank image and load as Texture2D
        Image blankImage = Raylib.GenImageColor(width: windowWidth, height: windowHeight, color: Color.White);
        Texture2D windowTexture = Raylib.LoadTextureFromImage(image: blankImage);
        Raylib.UnloadImage(image: blankImage);


        // Run main simulation loop
        while (!Raylib.WindowShouldClose())
        {
            // Simulation logic
            colorArray = GenWhiteNoise(colorArray, windowWidth, windowHeight);

            // Update the windowTexture with the newest colorArray data
            Raylib.UpdateTexture(texture: windowTexture, pixels: colorArray);

            // Simulation vizualization
            Raylib.BeginDrawing();
            Raylib.DrawTextureEx(texture: windowTexture, position: new Vector2(0, 0), rotation: 0.0f, scale: windowScale, tint: Color.White);

            // Display runtime info
            Raylib.DrawRectangle(10, 10, 154, 47, Color.LightGray);
            Raylib.DrawText("Sand Simulator", posX: 12, posY: 12, fontSize: 20, color: Color.Black);
            Raylib.DrawText($"FPS: {Raylib.GetFPS():F0}", posX: 12, posY: 36, fontSize: 20, color: Color.Black);

            Raylib.EndDrawing();
        }

        Raylib.UnloadTexture(texture: windowTexture);
        Raylib.CloseWindow();
    }
}
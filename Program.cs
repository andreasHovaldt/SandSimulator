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
        int windowWidth = 800;
        int windowHeight = 480;

        // Initilize a flat color array for texture data 
        Color[] colorArray = new Color[windowWidth * windowHeight];
        Array.Fill(colorArray, Color.Red);

        // for (int x = 0; x < windowWidth; x += 2)  // Every other column (0, 2, 4, ...)
        // {
        //     for (int y = 0; y < windowHeight; y++)
        //     {
        //         int idx = y * windowWidth + x;  // Row-major: y * width + x
        //         colorArray[idx] = Color.White;
        //     }
        // }


        // Color[][] colorGrid = new Color[windowWidth][];
        // for (int i = 0; i < colorGrid.Length; i++)
        // {
        //     colorGrid[i] = new Color[windowHeight];
        //     Array.Fill(colorGrid[i], Color.Red);
        // }

        // Color[,] colorArray2 = new Color[windowWidth, windowHeight];
        // for (int i = 0; i < windowWidth; i++)
        // {
        //     for (int j = 0; j < windowHeight; j++)
        //     {
        //         colorArray2[i,j] = Color.White;
        //     }
        // }

        // Initilize raylib window
        Raylib.InitWindow(width: windowWidth, height: windowHeight, title: "Sand Simulator");
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
            Raylib.DrawTexture(texture: windowTexture, posX: 0, posY: 0, tint: Color.White);

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
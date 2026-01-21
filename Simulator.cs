using System.Numerics;
using System.Diagnostics;
using Raylib_cs;

namespace SandSimulator;

class Simulator
{
    private readonly int width;
    private readonly int height;
    private readonly float scale;
    private readonly Color backgroundColor;
    private readonly SandType[] sandTypes;
    private readonly Dictionary<Color, SandType> sandTypeLookup;
    private readonly Random rng = new Random();
    private Color[] colorArray;
    private Texture2D texture;

    public Simulator(int width, int height, float scale, Color? backgroundColor = null)
    {
        this.width = width;
        this.height = height;
        this.sandTypes = [new YellowSand(), new BlueSand(), new GraySand()];
        this.sandTypeLookup = this.sandTypes.ToDictionary(s => s.GetColor);
        this.scale = scale;
        this.backgroundColor = backgroundColor ?? Color.White; // Default value is white if none is passed

        // Initilize a flat color array for texture data
        this.colorArray = new Color[this.width * this.height];
        Array.Fill(this.colorArray, Color.White);

        // Initilize texture using blank image loaded as Texture2D
        Image blankImage = Raylib.GenImageColor(width: this.width, height: this.height, color: Color.White);
        this.texture = Raylib.LoadTextureFromImage(image: blankImage);
        Raylib.UnloadImage(image: blankImage);
    }

    public void GenWhiteNoise()
    {
        for (int x = 0; x < this.width; x++)
        {
            for (int y = 0; y < this.height; y++)
            {
                Color rngColor = this.rng.Next(0, 2) switch
                {
                    0 => Color.White,
                    1 => Color.Black,
                    _ => Color.Blank,
                };
                this.colorArray[y * width + x] = rngColor;
            }
        }
    }

    private bool evenFrame = true;

    public void SimulateScene()
    {
        // Reverse direction every iteration
        this.evenFrame = !this.evenFrame;

        // Check which direction to check first (right/left)
        int xStart = evenFrame ? 0 : this.width - 1;
        int xEnd = evenFrame ? this.width - 1 : 0;
        int xStep = evenFrame ? 1 : -1;

        // Check the whole color array for pixels with the given color
        for (int x = xStart; x != xEnd; x += xStep)
        {
            // The for loops check in the reverse order, from bottom to top, 
            //   which ensures moved pixels arent sequentially moved multiple 
            //   times before a render, allowing a proper "falling" animation
            for (int y = this.height - 1; y >= 0; y--)
            {
                // Get color once and look up sand type directly
                Color currentColor = CheckPosColor(x, y);

                // Skip background pixels immediately
                if (currentColor.Equals(this.backgroundColor))
                    continue;

                // Look up sand type from dictionary, then skip if not a known sand type
                SandType? sandType = TryGetSandType(currentColor);
                if (sandType == null)
                    continue;

                // Check the allowed movements for the specific sand type
                foreach ((int, int) newRelativePosition in sandType.GetMovementArray)
                {
                    (int, int) newTruePosition = (x + newRelativePosition.Item1, y + newRelativePosition.Item2);

                    // Check if new pos is within bounds, if not, skip to next
                    if (!CheckPosBounds(newTruePosition.Item1, newTruePosition.Item2))
                        continue;

                    // Get color of target pixel
                    Color targetColor = CheckPosColor(newTruePosition.Item1, newTruePosition.Item2);

                    // If can move to empty space
                    if (targetColor.Equals(this.backgroundColor))
                    {
                        SetPosColor(x, y, this.backgroundColor);
                        SetPosColor(newTruePosition.Item1, newTruePosition.Item2, sandType.GetColor);
                        break;
                    }

                    // If it can displace lighter sand type
                    SandType? targetSandType = TryGetSandType(targetColor);
                    if (targetSandType != null && targetSandType.GetWeight < sandType.GetWeight)
                    {
                        SetPosColor(x, y, targetColor);
                        SetPosColor(newTruePosition.Item1, newTruePosition.Item2, sandType.GetColor);
                        break;
                    }
                }
            }
        }
    }


    public void MousePaint(Color color, int brushSize = 3, bool allowOverwrite = false, MouseButton triggerButton = MouseButton.Left)
    {
        (int trueMouseX, int trueMouseY) = TrueMousePositionInt();

        // Safety check for making sure the mouse is within the window before the 'CheckPosColor' is called.
        if (!CheckPosBounds(trueMouseX, trueMouseY))
        {
            return;
        }

        if (Raylib.IsMouseButtonDown(triggerButton) && (allowOverwrite || CheckPosColor(trueMouseX, trueMouseY).Equals(this.backgroundColor)))
        {
            for (int xOffset = -brushSize; xOffset <= brushSize; xOffset++)
            {
                for (int yOffset = -brushSize; yOffset <= brushSize; yOffset++)
                {
                    int xPos = trueMouseX + xOffset;
                    int yPos = trueMouseY + yOffset;

                    if (CheckPosBounds(xPos, yPos)) // Again safety check, the brush size might go beyond the window bounds
                    {
                        this.colorArray[yPos * this.width + xPos] = color;
                    }
                }
            }
        }
    }

    private Vector2 TrueMousePosition()
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        return mousePos / this.scale;
    }

    private (int x, int y) TrueMousePositionInt()
    {
        Vector2 mousePos = TrueMousePosition();
        return ((int)Math.Round(mousePos.X), (int)Math.Round(mousePos.Y));
    }

    private bool CheckPosBounds(int x, int y)
    {
        return (x >= 0) && (x < this.width) && (y >= 0) && (y < this.height);
    }

    private Color CheckPosColor(int x, int y)
    {
        return this.colorArray[(y * this.width) + x];
    }

    private void SetPosColor(int x, int y, Color color)
    {
        this.colorArray[(y * this.width) + x] = color;
    }

    private SandType? TryGetSandType(Color color)
    // Uses a dictionary for faster matching of Color to sandType, if the color doesnt exist, null is returned
    {
        return this.sandTypeLookup.TryGetValue(color, out var sandType) ? sandType : null;
    }

    public void UpdateTexture() => Raylib.UpdateTexture(texture: this.texture, pixels: this.colorArray);
    public void UnloadTexture() => Raylib.UnloadTexture(texture: this.texture);

    private void SpawnBenchmarkSand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int x = rng.Next(0, width);
            int y = rng.Next(0, height / 2); // Top half
            SetPosColor(x, y, sandTypes[rng.Next(sandTypes.Length)].GetColor);
        }
    }

    public (double totalMs, int frames) RunBenchmark(int sandCount, int frames)
    {
        // Spawn sand
        SpawnBenchmarkSand(sandCount);

        // Start timer and simulate for given frames
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < frames; i++)
        {
            SimulateScene();
            UpdateTexture();
            Raylib.BeginDrawing();
            Raylib.DrawTextureEx(texture: this.GetTexture, position: new Vector2(0, 0), rotation: 0.0f, scale: this.scale, tint: Color.White);
            Raylib.DrawText("! RUNNING BENCHMARK !", posX: (int)(this.width * scale / 5.7), posY: (int)(this.height * scale / 1.9), fontSize: (int)(20 * this.scale), color: Color.Red);
            Raylib.EndDrawing();
        }
        sw.Stop();
        return (sw.Elapsed.TotalMilliseconds, frames);
    }


    public Color[] GetColorArray => this.colorArray;
    public Texture2D GetTexture => this.texture;
    public SandType[] GetSandTypes => this.sandTypes;

}
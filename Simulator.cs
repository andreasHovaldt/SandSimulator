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
    private readonly int backgroundID;
    private readonly SandType[] sandTypes;
    private readonly Dictionary<int, SandType> sandTypeLookupFromID;
    private readonly Dictionary<int, Color> sandColorLookupFromID;
    private readonly Random rng = new Random();
    private Color[] colorArray;
    private int[] idArray;
    private Texture2D texture;

    public Simulator(int width, int height, float scale, Color? backgroundColor = null)
    {
        // Initilizing class fields
        this.width = width;
        this.height = height;
        this.scale = scale;

        this.sandTypes = [new YellowSand(id: 1), new BlueSand(id: 2), new GraySand(id: 3)];
        this.sandTypeLookupFromID = this.sandTypes.ToDictionary(s => s.GetID);                   // (s => s.GetColor) == (s => s.GetColor, s => s)
        this.sandColorLookupFromID = this.sandTypes.ToDictionary(s => s.GetID, s => s.GetColor); // (keySelection, valueSelector)

        this.backgroundColor = backgroundColor ?? Color.White; // Default value is white if none is passed
        this.backgroundID = 0;

        // Initilize a flat color array for texture data
        this.colorArray = new Color[this.width * this.height];
        Array.Fill(this.colorArray, Color.White);

        // Also a cell array which is used for the actual computation, since int comparison is more efficient than Color.Equal()
        this.idArray = new int[this.width * this.height];
        Array.Fill(this.idArray, 0);

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
                int currentIdx = (y * this.width) + x; // TODO: Small optimization: Utilize this instead of it being calculated multiple times in some of the helper funcs
                int currentID = this.idArray[currentIdx];

                // Skip background pixels immediately
                if (currentID == this.backgroundID)
                    continue;

                // Look up sand type from dictionary, then skip if not a known sand type
                SandType? sandType = TryGetSandTypeFromID(currentID);
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
                    int targetID = CheckPosID(newTruePosition.Item1, newTruePosition.Item2);

                    // If can move to empty space
                    if (targetID == this.backgroundID)
                    {
                        SetPosID(x, y, this.backgroundID);
                        SetPosID(newTruePosition.Item1, newTruePosition.Item2, currentID);
                        break;
                    }

                    // If it can displace lighter sand type
                    SandType? targetSandType = TryGetSandTypeFromID(targetID);
                    if (targetSandType != null && targetSandType.GetWeight < sandType.GetWeight)
                    {
                        SetPosID(x, y, targetID);
                        SetPosID(newTruePosition.Item1, newTruePosition.Item2, currentID);
                        break;
                    }
                }
            }
        }
    }


    public void MousePaint(int sandID, int brushSize = 3, bool allowOverwrite = false, MouseButton triggerButton = MouseButton.Left)
    {
        (int trueMouseX, int trueMouseY) = TrueMousePositionInt();

        // Safety check for making sure the mouse is within the window before the 'CheckPosID' is called.
        if (!CheckPosBounds(trueMouseX, trueMouseY))
        {
            return;
        }

        if (Raylib.IsMouseButtonDown(triggerButton) && (allowOverwrite || CheckPosID(trueMouseX, trueMouseY) == this.backgroundID))
        {
            for (int xOffset = -brushSize; xOffset <= brushSize; xOffset++)
            {
                for (int yOffset = -brushSize; yOffset <= brushSize; yOffset++)
                {
                    int xPos = trueMouseX + xOffset;
                    int yPos = trueMouseY + yOffset;

                    if (CheckPosBounds(xPos, yPos)) // Again safety check, the brush size might go beyond the window bounds
                    {
                        this.idArray[yPos * this.width + xPos] = sandID;
                    }
                }
            }
        }
    }

    private Vector2 TrueMousePosition() => Raylib.GetMousePosition() / this.scale;
    // Return the true mouse position within the idArray/colorArray as a Vector2

    private (int x, int y) TrueMousePositionInt()
    // Return the true mouse position within the idArray/colorArray as an integer tuple
    {
        Vector2 mousePos = TrueMousePosition();
        return ((int)Math.Round(mousePos.X), (int)Math.Round(mousePos.Y));
    }

    private bool CheckPosBounds(int x, int y) => ((x >= 0) && (x < this.width) && (y >= 0) && (y < this.height));
    // Check whether a given position is within the idArray/colorArray bounds

    private Color CheckPosColor(int x, int y) => this.colorArray[(y * this.width) + x];
    // Check the color of given position in colorArray

    private int CheckPosID(int x, int y) => this.idArray[(y * this.width) + x];
    // Chcek the ID of given position in idArray

    private void SetPosColor(int x, int y, Color color) => this.colorArray[(y * this.width) + x] = color;
    // Set color of given position in colorArray

    private void SetPosID(int x, int y, int id) => this.idArray[(y * this.width) + x] = id;
    // Set ID of given position in idArray

    private SandType? TryGetSandTypeFromID(int id)
    // Try tocConvert sand ID to sand type, returns null if the id cant be converted
    {
        return this.sandTypeLookupFromID.TryGetValue(id, out var sandType) ? sandType : null;
    }

    private void UpdateColorArray()
    // Updates the colorArray based on the idArray
    {
        for (int i = 0; i < this.idArray.Length; i++)
        {
            int id = this.idArray[i];
            this.colorArray[i] = id == 0
                ? this.backgroundColor
                : this.sandColorLookupFromID.TryGetValue(id, out var color) ? color : this.backgroundColor;
        }
    }
    public void UpdateTexture()
    // Update the raylib texture with the current idArray
    {
        UpdateColorArray();
        Raylib.UpdateTexture(texture: this.texture, pixels: this.colorArray);
    }

    public void UnloadTexture() => Raylib.UnloadTexture(texture: this.texture);

    private void SpawnBenchmarkSand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int x = rng.Next(0, width);
            int y = rng.Next(0, height / 2); // Spawn in top half so it also simulate falling sand
            SetPosID(x, y, sandTypes[rng.Next(sandTypes.Length)].GetID);
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
    public int[] GetIdArray => this.idArray;
    public Texture2D GetTexture => this.texture;
    public SandType[] GetSandTypes => this.sandTypes;

}
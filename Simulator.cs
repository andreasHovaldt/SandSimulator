using System.Numerics;
using Raylib_cs;

namespace SandSimulator;

class Simulator
{
    private readonly int width;
    private readonly int height;
    private readonly float scale;
    private readonly Color backgroundColor;
    private readonly SandType[] sandTypes;
    private readonly Random rng = new Random();
    private Color[] colorArray;
    private Texture2D texture;

    public Simulator(int width, int height, float scale, Color? backgroundColor = null)
    {
        this.width = width;
        this.height = height;
        this.sandTypes = [new YellowSand(), new BlueSand(), new GraySand()];
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
                foreach (SandType sandType in this.sandTypes)
                {
                    // If a pixel matches the given color, check if it is able to move
                    if (CheckPosColor(x, y).Equals(sandType.GetColor))
                    {
                        // Check the allowed movements for the specific sand type
                        foreach ((int, int) newRelativePosition in sandType.GetMovementArray)
                        {
                            (int, int) newTruePosition = (x + newRelativePosition.Item1, y + newRelativePosition.Item2);
                            if (CheckPosBounds(newTruePosition.Item1, newTruePosition.Item2) && CheckPosColor(newTruePosition.Item1, newTruePosition.Item2).Equals(this.backgroundColor))
                            {
                                SetPosColor(newTruePosition.Item1, newTruePosition.Item2, sandType.GetColor);
                                SetPosColor(x, y, this.backgroundColor);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }


    public void MousePaint(Color color, int brushSize = 3, bool allowOverwrite = false, MouseButton triggerButton = MouseButton.Left)
    {
        (int trueMouseX, int trueMouseY) = TrueMousePositionInt();

        if (Raylib.IsMouseButtonDown(triggerButton) && (allowOverwrite || CheckPosColor(trueMouseX, trueMouseY).Equals(this.backgroundColor)))
        {
            for (int xOffset = -brushSize; xOffset <= brushSize; xOffset++)
            {
                for (int yOffset = -brushSize; yOffset <= brushSize; yOffset++)
                {
                    int xPos = trueMouseX + xOffset;
                    int yPos = trueMouseY + yOffset;

                    if ((xPos >= 0) && (xPos < this.width) && (yPos >= 0) && (yPos < this.height))
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

    private bool CheckPosBounds(int x, int y) // Could be made a bit more sleek by removing the if and directly returning the bool evaluation
    {
        if ((x >= 0) && (x < this.width) && (y >= 0) && (y < this.height)) return true;
        else return false;
    }

    private Color CheckPosColor(int x, int y)
    {
        return this.colorArray[(y * this.width) + x];
    }

    private void SetPosColor(int x, int y, Color color)
    {
        this.colorArray[(y * this.width) + x] = color;
    }

    public void UpdateTexture() => Raylib.UpdateTexture(texture: this.texture, pixels: this.colorArray);
    public void UnloadTexture() => Raylib.UnloadTexture(texture: this.texture);


    public Color[] GetColorArray => this.colorArray;
    public Texture2D GetTexture => this.texture;
    public SandType[] GetSandTypes => this.sandTypes;

}
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Raylib_cs;

namespace SandSimulator;

class Simulator
{
    private readonly int width;
    private readonly int height;
    private readonly float scale;
    private readonly Color backgroundColor;
    private Random rng = new Random();
    private Color[] colorArray;
    private Texture2D texture;

    public Simulator(int width, int height, float scale, Color? backgroundColor = null)
    {
        this.width = width;
        this.height = height;
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
    public void SimulateColorMovement(Color color, int diagonalStability = 1)
    {
        // Reverse direction every iteration
        this.evenFrame = !evenFrame;

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
                // If a pixel matches the given color, check if it is able to move
                if (CheckPosColor(x, y).Equals(color))
                {
                    CheckPixelForMovement(x, y, color, diagonalStability);
                }
            }
        }
    }

    private void CheckPixelForMovement(int x, int y, Color color, int diagonalStability = 1)
    {
        if (CheckPosBounds(x, y) && this.colorArray[(y * this.width) + x].Equals(color))
        {
            // NOTE: The top is 0, while the bottom is this.height (Inverted y-axis compared to the usual coordinate system)

            // Check underneath position first (always preferred)
            if (CheckPosBounds(x, y + 1) && CheckPosColor(x, y + 1).Equals(this.backgroundColor))
            {
                SetPosColor(x, y + 1, color); // Paint new position
                SetPosColor(x, y, this.backgroundColor); // Paint old pos with background color
            }
            else
            {
                // Randomly decide which diagonal to check first
                int direction = this.rng.Next(0, 2) == 0 ? 1 : -1; // 1 = right first, -1 = left first

                // Check first diagonal direction
                if (CheckPosBounds(x + direction, y + diagonalStability) && CheckPosColor(x + direction, y + diagonalStability).Equals(this.backgroundColor))
                {
                    SetPosColor(x + direction, y + diagonalStability, color);
                    SetPosColor(x, y, this.backgroundColor);
                }
                // Check other diagonal direction
                else if (CheckPosBounds(x - direction, y + diagonalStability) && CheckPosColor(x - direction, y + diagonalStability).Equals(this.backgroundColor))
                {
                    SetPosColor(x - direction, y + diagonalStability, color);
                    SetPosColor(x, y, this.backgroundColor);
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

    public void DisplayMousePosition()
    {
        (int trueMouseX, int trueMouseY) = TrueMousePositionInt();

        for (int xOffset = -2; xOffset <= 2; xOffset++)
        {
            for (int yOffset = -2; yOffset <= 2; yOffset++)
            {
                int xPos = trueMouseX + xOffset;
                int yPos = trueMouseY + yOffset;

                if (CheckPosBounds(xPos, yPos))
                {
                    this.colorArray[yPos * this.width + xPos] = Color.Red;
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


    public Color[] ColorArray => this.colorArray;
    public Texture2D Texture => this.texture;

}


// class Pixel
// {
//     public Pixel()
//     {
//         Console.WriteLine("Init pixel");
//     }

//     public ()

// }
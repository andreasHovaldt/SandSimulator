using Raylib_cs;

namespace SandSimulator;

class Simulator
{
    private readonly int width;
    private readonly int height;
    private Color[] colorArray;
    private Texture2D texture;

    public Simulator(int width, int height)
    {
        this.width = width;
        this.height = height;

        // Initilize a flat color array for texture data
        this.colorArray = new Color[this.width * this.height];
        Array.Fill(this.colorArray, Color.Green);

        // Initilize texture using blank image loaded as Texture2D
        Image blankImage = Raylib.GenImageColor(width: this.width, height: this.height, color: Color.White);
        this.texture = Raylib.LoadTextureFromImage(image: blankImage);
        Raylib.UnloadImage(image: blankImage);
    }

    public void GenWhiteNoise()
    {
        Random rng = new Random();
        for (int x = 0; x < this.width; x++)
        {
            for (int y = 0; y < this.height; y++)
            {
                Color rngColor = rng.Next(0, 2) switch
                {
                    0 => Color.White,
                    1 => Color.Black,
                    _ => Color.Red,
                };
                this.colorArray[y * width + x] = rngColor;
            }
        }
    }

    public void UpdateTexture() => Raylib.UpdateTexture(texture: this.texture, pixels: this.colorArray);
    public void UnloadTexture() => Raylib.UnloadTexture(texture: this.texture);


    public Color[] ColorArray => this.colorArray;
    public Texture2D Texture => this.texture;

}
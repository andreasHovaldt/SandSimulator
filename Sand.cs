using Raylib_cs;

namespace SandSimulator;


// Parent class for containing the class blueprint for each sand type
class SandType
{
    // Fields
    protected readonly int horizontalViscosity; // Protected allows children to access, but the calling object cannot access
    protected readonly int verticalMovementResistance;
    protected readonly int weight;
    protected readonly Color color;
    // protected Random rng;
    protected readonly (int, int)[] movementArray;

    // Constructor
    public SandType(Color color, int horizontalViscosity, int verticalMovementResistance, int weight)
    {
        this.color = color;
        this.horizontalViscosity = horizontalViscosity;
        this.verticalMovementResistance = verticalMovementResistance;
        this.weight = weight;
        // this.rng = new Random();
        this.movementArray = this.MovementArray();
        Console.WriteLine("Created new sand type!");
    }

    // Class methods
    private (int x, int y)[] MovementArray()
    // Should return an array of (x,y) tuples with a sorted order of possible movements for the sand type
    {
        // Static elements have no allowed movements
        if (this.horizontalViscosity == 0 && this.verticalMovementResistance == 0)
        {
            return [];
        }

        int movementArraySize = 3 + (this.horizontalViscosity * 2);
        (int, int)[] movementOrderArray = new (int, int)[movementArraySize];

        // Vertical movement (down, diagonal down-right, diagonal down-left)
        movementOrderArray[0] = (0, 1);
        movementOrderArray[1] = (1, this.verticalMovementResistance);
        movementOrderArray[2] = (-1, this.verticalMovementResistance);

        // Horizontal movement (alternating right/left with increasing distance)
        int index = 3;
        for (int distance = 1; distance <= this.horizontalViscosity; distance++)
        {
            movementOrderArray[index++] = (distance, 0);
            movementOrderArray[index++] = (-distance, 0);
        }

        return movementOrderArray;
    }

    // Class properties
    public Color GetColor => this.color;
    public int GetHorizontalViscosity => this.horizontalViscosity;
    public int GetVerticalMovementResistance => this.verticalMovementResistance;
    public int GetWeight => this.weight;
    public (int, int)[] GetMovementArray => this.movementArray;
}

class BlueSand : SandType
// Emulates water
{
    public BlueSand(int horizontalViscosity = 5, int verticalMovementResistance = 1, int weight = 2)
        : base(new Color(0, 121, 241, 255), horizontalViscosity, verticalMovementResistance, weight) // Blue RGB values
    {
        Console.WriteLine($"Created {this.GetType()} type!");
    }
}

class YellowSand : SandType
// Emulates sand
{
    public YellowSand(int horizontalViscosity = 0, int verticalMovementResistance = 5, int weight = 5)
        : base(new Color(253, 249, 0, 255), horizontalViscosity, verticalMovementResistance, weight) // Yellow RGB values
    {
        Console.WriteLine($"Created {this.GetType()} type!");
    }
}

class GraySand : SandType
// Emulates static rock
{
    public GraySand(int horizontalViscosity = 0, int verticalMovementResistance = 0, int weight = 10)
        : base(new Color(130, 130, 130, 255), horizontalViscosity, verticalMovementResistance, weight) // Gray RGB values
    {
        Console.WriteLine($"Created {this.GetType()} type!");
    }
}
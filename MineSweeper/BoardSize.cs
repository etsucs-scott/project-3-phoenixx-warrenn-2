namespace Minesweeper.Core;


/// supported board configurations.
/// the numeric value doubles as the grid dimension (e.g. Small = 8 → 8×8)
public enum BoardSize
{
    Small = 8,   // 8×8,   10 mines
    Medium = 12,  // 12×12, 25 mines
    Large = 16   // 16×16, 40 mines
}

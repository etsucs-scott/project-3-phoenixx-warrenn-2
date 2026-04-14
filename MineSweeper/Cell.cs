namespace Minesweeper.Core;


/// represents a single cell on the Minesweeper grid
/// holds mine status, visibility, flag state, and adjacency count

public class Cell
{
    /// true if this cell contains a mine.</summary>
    public bool IsMine { get; set; }

    /// true once the player has revealed this cell.</summary>
    public bool IsRevealed { get; set; }

    /// true when the player has placed a flag on this cell.</summary>
    public bool IsFlagged { get; set; }


    /// Number of mines in the 8-cell Moore neighborhood.
    /// Only meaningful when <see cref="IsMine"/> is false.
    public int AdjacentMines { get; set; }
}
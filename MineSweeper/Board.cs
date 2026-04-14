namespace Minesweeper.Core;


/// manages the Minesweeper grid: seeded mine placement, adjacency computation,
/// reveal cascades, flagging, and win detection

public class Board
{
    public int Size { get; }
    public int MineCount { get; }
    public long Seed { get; }

    private readonly Cell[,] _cells;

    public Board(BoardSize boardSize, long seed)
    {
        Size = (int)boardSize;
        MineCount = MineCountFor(boardSize);
        Seed = seed;
        _cells = new Cell[Size, Size];

        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                _cells[r, c] = new Cell();

        PlaceMines(seed);
        ComputeAdjacency();
    }

    // mine placement

    /// place mines deterministically using a seeded RNG
    private void PlaceMines(long seed)
    {
        var rng = new Random((int)(seed ^ (seed >> 32)));
        int placed = 0;

        while (placed < MineCount)
        {
            int r = rng.Next(Size);
            int c = rng.Next(Size);

            if (!_cells[r, c].IsMine)
            {
                _cells[r, c].IsMine = true;
                placed++;
            }
        }
    }

    // adjacency 

    /// pre-compute adjacent mine counts for every cell
    private void ComputeAdjacency()
    {
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                if (!_cells[r, c].IsMine)
                    _cells[r, c].AdjacentMines = CountAdjacentMines(r, c);
    }

    ///count mines in the 8-cell Moore neighborhood of (r, c)
    public int CountAdjacentMines(int r, int c)
    {
        int count = 0;
        foreach (var (nr, nc) in Neighbors(r, c))
            if (_cells[nr, nc].IsMine) count++;
        return count;
    }

    /// enumerate all in-bounds neighbors of (r, c)
    private IEnumerable<(int Row, int Col)> Neighbors(int r, int c)
    {
        for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int nr = r + dr, nc = c + dc;
                if (InBounds(nr, nc)) yield return (nr, nc);
            }
    }

    // public API

    /// retrieve a cell for reading
    public Cell GetCell(int r, int c)
    {
        AssertInBounds(r, c);
        return _cells[r, c];
    }

  
    /// reveal cell (r, c). returns false if it was a mine
    /// cascades automatically when adjacency is zero

    public bool Reveal(int r, int c)
    {
        AssertInBounds(r, c);
        var cell = _cells[r, c];

        if (cell.IsFlagged || cell.IsRevealed) return true;

        cell.IsRevealed = true;

        if (cell.IsMine) return false;

        if (cell.AdjacentMines == 0)
            foreach (var (nr, nc) in Neighbors(r, c))
                if (!_cells[nr, nc].IsRevealed && !_cells[nr, nc].IsFlagged)
                    Reveal(nr, nc);

        return true;
    }

    /// toggle a flag on a hidden cell
    public void ToggleFlag(int r, int c)
    {
        AssertInBounds(r, c);
        var cell = _cells[r, c];
        if (!cell.IsRevealed)
            cell.IsFlagged = !cell.IsFlagged;
    }

    /// true when every non-mine cell has been revealed
    public bool IsWon
    {
        get
        {
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                    if (!_cells[r, c].IsMine && !_cells[r, c].IsRevealed)
                        return false;
            return true;
        }
    }

    /// returns true when (r, c) is within the grid boundaries
    public bool InBounds(int r, int c) =>
        r >= 0 && r < Size && c >= 0 && c < Size;

    // helpers

    /// return the canonical mine count for a given board size
    private static int MineCountFor(BoardSize size) => size switch
    {
        BoardSize.Small => 10,
        BoardSize.Medium => 25,
        BoardSize.Large => 40,
        _ => throw new ArgumentOutOfRangeException(nameof(size), "Unknown board size")
    };

    /// throw a descriptive exception when coordinates are out of range
    private void AssertInBounds(int r, int c)
    {
        if (!InBounds(r, c))
            throw new ArgumentOutOfRangeException(
                $"({r},{c}) is outside the {Size}×{Size} grid.");
    }
}
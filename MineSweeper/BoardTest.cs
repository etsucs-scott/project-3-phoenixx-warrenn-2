namespace Minesweeper.Tests;

using Minesweeper.Core;
using Xunit;


/// unit tests for Minesweeper.Core
/// all tests use explicit seeds so results are 100 % deterministic

public class BoardTests
{
    // ── 1. mine counts per board size 

    [Fact]
    public void SmallBoard_Has_Exactly10Mines()
    {
        var board = new Board(BoardSize.Small, seed: 42);
        Assert.Equal(10, CountMines(board));
    }

    [Fact]
    public void MediumBoard_Has_Exactly25Mines()
    {
        var board = new Board(BoardSize.Medium, seed: 42);
        Assert.Equal(25, CountMines(board));
    }

    [Fact]
    public void LargeBoard_Has_Exactly40Mines()
    {
        var board = new Board(BoardSize.Large, seed: 42);
        Assert.Equal(40, CountMines(board));
    }

    // ── 2. determinism 

    [Fact]
    public void SameSeed_AlwaysProduces_SameBoard()
    {
        // two boards built with the same seed must be bitwise identical
        var b1 = new Board(BoardSize.Small, seed: 12345);
        var b2 = new Board(BoardSize.Small, seed: 12345);

        for (int r = 0; r < b1.Size; r++)
            for (int c = 0; c < b1.Size; c++)
                Assert.Equal(b1.GetCell(r, c).IsMine, b2.GetCell(r, c).IsMine);
    }

    [Fact]
    public void DifferentSeeds_Produce_DifferentMineLayouts()
    {
        var b1 = new Board(BoardSize.Small, seed: 1);
        var b2 = new Board(BoardSize.Small, seed: 999);

        bool anyDifference = false;
        for (int r = 0; r < b1.Size; r++)
            for (int c = 0; c < b1.Size; c++)
                if (b1.GetCell(r, c).IsMine != b2.GetCell(r, c).IsMine)
                { anyDifference = true; break; }

        Assert.True(anyDifference, "Seeds 1 and 999 should produce different layouts.");
    }

    // ── 3. Adjacency

    [Fact]
    public void AdjacentMineCounts_AreCorrect_ForAllCells()
    {
        // for every non-mine cell, verify the cached count matches a fresh recount
        var board = new Board(BoardSize.Small, seed: 99);

        for (int r = 0; r < board.Size; r++)
            for (int c = 0; c < board.Size; c++)
            {
                var cell = board.GetCell(r, c);
                if (cell.IsMine) continue;

                int expected = board.CountAdjacentMines(r, c);
                Assert.Equal(expected, cell.AdjacentMines);
            }
    }

    // ── 4. reveal mechanics

    [Fact]
    public void RevealMine_Returns_False()
    {
        var board = new Board(BoardSize.Small, seed: 42);

        // find any mine and reveal it — must signal game-over (false)
        var (mr, mc) = FindFirst(board, c => c.IsMine);
        bool result = board.Reveal(mr, mc);

        Assert.False(result);
    }

    [Fact]
    public void RevealSafeCell_Returns_True_And_MarksRevealed()
    {
        var board = new Board(BoardSize.Small, seed: 42);
        var (r, c) = FindFirst(board, cell => !cell.IsMine);

        bool result = board.Reveal(r, c);

        Assert.True(result);
        Assert.True(board.GetCell(r, c).IsRevealed);
    }

    [Fact]
    public void FlaggedCell_Cannot_BeRevealed()
    {
        var board = new Board(BoardSize.Small, seed: 42);
        var (r, c) = FindFirst(board, cell => !cell.IsMine);

        board.ToggleFlag(r, c);
        board.Reveal(r, c);

        Assert.False(board.GetCell(r, c).IsRevealed,
            "A flagged cell must not be revealed until the flag is removed.");
    }

    // ── 5. flagging

    [Fact]
    public void ToggleFlag_SetsAndClearsFlag()
    {
        var board = new Board(BoardSize.Small, seed: 42);

        board.ToggleFlag(0, 0);
        Assert.True(board.GetCell(0, 0).IsFlagged);

        board.ToggleFlag(0, 0);
        Assert.False(board.GetCell(0, 0).IsFlagged);
    }

    [Fact]
    public void RevealedCell_CannotBeFlagged()
    {
        var board = new Board(BoardSize.Small, seed: 42);
        var (r, c) = FindFirst(board, cell => !cell.IsMine);

        board.Reveal(r, c);
        board.ToggleFlag(r, c); // should be ignored

        Assert.False(board.GetCell(r, c).IsFlagged,
            "An already-revealed cell must not be flaggable.");
    }

    // ── 6. cascade reveal

    [Fact]
    public void CascadeReveal_OpensNeighbors_WhenAdjacencyIsZero()
    {
        var board = new Board(BoardSize.Small, seed: 42);

        // Find a zero-adjacency safe cell to trigger cascade
        var zeroCell = FindFirstOrNull(board, cell => !cell.IsMine && cell.AdjacentMines == 0);
        if (zeroCell is null) return; // seed has no zero cells — skip gracefully

        var (r, c) = zeroCell.Value;
        board.Reveal(r, c);

        // At least one neighbor must also be revealed because of the cascade
        int revealedNeighbors = 0;
        for (int dr = -1; dr <= 1; dr++)
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int nr = r + dr, nc = c + dc;
                if (board.InBounds(nr, nc) && board.GetCell(nr, nc).IsRevealed)
                    revealedNeighbors++;
            }

        Assert.True(revealedNeighbors > 0,
            "Revealing a zero-adjacent cell should cascade to its neighbors.");
    }

    // ── 7. win / loss via Game wrapper 

    [Fact]
    public void Game_TransitionsTo_Won_WhenAllSafeCellsRevealed()
    {
        var game = new Game(BoardSize.Small, seed: 42);

        // reveal every non-mine cell directly via the Board reference
        for (int r = 0; r < game.Board.Size; r++)
            for (int c = 0; c < game.Board.Size; c++)
                if (!game.Board.GetCell(r, c).IsMine)
                    game.Reveal(r, c);

        Assert.Equal(GameState.Won, game.State);
    }

    [Fact]
    public void Game_TransitionsTo_Lost_WhenMineRevealed()
    {
        var game = new Game(BoardSize.Small, seed: 42);
        var (mr, mc) = FindFirst(game.Board, c => c.IsMine);

        game.Reveal(mr, mc);

        Assert.Equal(GameState.Lost, game.State);
    }

    // helpers 

    /// count all mine cells on a board
    private static int CountMines(Board board)
    {
        int count = 0;
        for (int r = 0; r < board.Size; r++)
            for (int c = 0; c < board.Size; c++)
                if (board.GetCell(r, c).IsMine) count++;
        return count;
    }

    /// find the (row, col) of the first cell matching <paramref name="predicate"/>
    private static (int, int) FindFirst(Board board, Func<Cell, bool> predicate)
    {
        for (int r = 0; r < board.Size; r++)
            for (int c = 0; c < board.Size; c++)
                if (predicate(board.GetCell(r, c))) return (r, c);
        throw new InvalidOperationException("No cell satisfies the predicate.");
    }

    private static (int, int)? FindFirstOrNull(Board board, Func<Cell, bool> predicate)
    {
        for (int r = 0; r < board.Size; r++)
            for (int c = 0; c < board.Size; c++)
                if (predicate(board.GetCell(r, c))) return (r, c);
        return null;
    }
}
namespace Minesweeper.Console;

using Minesweeper.Core;


/// renders the board and high-score table to the console
/// all presentation logic lives here; no game rules

public static class ConsoleRenderer
{
    // board symbols (spec-required)
    private const char SymHidden = '#';
    private const char SymFlag = 'f';
    private const char SymBomb = 'b';
    private const char SymEmpty = '.';

    // public API


    /// draw the full board with row/column guides.
    /// pass <paramref name="showMines"/>=true on game-over to reveal unflagged mines
    public static void DrawBoard(Board board, bool showMines = false)
    {
        int size = board.Size;

        // column index header
        System.Console.Write("      ");
        for (int c = 0; c < size; c++)
            System.Console.Write($"{c,3}");
        System.Console.WriteLine();

        System.Console.Write("      ");
        System.Console.WriteLine(new string('─', size * 3));

        for (int r = 0; r < size; r++)
        {
            System.Console.Write($"  {r,2} │");
            for (int c = 0; c < size; c++)
            {
                var cell = board.GetCell(r, c);
                char sym = CellSymbol(cell, showMines);
                var color = CellColor(cell, showMines);

                System.Console.ForegroundColor = color;
                System.Console.Write($"  {sym}");
                System.Console.ResetColor();
            }
            System.Console.WriteLine();
        }
        System.Console.WriteLine();
    }

    /// print the top-5 high scores for a given board size
    public static void ShowHighScores(HighScoreManager manager, BoardSize size)
    {
        var scores = manager.GetTop(size).ToList();

        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine($"  ── {size} ({(int)size}×{(int)size}) ──────────────────────");
        System.Console.ResetColor();

        if (scores.Count == 0)
        {
            System.Console.WriteLine("    (no scores yet)");
            return;
        }

        System.Console.WriteLine($"    {"Rank",-5}{"Time",7}{"Moves",7}  Seed");
        for (int i = 0; i < scores.Count; i++)
        {
            var s = scores[i];
            System.Console.WriteLine($"    #{i + 1,-4}{s.Seconds,5}s {s.Moves,6}  {s.Seed}");
        }
    }

    //private helpers

    /// choose the display character for a cell
    private static char CellSymbol(Cell cell, bool showMines)
    {
        if (cell.IsRevealed)
            return cell.IsMine ? SymBomb
                 : cell.AdjacentMines == 0 ? SymEmpty
                 : (char)('0' + cell.AdjacentMines);

        if (showMines && cell.IsMine && !cell.IsFlagged)
            return SymBomb;

        return cell.IsFlagged ? SymFlag : SymHidden;
    }

    /// choose a console colour for a cell to aid readability
    private static ConsoleColor CellColor(Cell cell, bool showMines)
    {
        if (cell.IsRevealed && cell.IsMine) return ConsoleColor.Red;
        if (cell.IsFlagged) return ConsoleColor.Yellow;
        if (showMines && cell.IsMine) return ConsoleColor.DarkRed;

        if (cell.IsRevealed)
            return cell.AdjacentMines switch
            {
                0 => ConsoleColor.DarkGray,
                1 => ConsoleColor.Blue,
                2 => ConsoleColor.Green,
                3 => ConsoleColor.Red,
                4 => ConsoleColor.DarkBlue,
                5 => ConsoleColor.DarkRed,
                6 => ConsoleColor.Cyan,
                7 => ConsoleColor.Magenta,
                8 => ConsoleColor.DarkGray,
                _ => ConsoleColor.White
            };

        return ConsoleColor.White;
    }
}
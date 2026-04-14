namespace Minesweeper.Console;

using Minesweeper.Core;


/// entry point for the Minesweeper console application
/// handles the main menu, seed prompting, game loop, and high-score display
/// all game rules are delegated to Minesweeper.Core
 

class Program
{
    // shared across all sessions in this process run
    private static readonly HighScoreManager ScoreManager = new("data/highscores.csv");

    // entry point

    static void Main()
    {
        while (true)
        {
            PrintMenu();
            string? choice = System.Console.ReadLine()?.Trim().ToLower();

            switch (choice)
            {
                case "1": StartGame(BoardSize.Small); break;
                case "2": StartGame(BoardSize.Medium); break;
                case "3": StartGame(BoardSize.Large); break;
                case "4": ShowAllHighScores(); break;
                case "q":
                    System.Console.WriteLine("\n  Goodbye!\n");
                    return;
                default:
                    System.Console.WriteLine("  Unknown option — try again.");
                    Pause(600);
                    break;
            }
        }
    }

    // menu 

    private static void PrintMenu()
    {
        System.Console.Clear();
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine();
        System.Console.WriteLine("  ╔═══════════════════════════════════╗");
        System.Console.WriteLine("  ║           MINESWEEPER             ║");
        System.Console.WriteLine("  ╠═══════════════════════════════════╣");
        System.Console.ResetColor();
        System.Console.WriteLine("  ║  1)  Small   8×8   — 10 mines    ║");
        System.Console.WriteLine("  ║  2)  Medium  12×12 — 25 mines    ║");
        System.Console.WriteLine("  ║  3)  Large   16×16 — 40 mines    ║");
        System.Console.WriteLine("  ║  4)  High Scores                  ║");
        System.Console.WriteLine("  ║  q)  Quit                         ║");
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine("  ╚═══════════════════════════════════╝");
        System.Console.ResetColor();
        System.Console.Write("\n  Choice: ");
    }

    // game flow 

    /// prompt for a seed, create a game, and run the play loop
    private static void StartGame(BoardSize size)
    {
        long seed = PromptSeed();
        var game = new Game(size, seed);

        // play loop
        while (game.State == GameState.Playing)
        {
            System.Console.Clear();
            PrintHud(game, seed);
            ConsoleRenderer.DrawBoard(game.Board);
            PrintHelp();

            System.Console.Write("  > ");
            string? line = System.Console.ReadLine()?.Trim().ToLower();

            if (line == "q") return;   // abandon game → back to menu

            ProcessCommand(game, line);
        }

        // game-over screen
        System.Console.Clear();
        bool won = game.State == GameState.Won;

        // on loss reveal all mines so the player sees what they missed
        ConsoleRenderer.DrawBoard(game.Board, showMines: !won);

        if (won)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"  🎉  You won!  Time: {game.ElapsedSeconds}s  |  Moves: {game.Moves}  |  Seed: {seed}");
            System.Console.ResetColor();

            ScoreManager.Add(new HighScore
            {
                Size = size,
                Seconds = game.ElapsedSeconds,
                Moves = game.Moves,
                Seed = seed,
                Timestamp = DateTime.UtcNow
            });

            System.Console.WriteLine();
            ConsoleRenderer.ShowHighScores(ScoreManager, size);
        }
        else
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"  💥  BOOM!  Better luck next time.  Seed: {seed}");
            System.Console.ResetColor();
        }

        System.Console.WriteLine("\n  Press any key to return to menu…");
        System.Console.ReadKey(intercept: true);
    }

 
    /// parse and execute one player command
    /// shows an error message and re-prompts on any invalid input

    private static void ProcessCommand(Game game, string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            Error("Empty input. Try:  r 0 0");
            return;
        }

        // split on whitespace; expect exactly three tokens
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3 || (parts[0] != "r" && parts[0] != "f"))
        {
            Error("Invalid command.  Use:  r row col   or   f row col");
            return;
        }

        if (!int.TryParse(parts[1], out int row) || !int.TryParse(parts[2], out int col))
        {
            Error("Row and col must be integers.");
            return;
        }

        if (!game.Board.InBounds(row, col))
        {
            Error($"Out of bounds. Valid range: 0 – {game.Board.Size - 1}.");
            return;
        }

        // dispatch to the appropriate game action
        if (parts[0] == "r")
            game.Reveal(row, col);
        else
            game.Flag(row, col);
    }

    // seed prompt 


    /// ask the player for a seed integer
    /// returns a timestamp-derived seed when the input is blank

    private static long PromptSeed()
    {
        System.Console.Write("\n  Enter seed (or press Enter for random): ");
        string? input = System.Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            // use milliseconds since epoch — unique enough for casual play
            long auto = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            System.Console.WriteLine($"  Using random seed: {auto}");
            Pause(700);
            return auto;
        }

        if (long.TryParse(input, out long seed))
        {
            System.Console.WriteLine($"  Using seed: {seed}");
            Pause(700);
            return seed;
        }

        System.Console.WriteLine("  Invalid seed — using random.");
        long fallback = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Pause(700);
        return fallback;
    }

    // high scores

    private static void ShowAllHighScores()
    {
        System.Console.Clear();
        System.Console.WriteLine("\n  ═══ HIGH SCORES ═══\n");
        foreach (BoardSize size in Enum.GetValues<BoardSize>())
        {
            ConsoleRenderer.ShowHighScores(ScoreManager, size);
            System.Console.WriteLine();
        }
        System.Console.WriteLine("  Press any key to return…");
        System.Console.ReadKey(intercept: true);
    }

    // small helpers

    /// print the heads-up display line above the board
    private static void PrintHud(Game game, long seed)
    {
        System.Console.ForegroundColor = ConsoleColor.DarkCyan;
        System.Console.WriteLine(
            $"  {game.BoardSize} ({game.Board.Size}×{game.Board.Size})  │  " +
            $"Seed: {seed}  │  Moves: {game.Moves}  │  Time: {game.ElapsedSeconds}s  │  " +
            $"Mines: {game.Board.MineCount}");
        System.Console.ResetColor();
        System.Console.WriteLine();
    }

    private static void PrintHelp() =>
        System.Console.WriteLine("  Commands:  r row col  (reveal)   f row col  (flag/unflag)   q  (quit)\n");

    /// print a red error message and pause briefly so it is readable
    private static void Error(string msg)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"  ✗ {msg}");
        System.Console.ResetColor();
        Pause(900);
    }

    private static void Pause(int ms) => Thread.Sleep(ms);
}
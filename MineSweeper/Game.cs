namespace Minesweeper.Core;

using System.Diagnostics;

/// orchestrates a single Minesweeper session.
/// wraps a <see cref="Board"/>, tracks elapsed time and move count,
/// and transitions <see cref="State"/> when the game ends



public class Game
{
    // public properties

    /// the underlying board for this session
    public Board Board { get; }

    /// current state: Playing, Won, or Lost
    public GameState State { get; private set; } = GameState.Playing;

    /// number of reveal actions taken (flagging is free)
    public int Moves { get; private set; }

    /// board size used for this game (stored for high-score recording)
    public BoardSize BoardSize { get; }

    /// wall-clock seconds elapsed since the game started
    public int ElapsedSeconds => (int)_stopwatch.Elapsed.TotalSeconds;

    // private state

    private readonly Stopwatch _stopwatch = new();

    // constructor

    
    /// start a new game of <paramref name="size"/> with the given <paramref name="seed"/>
    /// the stopwatch begins immediately
    
    public Game(BoardSize size, long seed)
    {
        BoardSize = size;
        Board = new Board(size, seed);
        _stopwatch.Start();
    }

    // player actions

    /// returns <c>false</c> if the reveal triggered a mine
    /// no-ops when the game is already over
   
    public bool Reveal(int r, int c)
    {
        if (State != GameState.Playing) return false;

        bool safe = Board.Reveal(r, c);
        Moves++;

        if (!safe)
        {
            // player hit a mine, stop the clock and mark as lost
            State = GameState.Lost;
            _stopwatch.Stop();
        }
        else if (Board.IsWon)
        {
            // all safe cells uncovered, player wins
            State = GameState.Won;
            _stopwatch.Stop();
        }

        return safe;
    }


    /// toggle a flag on cell (r, c). flags do not count as moves
    /// no-ops when the game is already over
    public void Flag(int r, int c)
    {
        if (State != GameState.Playing) return;
        Board.ToggleFlag(r, c);
    }
}
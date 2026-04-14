namespace Minesweeper.Core;


/// persists the top-5 high scores per board size in a CSV file under <c>data/</c>
/// file format: CSV with header <c>size,seconds,moves,seed,timestamp</c>
/// lower time wins; tie-break is fewer moves.
/// at most <see cref="TopN"/> entries per board size are kept on disk.
/// all I/O errors are caught and reported to <see cref="Console.Error"/> — the game

public class HighScoreManager
{
    private const int TopN = 5;
    private const string Header = "size,seconds,moves,seed,timestamp";

    private readonly string _filePath;
    private readonly List<HighScore> _scores = new();

    // constructor

    /// <param name="filePath">Path to the CSV file (created if missing).</param>
    public HighScoreManager(string filePath = "data/highscores.csv")
    {
        _filePath = filePath;
        Load();
    }

    //public API 

    
    /// add a new score entry and immediately persist the updated top-5 to disk
    
    public void Add(HighScore score)
    {
        _scores.Add(score);
        Save();
    }

    
    /// return the top <see cref="TopN"/> scores for the given board size,
    /// ordered by ascending time then ascending move count
   
    public IEnumerable<HighScore> GetTop(BoardSize size) =>
        _scores
            .Where(s => s.Size == size)
            .OrderBy(s => s.Seconds)
            .ThenBy(s => s.Moves)
            .Take(TopN);

    // file I/O

    
    /// load scores from <see cref="_filePath"/>, creating the file (with header only)
    /// if it does not exist yet
 
    private void Load()
    {
        try
        {
            // Make sure the directory exists before touching the file
            string? dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_filePath))
            {
                // First run — create an empty file so the header is ready
                File.WriteAllText(_filePath, Header + Environment.NewLine);
                return;
            }

            foreach (string line in File.ReadAllLines(_filePath))
            {
                // Skip header and blank lines
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("size")) continue;

                var score = HighScore.FromCsv(line);
                if (score != null) _scores.Add(score);
                // Silently skip malformed lines so one bad row doesn't wipe the whole list
            }
        }
        catch (IOException ex)
        {
            System.Console.WriteLine($"[Warning] Could not load high scores: {ex.Message}");
        }
    }

    
    /// Rewrite the file keeping only the top-5 per board size.
    /// Limits unbounded file growth if Add() is called many times.
    
    private void Save()
    {
        try
        {
            // Collect the top-N for every known size
            var toKeep = Enum.GetValues<BoardSize>()
                .SelectMany(GetTop)
                .ToList();

            var lines = new List<string>(toKeep.Count + 1) { Header };
            lines.AddRange(toKeep.Select(s => s.ToString()));

            File.WriteAllLines(_filePath, lines);
        }
        catch (IOException ex)
        {
            System.Console.WriteLine($"[Warning] Could not save high scores: {ex.Message}");
        }
    }
}
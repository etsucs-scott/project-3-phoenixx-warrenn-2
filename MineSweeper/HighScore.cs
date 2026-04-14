namespace Minesweeper.Core;


/// represents a single high-score entry
/// lower time is better; tie-break is fewer moves

public class HighScore
{
    public BoardSize Size { get; set; }
    public int Seconds { get; set; }
    public int Moves { get; set; }
    public long Seed { get; set; }
    public DateTime Timestamp { get; set; }

    // serialisation

    /// render this entry as one CSV row (no header)
    public override string ToString() =>
        $"{(int)Size},{Seconds},{Moves},{Seed},{Timestamp:O}";

    
    /// parse a CSV row produced by <see cref="ToString"/>
    /// returns on any parse failure so callers can safely skip bad lines
    
    public static HighScore? FromCsv(string line)
    {
        var parts = line.Split(',');
        if (parts.Length < 5) return null;

        try
        {
            return new HighScore
            {
                Size = (BoardSize)int.Parse(parts[0]),
                Seconds = int.Parse(parts[1]),
                Moves = int.Parse(parts[2]),
                Seed = long.Parse(parts[3]),
                // timestamp may contain commas inside timezone offset — rejoin remainder
                Timestamp = DateTime.Parse(string.Join(",", parts[4..]))
            };
        }
        catch
        {
            // malformed line — the caller will skip it
            return null;
        }
    }
}
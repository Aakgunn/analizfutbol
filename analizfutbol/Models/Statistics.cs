public class RefereeCardStatistics
{
    public int TotalMatches { get; set; }
    public double YellowCardsPerMatch { get; set; }
    public double RedCardsPerMatch { get; set; }
    public int TotalYellowCards { get; set; }
    public int TotalRedCards { get; set; }
}

public class TeamCardStatistics
{
    public int TotalMatches { get; set; }
    public int TotalYellowCards { get; set; }
    public int TotalRedCards { get; set; }
    public double YellowCardsPerMatch { get; set; }
    public double RedCardsPerMatch { get; set; }
}

public class WinRateStatistics
{
    public int TotalMatches { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public double WinRate { get; set; }
} 
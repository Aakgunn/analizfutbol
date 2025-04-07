public class RefereeDetailedAnalysis
{
    public string RefereeId { get; set; } = string.Empty;
    public string RefereeName { get; set; } = string.Empty;
    public int TotalMatches { get; set; }
    public double AverageYellowCardsPerMatch { get; set; }
    public double AverageRedCardsPerMatch { get; set; }
    public List<TeamStatistics> TeamStats { get; set; } = new();
}

public class TeamRefereeHistory
{
    public string TeamId { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public List<RefereeMatchHistory> RefereeHistory { get; set; } = new();
}

public class RefereeMatchHistory
{
    public string RefereeId { get; set; }
    public string RefereeName { get; set; }
    public int TotalMatches { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public double WinRate { get; set; }
}

public class ComparisonResult
{
    public List<ComparisonItem> Items { get; set; } = new();
}

public class ComparisonItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, double> Statistics { get; set; } = new();
}

public class TeamStatistics
{
    public string TeamId { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int TotalMatches { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public double CardsPerMatch { get; set; }
} 
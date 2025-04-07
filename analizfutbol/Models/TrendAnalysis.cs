public class CardTrend
{
    public DateTime Date { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public string RefereeId { get; set; }
    public string RefereeName { get; set; }
}

public class TeamPerformanceTrend
{
    public DateTime Date { get; set; }
    public string TeamId { get; set; }
    public string TeamName { get; set; }
    public int CardsReceived { get; set; }
    public string MatchResult { get; set; }
    public string RefereeId { get; set; }
} 
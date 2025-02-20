namespace OnLimit.Entities;

public class UsageUserPlans
{
    public string Id { get; set; } = string.Empty;

    public string Plan { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Date { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public static string MapDate(DateTime date)
      => $"{date.Year}-{date.Month}";
}

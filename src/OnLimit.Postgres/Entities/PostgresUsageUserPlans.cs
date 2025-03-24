using OnLimit.Entities;

namespace OnLimit.Postgres.Entities;

public class PostgresUsageUserPlans
{
    public Guid Id { get; set; }

    public string Plan { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string? ExternalPaymentId { get; set; }

    public string Date { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public static PostgresUsageUserPlans MapFrom(UsageUserPlans p)
      => new()
      {
          Id = Guid.Parse(p.Id),
          UserId = p.UserId,
          ExternalPaymentId = p.ExternalPaymentId,
          CreatedAt = p.CreatedAt,
          Date = p.Date,
          Plan = p.Plan
      };

    public UsageUserPlans MapToDomain()
      => new()
      {
          Id = Id.ToString(),
          UserId = UserId,
          ExternalPaymentId = ExternalPaymentId,
          CreatedAt = CreatedAt,
          Date = Date,
          Plan = Plan
      };

}

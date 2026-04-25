using System.Security.Claims;

namespace ResumeAI.Resume.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? principal.FindFirstValue("sub");
        return int.TryParse(value, out var id) ? id : 0;
    }

    public static string GetSubscriptionPlan(this ClaimsPrincipal principal)
        => principal.FindFirstValue("subscription_plan") ?? "FREE";

    public static bool IsPremium(this ClaimsPrincipal principal)
        => principal.GetSubscriptionPlan().Equals("PREMIUM", StringComparison.OrdinalIgnoreCase);

    public static bool IsAdmin(this ClaimsPrincipal principal)
        => principal.IsInRole("ADMIN");
}

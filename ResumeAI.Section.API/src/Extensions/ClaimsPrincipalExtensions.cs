using System.Security.Claims;

namespace ResumeAI.Section.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? principal.FindFirstValue("sub");
        return int.TryParse(value, out var id) ? id : 0;
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
        => principal.IsInRole("ADMIN");

    public static string GetSubscriptionPlan(this ClaimsPrincipal principal)
        => principal.FindFirstValue("subscription_plan") ?? "FREE";
}

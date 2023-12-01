namespace TwoFactorAuthAPI.Models
{
    public interface ITwoFactorAuthConfig
    {
        int CodeLifetimeMinutes { get; set; }
        int ConcurrentCodesPerPhone { get; set; }
    }
}
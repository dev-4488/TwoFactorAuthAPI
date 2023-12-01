namespace TwoFactorAuthAPI.Models
{
    public class TwoFactorAuthConfig : ITwoFactorAuthConfig
    {
        public int CodeLifetimeMinutes { get; set; }
        public int ConcurrentCodesPerPhone { get; set; }
    }
}

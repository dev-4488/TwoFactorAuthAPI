namespace TwoFactorAuthAPI.Models
{
    public class ConfirmationCode
    {
        public string Code { get; set; }
        public DateTime CreationTime { get; set; }
    }

}

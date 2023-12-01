using IdentityServer4.Models;

namespace TwoFactorAuthAPI.Models
{
    public class IdentityServerConfig
    {
        public static IEnumerable<Client> GetClients() =>
            new List<Client>
            {
            new Client
            {
                ClientId = "Demo",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("Test@123".Sha256()) },
                AllowedScopes = { "api1" }
               
            }
            };

        public static IEnumerable<ApiScope> GetApiScopes() =>
            new List<ApiScope>
            {
            new ApiScope("api1", "My API")
            };
    }

}

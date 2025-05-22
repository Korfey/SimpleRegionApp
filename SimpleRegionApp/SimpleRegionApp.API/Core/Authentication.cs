using Amazon;
using Amazon.RDS.Util;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement.Internal;

namespace SimpleRegionApp.API.Core;

public static class Authentication
{
    public static string GetConnectionString(string DatabaseEndpoint, string Password)
    {
        return $"Server={DatabaseEndpoint},1433;Database=SimpleDb;User Id=madenovartur;Password={Password};Encrypt=True;TrustServerCertificate=False;";
    }
}

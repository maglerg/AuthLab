
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace AuthLab.Provider.Services;

public class JwksService
{
    private readonly IConfiguration _configuration;

    public JwksService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public object GetJwks()
    {
        var publicKeyPath = _configuration["Jwt:PublicKeyPath"]
            ?? throw new InvalidOperationException(
                "JWT public key path is not configured.");

        var keyId = _configuration["Jwt:KeyId"]
            ?? throw new InvalidOperationException(
                "JWT key ID is not configured.");

        if (!File.Exists(publicKeyPath))
        {
            throw new InvalidOperationException(
                "JWT public key file was not found.");
        }

        using var rsa = RSA.Create();

        var publicKey = File.ReadAllText(publicKeyPath);
        rsa.ImportFromPem(publicKey);

        var parameters = rsa.ExportParameters(false);

        return new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA",
                    use = "sig",
                    alg = SecurityAlgorithms.RsaSha256,
                    kid = keyId,

                    n = Base64UrlEncoder.Encode(
                        parameters.Modulus!),

                    e = Base64UrlEncoder.Encode(
                        parameters.Exponent!)
                }
            }
        };
    }
}

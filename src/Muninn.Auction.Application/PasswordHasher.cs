using System.Security.Cryptography;
using System.Text;
using Muninn.Auction.Application.Common;
using static System.Convert;

namespace Muninn.Auction.Application;

internal sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10_000;
    private const char Separator = '-';

    private readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA512;
    
    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _algorithm, HashSize);

        return new StringBuilder()
            .Append(ToHexString(hash))
            .Append(Separator)
            .Append(ToHexString(salt))
            .ToString();
    }

    public bool Verify(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split(Separator);
        var hash = FromHexString(parts[0]);
        var salt = FromHexString(parts[1]);
        var verifyHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _algorithm, HashSize);
        
        return CryptographicOperations.FixedTimeEquals(hash, verifyHash);
    }
}

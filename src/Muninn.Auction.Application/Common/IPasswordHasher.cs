namespace Muninn.Auction.Application.Common;

public interface IPasswordHasher
{
    string HashPassword(string password);
    
    bool Verify(string password, string hashedPassword);
}

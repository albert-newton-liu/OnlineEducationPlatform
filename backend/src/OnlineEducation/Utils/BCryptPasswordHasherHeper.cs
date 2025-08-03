using BCrypt.Net; // Correct namespace for BCrypt.Net-Next

namespace OnlineEducation.Utils;

public class BCryptPasswordHasher
{
    public static string HashPassword(string password)
    {
        // BCrypt.Net.BCrypt.HashPassword automatically generates a salt
        // and embeds it within the returned hash string.
        // workFactor (cost) parameter controls the computational cost of hashing.
        // Higher values are more secure but slower. Recommended values are 10-14.
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        // BCrypt.Net.BCrypt.Verify automatically extracts the salt from the hashed password
        // and uses it to verify the provided plain-text password.
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
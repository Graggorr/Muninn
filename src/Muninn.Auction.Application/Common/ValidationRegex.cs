using System.Text.RegularExpressions;
using static System.Text.RegularExpressions.RegexOptions;

namespace Muninn.Auction.Application.Common;

/// <summary>
/// Source-generated regular expressions backing the profile field validation rules.
/// </summary>
internal static partial class ValidationRegex
{
    /// <summary>
    /// Username: 3-20 characters, starts with a letter, made of letters and digits that may be
    /// separated by a single dot, underscore or hyphen. Separators can neither repeat nor end the
    /// username, so "john.doe" and "j_d-1" pass while "john..doe", "_john" and "john." fail.
    /// </summary>
    [GeneratedRegex(@"^(?=.{3,20}$)[a-zA-Z][a-zA-Z0-9]*(?:[._-][a-zA-Z0-9]+)*$", CultureInvariant)]
    public static partial Regex Username { get; }

    /// <summary>
    /// Password: 8-64 characters with no whitespace, containing at least one lowercase letter, one
    /// uppercase letter, one digit and one special character.
    /// </summary>
    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s])\S{8,64}$", CultureInvariant)]
    public static partial Regex Password { get; }

    /// <summary>
    /// Mobile number in E.164 form: a leading '+', a non-zero country-code digit and 8-15 digits in
    /// total. No spaces, dashes or parentheses - the number is expected to be normalised already.
    /// </summary>
    [GeneratedRegex(@"^\+[1-9]\d{7,14}$", CultureInvariant)]
    public static partial Regex MobileNumber { get; }
}

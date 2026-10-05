using System;
using System.Security.Cryptography;
using System.Text;

namespace Switchboard.Evaluation;

/// <summary>
/// Deterministic rollout bucketing, identical across every Switchboard SDK.
/// See docs/SPEC.md §4. Uses SHA1 so every language's standard library produces
/// the same bucket for the same (flagKey, salt, contextKey).
/// </summary>
public static class Bucketing
{
    // 13 hex 'f' characters = 52 bits = 4503599627370495 (2^52 - 1),
    // chosen so the integer is exactly representable as a double in every language.
    private const double MaxValue = 0xFFFFFFFFFFFFF;

    /// <summary>Returns a stable bucket in [0, 1) for the given inputs.</summary>
    public static double BucketOf(string flagKey, string salt, string contextKey)
    {
        var input = $"{flagKey}.{salt}.{contextKey}";
#if NET8_0_OR_GREATER
        Span<byte> hash = stackalloc byte[20];
        SHA1.HashData(Encoding.UTF8.GetBytes(input), hash);
        var hex = Convert.ToHexString(hash); // upper-case, 40 chars
#else
        byte[] hash;
        using (var sha1 = SHA1.Create())
            hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash) sb.Append(b.ToString("X2"));
        var hex = sb.ToString();
#endif
        var hex13 = hex.Substring(0, 13);
        long n = Convert.ToInt64(hex13, 16);
        return n / MaxValue;
    }
}

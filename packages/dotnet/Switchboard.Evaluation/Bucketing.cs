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
    // 15 hex 'f' characters = 60 bits = 1152921504606846975
    private const double MaxValue = 0xFFFFFFFFFFFFFFF;

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
        var hex15 = hex.Substring(0, 15);
        long n = Convert.ToInt64(hex15, 16);
        return n / MaxValue;
    }
}

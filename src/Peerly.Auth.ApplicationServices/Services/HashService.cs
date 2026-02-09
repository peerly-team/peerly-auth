using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;
using Peerly.Auth.ApplicationServices.Services.Abstractions;

namespace Peerly.Auth.ApplicationServices.Services;

internal sealed class HashService : IHashService
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int DegreeOfParallelism = 8; // Number of threads to use
    private const int Iterations = 4; // Number of iterations
    private const int MemorySize = 64 * 1024; // 64 MB

    public Task<string> HashAsync(string text, CancellationToken cancellationToken) => Task.Run(() => HashCore(text), cancellationToken);

    private string HashCore(string text)
    {
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        var hash = HashCore(text, salt);

        var combinedBytes = new byte[salt.Length + hash.Length];
        Array.Copy(salt, 0, combinedBytes, 0, salt.Length);
        Array.Copy(hash, 0, combinedBytes, salt.Length, hash.Length);

        return Convert.ToBase64String(combinedBytes);
    }

    public bool Verify(string text, string hashedText)
    {
        var combinedBytes = Convert.FromBase64String(hashedText);

        var salt = new byte[SaltSize];
        var hash = new byte[HashSize];
        Array.Copy(combinedBytes, 0, salt, 0, SaltSize);
        Array.Copy(combinedBytes, SaltSize, hash, 0, HashSize);

        var newHash = HashCore(text, salt);

        return CryptographicOperations.FixedTimeEquals(hash, newHash);
    }

    private static byte[] HashCore(string text, byte[] salt)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(text))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            Iterations = Iterations,
            MemorySize = MemorySize
        };

        return argon2.GetBytes(HashSize);
    }
}

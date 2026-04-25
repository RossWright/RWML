using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace RossWright;

/// <summary>
/// Pure-managed Elliptic Curve Digital Signature Algorithm (ECDSA) implementation.
/// Unlike <see cref="System.Security.Cryptography.ECDsa"/>, this implementation
/// runs in Blazor WebAssembly where the BCL cryptography stack is not fully available,
/// allowing signing and verification to execute client-side in the browser.
/// </summary>
public static partial class Ecdsa
{
    /// <summary>
    /// Generates a new ECDSA key pair encoded as PEM strings.
    /// Store the private key securely; the public key may be distributed freely.
    /// </summary>
    /// <param name="privateKeyPem">
    /// Receives the generated private key in PEM format. Keep this secret.
    /// </param>
    /// <param name="publicKeyPem">
    /// Receives the corresponding public key in PEM format.
    /// </param>
    /// <example>
    /// <code>
    /// Ecdsa.GenerateKeyPair(out var privateKeyPem, out var publicKeyPem);
    /// </code>
    /// </example>
    public static void GenerateKeyPair(out string privateKeyPem, out string publicKeyPem)
    {
        var privateKeyObj = new PrivateKey();
        privateKeyPem = privateKeyObj.ToPem();
        publicKeyPem = Der.ToPem(privateKeyObj.PublicKey.ToDerBytes(), "PUBLIC KEY");
    }

    /// <summary>
    /// Verifies that <paramref name="signatureBase64"/> was produced by signing
    /// <paramref name="data"/> with the private key matching <paramref name="publicKeyPem"/>.
    /// </summary>
    /// <param name="publicKeyPem">The PEM-encoded public key.</param>
    /// <param name="signatureBase64">The Base64-encoded signature to verify.</param>
    /// <param name="data">The original data that was signed.</param>
    /// <returns>
    /// <see langword="true"/> if the signature is valid for the given data and key;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool Verify(string publicKeyPem, string signatureBase64, string data) =>
        PublicKey.FromDer(Der.FromPem(publicKeyPem))
            .Verify(data, Signature.FromBase64(signatureBase64));

    /// <summary>
    /// Signs <paramref name="data"/> using <paramref name="privateKeyPem"/> and returns
    /// the signature as a Base64-encoded string.
    /// </summary>
    /// <param name="privateKeyPem">The PEM-encoded private key.</param>
    /// <param name="data">The data to sign.</param>
    /// <returns>A Base64-encoded ECDSA signature.</returns>
    public static string Sign(string privateKeyPem, string data) =>
        PrivateKey.FromPem(privateKeyPem).Sign(data).ToBase64();


    internal record Point(BigInteger X, BigInteger Y, BigInteger Z) { }

    internal record CurveFp(BigInteger A, BigInteger B, BigInteger P, BigInteger N, Point G, string Name, int[] Oid, string NistName = "")
    {
        public bool Contains(Point p)
        {
            if (p.X < 0 || p.X > P - 1) return false;
            if (p.Y < 0 || p.Y > P - 1) return false;
            if (!Modulo(BigInteger.Pow(p.Y, 2) - (BigInteger.Pow(p.X, 3) + A * p.X + B), P).IsZero) return false;
            return true;
        }

        public int Length => N.ToString("X").Length / 2;
    }

    internal record Signature(BigInteger R, BigInteger S)
    {
        public string ToBase64() => Convert.ToBase64String(Der.EncodeSequence(Der.EncodeInteger(R), Der.EncodeInteger(S)));

        public static Signature FromBase64(string str)
        {
            Der.RemoveSequence(Convert.FromBase64String(str), out var rs, out var removeSequenceTrail);
            if (removeSequenceTrail.Length > 0) throw new ArgumentException("trailing junk after DER signature: " + HexFromBinary(removeSequenceTrail));
            Der.RemoveInteger(rs, out var r, out var rest);
            Der.RemoveInteger(rest, out var s, out var removeIntegerTrail);
            if (removeIntegerTrail.Length > 0) throw new ArgumentException("trailing junk after DER numbers: " + HexFromBinary(removeIntegerTrail));
            return new Signature(r, s);
        }
    }

    internal static string HashSha256(string message)
    {
        using SHA256 sha256Hash = SHA256.Create();
        return string.Concat(sha256Hash
            .ComputeHash(Encoding.UTF8.GetBytes(message))
            .Select(_ => _.ToString("x2")));
    }

    internal static string HexFromBinary(byte[] bytes)
    {
        var hex = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            hex.AppendFormat("{0:x2}", b);
        }
        return hex.ToString();
    }

    internal static byte[] BinaryFromHex(string hex)
    {
        int numberChars = hex.Length;
        if (numberChars % 2 == 1)
        {
            hex = "0" + hex;
            numberChars++;
        }
        byte[] bytes = new byte[numberChars / 2];
        for (int i = 0; i < numberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }

    internal static BigInteger NumberFromHex(string hex)
    {
        if (hex.Length % 2 == 1 || hex[0] != '0')
        {
            hex = "0" + hex; // if the hex string doesnt start with 0, the parse will assume its negative
        }
        return BigInteger.Parse(hex, NumberStyles.HexNumber);
    }

    internal static byte[] StringFromNumber(BigInteger number, int length)
    {
        string hex = number.ToString("X");
        if (hex.Length <= 2 * length) hex = new string('0', 2 * length - hex.Length) + hex;
        else if (hex[0] == '0') hex = hex.Substring(1);
        else throw new ArgumentException("number hex length is bigger than 2*length: " + number + ", length=" + length);
        return BinaryFromHex(hex);
    }

    internal static byte[] SliceByteArray(byte[] bytes, int start, int length = int.MaxValue)
    {
        int newLength = System.Math.Min(bytes.Length - start, length);
        byte[] result = new byte[newLength];
        Array.Copy(bytes, start, result, 0, newLength);
        return result;
    }

    internal static byte[] IntToCharBytes(int num) => [(byte)num];

    internal static BigInteger Modulo(BigInteger dividend, BigInteger divisor)
    {
        BigInteger remainder = BigInteger.Remainder(dividend, divisor);
        return remainder < 0 ? remainder + divisor : remainder;
    }

    internal static BigInteger RandomBetween(BigInteger minimum, BigInteger maximum)
    {
        if (maximum < minimum) throw new ArgumentException("maximum must be greater than minimum");

        var bitsNeeded = 0;
        var bytesNeeded = 0;
        var mask = new BigInteger(1);
        for (BigInteger range = maximum - minimum; range > 0; range >>= 1)
        {
            if (bitsNeeded % 8 == 0) bytesNeeded += 1;
            bitsNeeded++;
            mask = mask << 1 | 1;
        }

        byte[] randomBytes = new byte[bytesNeeded];
        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(randomBytes);
        }

        BigInteger randomValue = new BigInteger(randomBytes);
        randomValue &= mask;
        if (randomValue <= maximum - minimum) return minimum + randomValue;
        return RandomBetween(minimum, maximum);
    }

    internal static Point Multiply(Point p, BigInteger n, BigInteger N, BigInteger A, BigInteger P) =>
        FromJacobian(JacobianMultiply(ToJacobian(p), n, N, A, P), P);

    internal static Point Add(Point p, Point q, BigInteger A, BigInteger P) =>
        FromJacobian(JacobianAdd(ToJacobian(p), ToJacobian(q), A, P), P);

    internal static BigInteger Inv(BigInteger x, BigInteger n)
    {
        if (x.IsZero) return 0;

        BigInteger lm = BigInteger.One;
        BigInteger hm = BigInteger.Zero;
        BigInteger low = Modulo(x, n);
        BigInteger high = n;
        BigInteger r, nm, newLow;

        while (low > 1)
        {
            r = high / low;

            nm = hm - (lm * r);
            newLow = high - (low * r);

            high = low;
            hm = lm;
            low = newLow;
            lm = nm;
        }

        return Modulo(lm, n);

    }

    private static Point ToJacobian(Point p) => new Point(p.X, p.Y, 1);

    private static Point FromJacobian(Point p, BigInteger P)
    {
        BigInteger z = Inv(p.Z, P);
        return new Point(
            Modulo(p.X * BigInteger.Pow(z, 2), P),
            Modulo(p.Y * BigInteger.Pow(z, 3), P),
            BigInteger.Zero);
    }

    private static Point JacobianDouble(Point p, BigInteger A, BigInteger P)
    {
        if (p.Y.IsZero) return new Point(BigInteger.Zero, BigInteger.Zero, BigInteger.Zero);
        var ysq = Modulo(BigInteger.Pow(p.Y, 2), P);
        var S = Modulo(4 * p.X * ysq, P);
        var M = Modulo(3 * BigInteger.Pow(p.X, 2) + A * BigInteger.Pow(p.Z, 4), P);
        var nx = Modulo(BigInteger.Pow(M, 2) - 2 * S, P);
        var ny = Modulo(M * (S - nx) - 8 * BigInteger.Pow(ysq, 2), P);
        var nz = Modulo(2 * p.Y * p.Z, P);
        return new Point(nx, ny, nz);
    }

    private static Point JacobianAdd(Point p, Point q, BigInteger A, BigInteger P)
    {
        if (p.Y.IsZero) return q;
        if (q.Y.IsZero) return p;
        var U1 = Modulo(p.X * BigInteger.Pow(q.Z, 2), P);
        var U2 = Modulo(q.X * BigInteger.Pow(p.Z, 2), P);
        var S1 = Modulo(p.Y * BigInteger.Pow(q.Z, 3), P);
        var S2 = Modulo(q.Y * BigInteger.Pow(p.Z, 3), P);

        if (U1 == U2)
        {
            if (S1 != S2) return new Point(BigInteger.Zero, BigInteger.Zero, BigInteger.One);
            return JacobianDouble(p, A, P);
        }

        var H = U2 - U1;
        var R = S2 - S1;
        var H2 = Modulo(H * H, P);
        var H3 = Modulo(H * H2, P);
        var U1H2 = Modulo(U1 * H2, P);
        var nx = Modulo(BigInteger.Pow(R, 2) - H3 - 2 * U1H2, P);
        var ny = Modulo(R * (U1H2 - nx) - S1 * H3, P);
        var nz = Modulo(H * p.Z * q.Z, P);
        return new Point(nx, ny, nz);
    }

    private static Point JacobianMultiply(Point p, BigInteger n, BigInteger N, BigInteger A, BigInteger P)
    {
        if (p.Y.IsZero | n.IsZero) return new Point(BigInteger.Zero, BigInteger.Zero, BigInteger.One);
        if (n.IsOne) return p;
        if (n < 0 | n >= N) return JacobianMultiply(p, Modulo(n, N), N, A, P);
        if (Modulo(n, 2).IsZero) return JacobianDouble(JacobianMultiply(p, n / 2, N, A, P), A, P);
        return JacobianAdd(JacobianDouble(JacobianMultiply(p, n / 2, N, A, P), A, P), p, A, P);
    }
}
using System.Numerics;

namespace RossWright;

public static partial class Ecdsa
{
    internal static class Curves
    {
        public static CurveFp GetCurveByName(string name)
        {
            name = name.ToLower();
            if (name == "secp256k1") return secp256k1;
            if (name == "p256" | name == "prime256v1") return prime256v1;
            throw new ArgumentException("unknown curve " + name);
        }

        public static CurveFp secp256k1 { get; } = new CurveFp(
            NumberFromHex("0000000000000000000000000000000000000000000000000000000000000000"),
            NumberFromHex("0000000000000000000000000000000000000000000000000000000000000007"),
            NumberFromHex("fffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f"),
            NumberFromHex("fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141"),
            new Point(
                NumberFromHex("79be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798"),
                NumberFromHex("483ada7726a3c4655da4fbfc0e1108a8fd17b448a68554199c47d08ffb10d4b8"),
                BigInteger.Zero),
            "secp256k1",
            [1, 3, 132, 0, 10]
        );

        public static CurveFp prime256v1 { get; } = new CurveFp(
            NumberFromHex("ffffffff00000001000000000000000000000000fffffffffffffffffffffffc"),
            NumberFromHex("5ac635d8aa3a93e7b3ebbd55769886bc651d06b0cc53b0f63bce3c3e27d2604b"),
            NumberFromHex("ffffffff00000001000000000000000000000000ffffffffffffffffffffffff"),
            NumberFromHex("ffffffff00000000ffffffffffffffffbce6faada7179e84f3b9cac2fc632551"),
            new Point(
                NumberFromHex("6b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296"),
                NumberFromHex("4fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f5"),
                BigInteger.Zero),
            "prime256v1",
            [1, 2, 840, 10045, 3, 1, 7],
            "P-256"
        );

        public static CurveFp[] SupportedCurves { get; } = { secp256k1, prime256v1 };

        public static Dictionary<string, CurveFp> CurvesByOid { get; } = new Dictionary<string, CurveFp>() 
        {
            { string.Join(",", secp256k1.Oid), secp256k1 },
            { string.Join(",", prime256v1.Oid), prime256v1 }
        };
    }
}
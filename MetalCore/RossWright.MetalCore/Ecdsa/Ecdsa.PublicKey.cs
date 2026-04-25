using System.Numerics;

namespace RossWright;

public static partial class Ecdsa
{
    internal class PublicKey(Point _point, CurveFp _curve)
    {
        public byte[] ToStringBytes() => 
            Der.CombineByteArrays(
                BinaryFromHex("00"),
                BinaryFromHex("04"),
                StringFromNumber(_point.X, _curve.Length),
                StringFromNumber(_point.Y, _curve.Length));

        public byte[] ToDerBytes() =>
            Der.EncodeSequence(
                Der.EncodeSequence(
                    Der.EncodeOid([1, 2, 840, 10045, 2, 1]),
                    Der.EncodeOid(_curve.Oid)),
                Der.EncodeBitString(ToStringBytes()));

        public static PublicKey FromDer(byte[] der)
        {
            Der.RemoveSequence(der, out var s1, out var rest);

            if (rest.Length > 0) 
                throw new ArgumentException("trailing junk after DER public key: " + HexFromBinary(rest));

            Der.RemoveSequence(s1, out var s2, out var pointBitString);
            Der.RemoveObject(s2, out _, out rest);
            Der.RemoveObject(rest, out var oidCurve, out var remaining);

            if (remaining.Length > 0) 
                throw new ArgumentException("trailing junk after DER public key objects: " + HexFromBinary(remaining));

            string stringOid = string.Join(",", oidCurve);

            if (!Curves.CurvesByOid.ContainsKey(stringOid))
            {
                int numCurves = Curves.SupportedCurves.Length;
                string[] supportedCurves = Enumerable.Range(0, numCurves).Select(_ => Curves.SupportedCurves[_].Name).ToArray();

                throw new ArgumentException($"Unknown curve with oid [{string.Join(", ", oidCurve)}]. " +
                    $"Only the following are available: {string.Join(", ", supportedCurves)}");
            }

            Der.RemoveBitString(pointBitString, out var pointString, out remaining);
            if (remaining.Length > 0) throw new ArgumentException("trailing junk after public key point-string");

            var curveObject = Curves.CurvesByOid[stringOid];
            
            var str = SliceByteArray(pointString, 2);
            if (str.Length != 2 * curveObject.Length) throw new ArgumentException("string length [" + str.Length + "] should be " + 2 * curveObject.Length);
            var xs = HexFromBinary(SliceByteArray(str, 0, curveObject.Length));
            var ys = HexFromBinary(SliceByteArray(str, curveObject.Length));
            var p = new Point(NumberFromHex(xs), NumberFromHex(ys), BigInteger.Zero);

            if (p.Y.IsZero) throw new ArgumentException("Public Key point is at infinity");
            if (!curveObject.Contains(p)) throw new ArgumentException($"Point ({p.X},{p.Y}) is not valid for curve {curveObject.Name}");
            if (!Multiply(p, curveObject.N, curveObject.N, curveObject.A, curveObject.P).Y.IsZero)
                throw new ArgumentException($"Point ({p.X},{p.Y}) * {curveObject.Name}.N is not at infinity");
            
            return  new PublicKey(p, curveObject);
        }

        public bool Verify(string message, Signature signature)
        {
            string hashMessage = HashSha256(message);
            BigInteger numberMessage = NumberFromHex(hashMessage);
            CurveFp curve = _curve;
            BigInteger sigR = signature.R;
            BigInteger sigS = signature.S;

            if (sigR < 1 || sigR >= curve.N) return false;
            if (sigS < 1 || sigS >= curve.N) return false;

            BigInteger inv = Inv(sigS, curve.N);

            Point u1 = Multiply(curve.G, Modulo(numberMessage * inv, curve.N), curve.N, curve.A, curve.P);
            Point u2 = Multiply(_point, Modulo(sigR * inv, curve.N), curve.N, curve.A, curve.P);
            Point v = Add(u1, u2, curve.A, curve.P);
            if (v.Y.IsZero) return false;
            return Modulo(v.X, curve.N) == sigR;
        }
    }
}
using System.Numerics;

namespace RossWright;

public static partial class Ecdsa
{
    internal class PrivateKey
    {
        public CurveFp Curve { get; private set; }
        public BigInteger Secret { get; private set; }

        public PrivateKey(string curve = "secp256k1", BigInteger? secret = null)
        {
            Curve = Curves.GetCurveByName(curve);
            secret ??= RandomBetween(1, Curve.N - 1);
            Secret = (BigInteger)secret;
        }

        public PublicKey PublicKey =>
            new PublicKey(Multiply(Curve.G, Secret, Curve.N, Curve.A, Curve.P), Curve);

        public string ToPem() => Der.ToPem(
            Der.EncodeSequence(
                Der.EncodeInteger(1),
                Der.EncodeOctetString(StringFromNumber(Secret, Curve.Length)),
                Der.EncodeConstructed(0, Der.EncodeOid(Curve.Oid)),
                Der.EncodeConstructed(1, PublicKey.ToStringBytes())),
            "EC PRIVATE KEY");

        public static PrivateKey FromPem(string str)
        {
            string[] split = str.Split(new string[] { "-----BEGIN EC PRIVATE KEY-----" }, StringSplitOptions.None);
            if (split.Length != 2) throw new ArgumentException("invalid PEM");
            return FromDer(Der.FromPem(split[1]));
        }

        public static PrivateKey FromDer(byte[] der)
        {
            Der.RemoveSequence(der, out var removeSequenceItem1, out var removeSequenceItem2);
            if (removeSequenceItem2.Length > 0) throw new ArgumentException("trailing junk after DER private key: " 
                + HexFromBinary(removeSequenceItem2));

            Der.RemoveInteger(removeSequenceItem1, out var removeIntegerItem1, out var removeIntegerItem2);
            if (removeIntegerItem1 != 1) throw new ArgumentException("expected '1' at start of DER private key, got " 
                + removeIntegerItem1.ToString());

            Der.RemoveOctetString(removeIntegerItem2, out var privateKeyStr, out var removeOctetStringItem2);

            Der.RemoveConstructed(removeOctetStringItem2, out var tag, out var curveOidString, out _);
            if (tag != 0) throw new ArgumentException("expected tag 0 in DER private key, got " + tag.ToString());

            Der.RemoveObject(curveOidString, out var oidCurve, out var removeObjectItem2);
            if (removeObjectItem2.Length > 0) throw new ArgumentException(
                "trailing junk after DER private key curve_oid: "
                + HexFromBinary(removeObjectItem2));

            var stringOid = string.Join(",", oidCurve);
            if (!Curves.CurvesByOid.ContainsKey(stringOid)) throw new ArgumentException(
                $"Unknown curve with oid [{string.Join(", ", oidCurve)}]. " 
                + $"Only the following are available: {string.Join(", ", Curves.SupportedCurves.Select(_ => _.Name))}");

            var curve = Curves.CurvesByOid[stringOid];

            if (privateKeyStr.Length < curve.Length)
            {
                privateKeyStr = Der.CombineByteArrays(BinaryFromHex(
                    new string('0', (curve.Length - privateKeyStr.Length) * 2)), 
                    privateKeyStr);
            }

            return new PrivateKey(curve.Name, NumberFromHex(HexFromBinary(privateKeyStr)));
        }

        public Signature Sign(string message)
        {
            var randNum = RandomBetween(BigInteger.One, Curve.N - 1);
            var r = Modulo(Multiply(Curve.G, randNum, Curve.N, Curve.A, Curve.P).X, Curve.N);
            return new Signature(r, Modulo(
                (NumberFromHex(HashSha256(message)) + r * Secret) * 
                Inv(randNum, Curve.N), Curve.N));
        }
    }
}
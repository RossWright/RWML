using System.Numerics;
using System.Text;

namespace RossWright;

public static partial class Ecdsa
{
    internal static class Der
    {

        private static readonly int hex31 = 0x1f;
        private static readonly int hex127 = 0x7f;
        private static readonly int hex128 = 0x80;
        private static readonly int hex160 = 0xa0;
        private static readonly int hex224 = 0xe0;

        private static readonly string hexAt = "00";
        private static readonly string hexB = "02";
        private static readonly string hexC = "03";
        private static readonly string hexD = "04";
        private static readonly string hexF = "06";
        private static readonly string hex0 = "30";

        private static readonly byte[] bytesHexAt = BinaryFromHex(hexAt);
        private static readonly byte[] bytesHexB = BinaryFromHex(hexB);
        private static readonly byte[] bytesHexC = BinaryFromHex(hexC);
        private static readonly byte[] bytesHexD = BinaryFromHex(hexD);
        private static readonly byte[] bytesHexF = BinaryFromHex(hexF);
        private static readonly byte[] bytesHex0 = BinaryFromHex(hex0);

        public static byte[] EncodeSequence(params byte[][] encodedPieces) =>
            CombineByteArrays([
                bytesHex0, 
                EncodeLength(encodedPieces.Sum(_ => _.Length)), 
                ..encodedPieces]);

        public static byte[] EncodeInteger(BigInteger x)
        {
            if (x < 0) throw new ArgumentException("x cannot be negative");
            string t = x.ToString("X");
            if (t.Length % 2 == 1) t = "0" + t;
            byte[] xBytes = BinaryFromHex(t);
            return xBytes[0] <= hex127
                ? CombineByteArrays(
                    bytesHexB,
                    IntToCharBytes(xBytes.Length),
                    xBytes)
                : CombineByteArrays(
                    bytesHexB,
                    IntToCharBytes(xBytes.Length + 1),
                    bytesHexAt,
                    xBytes);

        }

        public static byte[] EncodeOid(int[] oid)
        {
            if (oid[0] > 2) throw new ArgumentException("first has to be <= 2");
            if (oid[1] > 39) throw new ArgumentException("second has to be <= 39");
            byte[] body = CombineByteArrays([
                IntToCharBytes(40 * oid[0] + oid[1]),
                ..Enumerable.Range(2, oid.Length-2).Select(i => EncodeNumber(oid[i]))]);
            return CombineByteArrays(
                bytesHexF,
                EncodeLength(body.Length),
                body);
        }

        public static byte[] EncodeBitString(byte[] t) =>
            CombineByteArrays(bytesHexC, EncodeLength(t.Length), t);

        public static byte[] EncodeOctetString(byte[] t) =>
            CombineByteArrays(bytesHexD, EncodeLength(t.Length), t);

        public static byte[] EncodeConstructed(int tag, byte[] value) =>
            CombineByteArrays(IntToCharBytes(hex160 + tag), EncodeLength(value.Length), value);

        public static void RemoveSequence(byte[] bytes, out byte[] rs, out byte[] removeSequence)
        {
            CheckSequenceError(bytes, hex0, "30");
            ReadLength(SliceByteArray(bytes, 1), out var length, out var lengthLen);
            int endSeq = 1 + lengthLen + length;
            rs = SliceByteArray(bytes, 1 + lengthLen, length);
            removeSequence = SliceByteArray(bytes, endSeq);
        }

        public static void RemoveInteger(byte[] bytes, out BigInteger num, out byte[] rest)
        {
            CheckSequenceError(bytes, hexB, "02");
            ReadLength(SliceByteArray(bytes, 1), out var length, out var lengthLen);
            var numberBytes = SliceByteArray(bytes, 1 + lengthLen, length);
            rest = SliceByteArray(bytes, 1 + lengthLen + length);
            if (numberBytes[0] >= hex128) throw new ArgumentException("first byte of integer must be < 128");
            num = NumberFromHex(HexFromBinary(numberBytes));
        }

        public static void RemoveObject(byte[] bytes, out int[] numbers, out byte[] rest)
        {
            CheckSequenceError(bytes, hexF, "06");

            ReadLength(SliceByteArray(bytes, 1), out var length, out var lengthLen);

            byte[] body = SliceByteArray(bytes, 1 + lengthLen, length);
            rest = SliceByteArray(bytes, 1 + lengthLen + length);

            List<int> numbersList = new List<int>();
            while (body.Length > 0)
            {
                ReadNumber(body, out var number, out var numberLength);
                numbersList.Add(number);
                body = SliceByteArray(body, numberLength);
            }

            int n0 = numbersList[0];
            numbersList.RemoveAt(0);

            int first = n0 / 40;
            int second = n0 - 40 * first;
            numbersList.Insert(0, first);
            numbersList.Insert(1, second);

            numbers = numbersList.ToArray();
        }

        public static void RemoveBitString(byte[] bytes, out byte[] body, out byte[] rest)
        {
            CheckSequenceError(bytes, hexC, "03");
            ReadLength(SliceByteArray(bytes, 1), out var length, out var lengthLen);
            body = SliceByteArray(bytes, 1 + lengthLen, length);
            rest = SliceByteArray(bytes, 1 + lengthLen + length);
        }

        public static void RemoveOctetString(byte[] bytes, out byte[] body, out byte[] rest)
        {
            CheckSequenceError(bytes, hexD, "04");
            ReadLength(SliceByteArray(bytes, 1), out var length, out var lengthLen);
            body = SliceByteArray(bytes, 1 + lengthLen, length);
            rest = SliceByteArray(bytes, 1 + lengthLen + length);
        }

        public static void RemoveConstructed(byte[] bytes, out int tag, out byte[] body, out byte[] rest)
        {
            int s0 = bytes[0];
            if ((s0 & hex224) != hex160) throw new ArgumentException("wanted constructed tag (0xa0-0xbf), got " + s0);
            tag = s0 & hex31;
            ReadLength(SliceByteArray(bytes, 1), out var length, out var lengthLen);
            body = SliceByteArray(bytes, 1 + lengthLen, length);
            rest = SliceByteArray(bytes, 1 + lengthLen + length);
        }

        public static byte[] FromPem(string pem)
        {
            string[] split = pem.Split(new string[] { "\n" }, StringSplitOptions.None);
            List<string> stripped = new List<string>();

            for (int i = 0; i < split.Length; i++)
            {
                string line = split[i].Trim();
                if (!line.StartsWith("-----"))
                {
                    stripped.Add(line);
                }
            }

            return Convert.FromBase64String(string.Join("", stripped));
        }

        public static string ToPem(byte[] der, string name)
        {
            var b64 = Convert.ToBase64String(der);
            StringBuilder sb = new();
            sb.AppendLine($"-----BEGIN {name}-----");
            for (int i = 0; i < b64.Length; i += 64)
            {
                sb.AppendLine(b64[i..System.Math.Min(i + 64, b64.Length)]);
            }
            sb.AppendLine($"-----END {name}-----");
            return sb.ToString();
        }

        public static byte[] CombineByteArrays(params byte[][] byteArrayList)
        {
            int totalLength = byteArrayList.Sum(_ => _.Length);
            byte[] combined = new byte[totalLength];
            int consumedLength = 0;
            foreach (byte[] bytes in byteArrayList)
            {
                Array.Copy(bytes, 0, combined, consumedLength, bytes.Length);
                consumedLength += bytes.Length;
            }
            return combined;
        }

        private static byte[] EncodeLength(int length)
        {
            if (length < 0) throw new ArgumentException("length cannot be negative");
            if (length < hex128) return IntToCharBytes(length);
            string s = length.ToString("X");
            if (s.Length % 2 == 1) s = "0" + s;
            byte[] bytes = BinaryFromHex(s);
            return CombineByteArrays(IntToCharBytes(hex128 | bytes.Length), bytes);
        }

        private static byte[] EncodeNumber(int n)
        {
            List<int> b128Digits = new List<int>();

            while (n > 0)
            {
                b128Digits.Insert(0, n & hex127 | hex128);
                n >>= 7;
            }

            int b128DigitsCount = b128Digits.Count;

            if (b128DigitsCount == 0)
            {
                b128Digits.Add(0);
                b128DigitsCount++;
            }

            b128Digits[b128DigitsCount - 1] &= hex127;

            return CombineByteArrays(b128Digits.Select(_ => IntToCharBytes(_)).ToArray());
        }

        private static void ReadLength(byte[] bytes, out int length, out int lengthLen)
        {
            int num = bytes[0];

            if ((num & hex128) == 0)
            {
                length = num & hex127;
                lengthLen = 1;
            }
            else
            {
                lengthLen = num & hex127;
                if (lengthLen > bytes.Length - 1) throw new ArgumentException("ran out of length bytes");
                length = int.Parse(
                    HexFromBinary(SliceByteArray(bytes, 1, lengthLen)),
                    System.Globalization.NumberStyles.HexNumber);
                lengthLen = 1 + lengthLen;
            }
        }

        private static void ReadNumber(byte[] str, out int number, out int lengthLen)
        {
            number = 0;
            lengthLen = 0;
            int d;

            while (true)
            {
                if (lengthLen > str.Length) throw new ArgumentException("ran out of length bytes");
                number <<= 7;
                d = str[lengthLen];
                number += d & hex127;
                lengthLen += 1;
                if ((d & hex128) == 0) break;
            }
        }

        private static void CheckSequenceError(byte[] bytes, string start, string expected)
        {
            if (HexFromBinary(bytes).Substring(0, start.Length) != start)
                throw new ArgumentException($"wanted sequence {expected.Substring(0, 2)}, " +
                    $"got {(bytes[0].ToString("X"))}");
        }
    }
}
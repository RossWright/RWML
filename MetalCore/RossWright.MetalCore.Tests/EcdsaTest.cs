
using System.Numerics;
using static RossWright.Ecdsa;

namespace RossWright.MetalCore.Tests;

public class EcdsaTest
{
    [Fact] public void HappyPath()
    {
        var privKey = new PrivateKey();
        var pubKey = privKey.PublicKey;
        var msg = "TEST MESSAGE";
        var sig = privKey.Sign(msg);
        pubKey.Verify(msg, sig).ShouldBeTrue();
    }

    [Fact] public void HackedMsgDetect()
    {
        var privKey = new PrivateKey();
        var pubKey = privKey.PublicKey;
        var sig = privKey.Sign("TEST MESSAGE");
        pubKey.Verify("HACK MESSAGE", sig).ShouldBeFalse();
    }

    // Both supported curves produce valid sign/verify cycles
    [Theory]
    [InlineData("secp256k1")]
    [InlineData("prime256v1")]
    public void HappyPathOnAllCurves(string curveName)
    {
        var privKey = new PrivateKey(curveName);
        var sig = privKey.Sign("TEST MESSAGE");
        privKey.PublicKey.Verify("TEST MESSAGE", sig).ShouldBeTrue();
    }

    // Static string-based API: full roundtrip
    [Fact] public void StaticApi_SignAndVerify_ReturnsTrue()
    {
        GenerateKeyPair(out var privateKeyPem, out var publicKeyPem);
        var sig = Sign(privateKeyPem, "TEST MESSAGE");
        Verify(publicKeyPem, sig, "TEST MESSAGE").ShouldBeTrue();
    }

    // Static API: a signature cannot be verified by a different key pair
    [Fact] public void StaticApi_WrongPublicKey_ReturnsFalse()
    {
        GenerateKeyPair(out var privateKeyPem, out _);
        GenerateKeyPair(out _, out var otherPublicKeyPem);
        var sig = Sign(privateKeyPem, "TEST MESSAGE");
        Verify(otherPublicKeyPem, sig, "TEST MESSAGE").ShouldBeFalse();
    }

    // Static API: a signature cannot be verified against a different message
    [Fact] public void StaticApi_TamperedMessage_ReturnsFalse()
    {
        GenerateKeyPair(out var privateKeyPem, out var publicKeyPem);
        var sig = Sign(privateKeyPem, "ORIGINAL MESSAGE");
        Verify(publicKeyPem, sig, "TAMPERED MESSAGE").ShouldBeFalse();
    }

    // A private key serialized to PEM and restored can still sign verifiable messages
    [Fact] public void PrivateKeyPemRoundtrip_CanStillSign()
    {
        var privKey = new PrivateKey();
        var restored = PrivateKey.FromPem(privKey.ToPem());
        var sig = restored.Sign("TEST MESSAGE");
        privKey.PublicKey.Verify("TEST MESSAGE", sig).ShouldBeTrue();
    }

    // A public key serialized to DER
    [Fact] public void PublicKeyDerRoundtrip_CanStillVerify()
    {
        var privKey = new PrivateKey();
        var sig = privKey.Sign("TEST MESSAGE");
        var restoredPubKey = PublicKey.FromDer(privKey.PublicKey.ToDerBytes());
        restoredPubKey.Verify("TEST MESSAGE", sig).ShouldBeTrue();
    }

    // Signing a message a second time with the same key produces a different (r, s) pair
    // because ECDSA uses a random nonce k each time
    [Fact] public void SigningSameMessageTwice_ProducesDifferentSignatures()
    {
        var privKey = new PrivateKey();
        var sig1 = privKey.Sign("TEST MESSAGE");
        var sig2 = privKey.Sign("TEST MESSAGE");
        (sig1.R == sig2.R && sig1.S == sig2.S).ShouldBeFalse();
    }

    // Signing with key A cannot be verified by key B's public key
    [Fact] public void VerifyWithWrongPublicKey_ReturnsFalse()
    {
        var signerKey = new PrivateKey();
        var otherKey = new PrivateKey();
        var sig = signerKey.Sign("TEST MESSAGE");
        otherKey.PublicKey.Verify("TEST MESSAGE", sig).ShouldBeFalse();
    }

    // Verify rejects a signature whose R component is out of range (< 1)
    [Fact] public void VerifySignatureWithZeroR_ReturnsFalse()
    {
        var privKey = new PrivateKey();
        privKey.PublicKey.Verify("TEST MESSAGE", new Signature(BigInteger.Zero, BigInteger.One)).ShouldBeFalse();
    }

    // Verify rejects a signature whose S component is out of range (< 1)
    [Fact] public void VerifySignatureWithZeroS_ReturnsFalse()
    {
        var privKey = new PrivateKey();
        privKey.PublicKey.Verify("TEST MESSAGE", new Signature(BigInteger.One, BigInteger.Zero)).ShouldBeFalse();
    }

    // SHA-256 is defined for all inputs; empty string is a valid message
    [Fact] public void SignAndVerify_EmptyMessage()
    {
        var privKey = new PrivateKey();
        var sig = privKey.Sign("");
        privKey.PublicKey.Verify("", sig).ShouldBeTrue();
    }

    // SHA-256 operates on UTF-8 bytes; unicode characters must round-trip correctly
    [Fact] public void SignAndVerify_UnicodeMessage()
    {
        var privKey = new PrivateKey();
        var msg = "Unicode: \u4e2d\u6587 \ud83d\udd10";
        var sig = privKey.Sign(msg);
        privKey.PublicKey.Verify(msg, sig).ShouldBeTrue();
    }

    // Public key derivation (G * secret) is purely deterministic; the same secret always yields the same public key
    [Fact] public void SameSecret_AlwaysProducesSamePublicKey()
    {
        var secret = new BigInteger(0x1234567890ABCDEFUL);
        var key1 = new PrivateKey("secp256k1", secret);
        var key2 = new PrivateKey("secp256k1", secret);
        key2.PublicKey.ToDerBytes().ShouldBe(key1.PublicKey.ToDerBytes());
    }

    // A PEM string that does not begin with the expected header is rejected immediately
    [Fact] public void FromPem_InvalidHeader_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => PrivateKey.FromPem("not a valid pem"));
    }
}

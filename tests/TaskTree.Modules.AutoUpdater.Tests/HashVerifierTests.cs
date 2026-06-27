// SPEC-DERIVED-PHASE3A  HALT #19

using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Modules.AutoUpdater;

namespace TaskTree.Modules.AutoUpdater.Tests
{
    [TestClass]
    public class HashVerifierTests
    {
        [TestMethod]
        public void ComputeSha256Hex_KnownPayload_ReturnsExpected()
        {
            var verifier = new HashVerifier();
            Assert.AreEqual("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD", verifier.ComputeSha256Hex(Encoding.UTF8.GetBytes("abc")));
        }

        [TestMethod]
        public void VerifySha256_ValidHash_ReturnsTrue()
        {
            var verifier = new HashVerifier();
            var payload = Encoding.UTF8.GetBytes("abc");
            Assert.IsTrue(verifier.VerifySha256(payload, verifier.ComputeSha256Hex(payload)));
        }

        [TestMethod]
        public void VerifySha256_TamperedPayload_ReturnsFalse()
        {
            var verifier = new HashVerifier();
            var hash = verifier.ComputeSha256Hex(Encoding.UTF8.GetBytes("abc"));
            Assert.IsFalse(verifier.VerifySha256(Encoding.UTF8.GetBytes("abcd"), hash));
        }

        [TestMethod]
        public void VerifySha256_InvalidHex_ReturnsFalse()
        {
            Assert.IsFalse(new HashVerifier().VerifySha256(Encoding.UTF8.GetBytes("abc"), "not-hex"));
        }

        [TestMethod]
        public void VerifySha256_CaseInsensitiveHex_ReturnsTrue()
        {
            var verifier = new HashVerifier();
            var payload = Encoding.UTF8.GetBytes("abc");
            Assert.IsTrue(verifier.VerifySha256(payload, verifier.ComputeSha256Hex(payload).ToLowerInvariant()));
        }
    }
}

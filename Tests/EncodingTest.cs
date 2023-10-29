using MARC;

namespace Tests
{
    [TestClass]
    public class EncodingTest
    {
        [TestMethod]
        public void Test_InstantiateClass()
        {
            Marc8 marc8 = new Marc8();
            Assert.IsNotNull(marc8);
        }

        [TestMethod]
        public void Test_Empty()
        {
            Marc8 marc8 = new Marc8();

            byte[] bytes = { };
            char[] chars = { };
            char[] result = marc8.GetChars(bytes);

            CollectionAssert.AreEqual(chars, result);
        }

        [TestMethod]
        public void Test_ASCII()
        {
            Marc8 marc8 = new Marc8();

            byte[] bytes = { 0x20, 0x23, 0x21 };
            char[] chars = { ' ', '#', '!' };
            char[] result = marc8.GetChars(bytes);

            CollectionAssert.AreEqual(chars, result);
        }

        [TestMethod]
        public void Test_ANSEL()
        {
            Marc8 marc8 = new Marc8();

            byte[] bytes = { 0x1B, 0x29, 0x21, 0x45, 0xF0, 0x63, 0x61 };
            char[] chars = { 'c', '\u0327', 'a' };
            char[] result = marc8.GetChars(bytes);

            CollectionAssert.AreEqual(chars, result);
        }

        [TestMethod]
        public void Test_Debug()
        {
            Marc8 marc8 = new Marc8();

            byte[] bytes = { 0xF0, 0x63, 0x61 };
            char[] chars = { 'c', '\u0327', 'a' };
            char[] result = marc8.GetChars(bytes);

            CollectionAssert.AreEqual(chars, result);
        }

        /* Test of the alternate escape sequence {ESC s} for ASCII. */
        [TestMethod]
        public void Test_SubLatin()
        {
            Marc8 marc8 = new Marc8();

            byte[] bytes = { 0x1B, 0x73, 0x61, 0x62, 0x63 };
            char[] chars = { 'a', 'b', 'c' };
            char[] result = marc8.GetChars(bytes);

            CollectionAssert.AreEqual(chars, result);
        }

        /* Test of the Greek subset using the escape sequence {ESC g}. */
        [TestMethod]
        public void Test_SubGreek()
        {
            Marc8 marc8 = new Marc8();

            byte[] bytes = { 0x1B, 0x67, 0x61, 0x62, 0x63 };
            char[] chars = { '\u03B1', '\u03B2', '\u03B3' };
            char[] result = marc8.GetChars(bytes);

            CollectionAssert.AreEqual(chars, result);
        }

        /* Test of the subscript subset using the escape sequence {ESC b}. */
        [TestMethod]
        public void Test_Subscript()
        {
            Marc8 marc8 = new Marc8();

            byte[] bytes = { 0x41, 0x1B, 0x62, 0x31, 0x32, 0x33 };
            char[] chars = { 'A', '\u2081', '\u2082', '\u2083' };
            char[] result = marc8.GetChars(bytes);

            CollectionAssert.AreEqual(chars, result);
        }

        /* Test of the superscript subset using the escape sequence {ESC p}. */
        [TestMethod]
        public void Test_Superscript()
        {
            Marc8 marc8 = new Marc8();

            byte[] bytes = { 0x41, 0x1B, 0x70, 0x31, 0x32, 0x33 };
            char[] chars = { 'A', '\u00B9', '\u00B2', '\u00B3' };
            char[] result = marc8.GetChars(bytes);

            CollectionAssert.AreEqual(chars, result);
        }

        [TestMethod]
        public void Test_EastAsian()
        {
            Marc8 marc8 = new Marc8();

            byte[] bytes = { 0x1B, 0x24, 0x31, 0x21, 0x30, 0x64 };
            char[] chars = { '\u4EBA' };
            char[] result = marc8.GetChars(bytes);

            CollectionAssert.AreEqual(chars, result);
        }
    }
}
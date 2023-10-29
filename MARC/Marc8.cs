using System.Globalization;
using System.Text;

namespace MARC
{
    public class Marc8 : Encoding
    {
        public const int CODE_PAGE = 0xE016; // 0xE000 (private use area) + 22 (reference to ISO-2022)

        private static readonly CharacterSet CHARSET;

        static Marc8()
        {
            CHARSET = new CharacterSet();
            CHARSET.Load("res/codetables.xml");
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            return charCount;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            CoderState state = CoderState.CODE;
            byte[] multibyte = new byte[] { 0x01, 0x01 };
            char[] graphic = new char[] { (char)0x42, (char)0x45 };

            int charCount = 0;
            int i = 0;
            int bkey;
            byte bytesPerChar = 0x01;
            int g = 0;
            byte b;

            while (i < count)
            {
                if (index + i >= bytes.Length)
                {
                    break;
                }
                b = bytes[index + i];

                if (b == 0x1B)
                {
                    state = CoderState.ESCAPE;
                    ++i;
                    continue;
                }

                switch (state)
                {
                    case CoderState.ESCAPE:
                        bytesPerChar = 0x01;
                        g = 0;
                        if ((b >= 0x20) && (b < 0x30))
                        {
                            state = CoderState.IMM;
                        }
                        else if (b >= 0x30)
                        {
                            state = CoderState.FINAL;
                        }
                        else
                        {
                            state = CoderState.CODE;
                        }
                        break;
                    case CoderState.IMM:
                        if (b == 0x21)
                        {
                            ++i;
                        }
                        else if (b == 0x24)
                        {
                            bytesPerChar = 0x03;
                            ++i;
                        }
                        else if ((b & 0x0A) == 0x08) // 8,9,C,D
                        {
                            g = ((b & 0x01) == 0x00) ? 0 : 1;
                            ++i;
                        }
                        else
                        {
                            multibyte[g] = bytesPerChar;
                            state = CoderState.FINAL;
                        }
                        break;
                    case CoderState.FINAL:
                        graphic[g] = (char)b;
                        ++i;
                        state = CoderState.CODE;
                        break;
                    case CoderState.CODE:
                        g = (b & 0x80) == 0x00 ? 0 : 1;
                        bkey = b;

                        if ((bkey >= 0x00) && (bkey <= 0x20))
                        {
                            // decode with C0
                            ++charCount;
                            ++i;
                        }
                        else if ((bkey >= 0x80) && (bkey <= 0xA0))
                        {
                            // decode with C1
                            ++charCount;
                            ++i;
                        }
                        else
                        {
                            for (int byteCounter = 1; byteCounter < multibyte[g]; ++byteCounter)
                            {
                                bkey <<= 8;
                                bkey |= bytes[index + i + byteCounter];
                            }
                            charCount += CHARSET.GetChar(graphic[g], bkey).Length;
                            i += multibyte[g];
                        }
                        break;
                    default:
                        ++i;
                        break;
                }
            }
            return charCount;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            CoderState state = CoderState.CODE;
            byte[] multibyte = new byte[] { 0x01, 0x01 };
            char[] graphic = new char[] { (char)0x42, (char)0x45 };

            int charCount = 0;
            int i = 0;
            int bkey = 0;
            byte bytesPerChar = 0x01;
            int g = 0;
            byte b;
            string ch, diacritics = "";

            while (i < byteCount)
            {
                if (byteIndex + i >= bytes.Length)
                {
                    break;
                }
                b = bytes[byteIndex + i];

                if (b == 0x1B)
                {
                    state = CoderState.ESCAPE;
                    ++i;
                    continue;
                }

                switch (state)
                {
                    case CoderState.ESCAPE:
                        bytesPerChar = 0x01;
                        g = 0;
                        if ((b >= 0x20) && (b < 0x30))
                        {
                            state = CoderState.IMM;
                        }
                        else if (b >= 0x30)
                        {
                            state = CoderState.FINAL;
                        }
                        else
                        {
                            state = CoderState.CODE;
                        }
                        break;
                    case CoderState.IMM:
                        if (b == 0x21)
                        {
                            ++i;
                        }
                        else if (b == 0x24)
                        {
                            bytesPerChar = 0x03;
                            ++i;
                        }
                        else if ((b & 0x0A) == 0x08) // 8,9,C,D
                        {
                            g = ((b & 0x01) == 0x00) ? 0 : 1;
                            ++i;
                        }
                        else
                        {
                            multibyte[g] = bytesPerChar;
                            state = CoderState.FINAL;
                        }
                        break;
                    case CoderState.FINAL:
                        graphic[g] = (char)b;
                        ++i;
                        state = CoderState.CODE;
                        break;
                    case CoderState.CODE:
                        g = (b & 0x80) == 0x00 ? 0 : 1;
                        bkey = b;

                        if ((bkey >= 0x00) && (bkey <= 0x20))
                        {
                            // decode with C0
                            ch = CHARSET.GetChar((char)0x42, bkey);
                        }
                        else if ((bkey >= 0x80) && (bkey <= 0xA0))
                        {
                            // decode with C1
                            ch = CHARSET.GetChar((char)0x45, bkey);
                        }
                        else
                        {
                            for (int byteCounter = 1; byteCounter < multibyte[g]; ++byteCounter)
                            {
                                bkey <<= 8;
                                bkey |= bytes[byteIndex + i + byteCounter];
                            }
                            ch = CHARSET.GetChar(graphic[g], bkey);
                        }

                        char[] carr = ch.ToCharArray();
                        if (carr.Length > 0)
                        {
                            if (Char.GetUnicodeCategory(carr[0]) == UnicodeCategory.NonSpacingMark)
                            {
                                diacritics += ch;
                            }
                            else
                            {
                                ch = ch + diacritics;
                                carr = ch.ToCharArray();
                                diacritics = "";

                                Array.Copy(carr, 0, chars, charIndex + charCount, carr.Length);
                                charCount += carr.Length;
                            }
                        }

                        i += multibyte[g];
                        break;
                    default:
                        ++i;
                        break;
                }
            }

            return charCount;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount * 3;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }
    }
}

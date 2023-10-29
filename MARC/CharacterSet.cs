using System.Xml;

namespace MARC
{
    internal class CharacterSet
    {
        protected class EncodeTable : Dictionary<string, int>
        {
            public int GetBytes(string ch)
            {
                int output = 0;
                TryGetValue(ch, out output);
                return output;
            }
        }

        protected class DecodeTable : Dictionary<int, string>
        {
            public string GetChar(int bytes)
            {
                string? output;
                TryGetValue(bytes, out output);
                return output ?? "";
            }
        }

        private Dictionary<char, EncodeTable> encodeSet;
        private Dictionary<char, DecodeTable> decodeSet;

        public CharacterSet()
        {
            encodeSet = new Dictionary<char, EncodeTable>();
            decodeSet = new Dictionary<char, DecodeTable>();
        }

        public void Load(string filename)
        {
            string name, text;
            int hex;
            char isoCode = (char)0x42;
            int bytes = 0;
            string chars = "";

            EncodeTable enc = null;
            DecodeTable dec = null;

            using (XmlReader reader = XmlReader.Create(filename))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "characterSet":
                                    name = reader.GetAttribute("name") ?? "";
                                    text = reader.GetAttribute("ISOcode") ?? "";
                                    if (int.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out hex))
                                    {
                                        isoCode = (char)hex;
                                    }
                                    dec = new DecodeTable();
                                    break;
                                case "marc":
                                    text = reader.ReadElementContentAsString();
                                    if (int.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out hex))
                                    {
                                        bytes = hex;
                                    }
                                    break;
                                case "ucs":
                                    text = reader.ReadElementContentAsString();
                                    if (int.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out hex))
                                    {
                                        chars = Char.ConvertFromUtf32(hex);
                                        dec.Add(bytes, chars);
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case XmlNodeType.Text:
                            break;
                        case XmlNodeType.EndElement:
                            switch (reader.Name)
                            {
                                case "characterSet":
                                    // create encoding table from decoding table
                                    enc = new EncodeTable();
                                    foreach (KeyValuePair<int, string> code in dec)
                                    {
                                        if (!enc.ContainsKey(code.Value))
                                        {
                                            enc.Add(code.Value, code.Key);
                                        }
                                    }

                                    encodeSet.Add(isoCode, enc);
                                    decodeSet.Add(isoCode, dec);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public int GetBytes(char set, string ch)
        {
            return encodeSet.GetValueOrDefault(set)?.GetBytes(ch) ?? 0;
        }

        public string GetChar(char set, int bytes)
        {
            string text = "";
            if ((bytes >= 0x0000) && (bytes < 0x0100))
            {
                text = char.ConvertFromUtf32(bytes);
            }

            if (set == (char)0x73)
            {
                set = (char)0x42;
            }

            return decodeSet.GetValueOrDefault(set)?.GetChar(bytes) ?? text;
        }
    }
}

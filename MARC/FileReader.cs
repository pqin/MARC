using System.Text;

namespace MARC
{
    public class FileReader
    {
        public Record ReadRecord(BinaryReader reader)
        {
            Record record = new Record();
            Encoding encoding = Encoding.ASCII;

            string leader = Encoding.ASCII.GetString(reader.ReadBytes(24));
            record.Leader = leader;
            // determine character encoding scheme of record
            switch (leader[9])
            {
                case ' ':
                    encoding = new Marc8();
                    break;
                case 'a':
                    encoding = Encoding.UTF8;
                    break;
                default:
                    break;
            }

            int recordSize = int.Parse(leader.Substring(0, 5));
            int dirAddress = int.Parse(leader.Substring(12, 5));
            byte[] bDirectory = reader.ReadBytes(dirAddress - 24);
            byte[] bField = reader.ReadBytes(recordSize - dirAddress);

            int flength, fpos = 0;
            string directory = Encoding.ASCII.GetString(bDirectory);
            string tag, fieldData;
            for (int entry = 0; entry * 12 < bDirectory.Length - 1; ++entry)
            {
                tag = directory.Substring((12 * entry) + 0, 3);
                flength = int.Parse(directory.Substring((12 * entry) + 3, 4));
                fpos = int.Parse(directory.Substring((12 * entry) + 7, 5));
                fieldData = encoding.GetString(bField, fpos, flength);

                if (tag.StartsWith("00"))
                {
                    ControlField cfield = new ControlField();
                    cfield.Tag = tag;
                    cfield.Data = fieldData;
                    record.Fields.Add(cfield);
                }
                else
                {
                    DataField dfield = new DataField();
                    dfield.Tag = tag;
                    dfield.Indicator1 = fieldData[0];
                    dfield.Indicator2 = fieldData[1];
                    string[] subfieldData = fieldData.Substring(2).Split(
                        (char)0x1F,
                        StringSplitOptions.RemoveEmptyEntries |
                        StringSplitOptions.TrimEntries);
                    foreach (string subf in subfieldData)
                    {
                        dfield.Subfields.Add(new Subfield()
                        {
                            Code = subf[0],
                            Data = subf.Substring(1)
                        });
                    }
                    record.Fields.Add(dfield);
                }
                Thread.Sleep(0);
            }

            return record;
        }
    }
}

namespace MARC
{
    public class Record
    {
        private char[] leader;
        private List<Field> fields;

        public String Leader
        {
            get { return new string(leader); }
            set { Array.Copy(value.ToCharArray(), leader, 24); }
        }

        // Field[]
        public List<Field> Fields
        {
            get { return fields; }
            set { fields = value; }
        }

        public Record()
        {
            leader = new char[24];
            fields = new List<Field>();
        }
    }
}
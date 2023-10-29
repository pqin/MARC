namespace MARC
{
    public class DataField : Field
    {
        private char indicator1;
        private char indicator2;
        private List<Subfield> subfields;

        public char Indicator1
        {
            get
            {
                return indicator1;
            }
            set
            {
                indicator1 = value;
            }
        }
        public char Indicator2
        {
            get
            {
                return indicator2;
            }
            set
            {
                indicator2 = value;
            }
        }
        public List<Subfield> Subfields
        {
            get { return subfields; }
            set { subfields = value; }
        }

        public DataField()
        {
            indicator1 = ' ';
            indicator2 = ' ';
            subfields = new List<Subfield>();
        }
    }
}

namespace MARC
{
    public class Subfield
    {
        public char Code { get; set; }
        public string Data { get; set; }

        public Subfield()
        {
            Code = ' ';
            Data = "";
        }
    }
}

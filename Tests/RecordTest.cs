using MARC;

namespace Tests
{
    [TestClass]
    public class RecordTest
    {
        [TestMethod]
        public void Test_InstantiateClass()
        {
            Record record = new Record();
            Assert.IsNotNull(record);
        }
    }
}

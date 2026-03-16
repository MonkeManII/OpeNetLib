using OpeNetLib.Packet;

namespace OpeNetLibTests
{
    [TestClass]
    public sealed class ConstructDestructEqualityTest
    {
        [TestMethod]
        public void RunTest()
        {
            TestUInt();
            TestInt();
        }

        #region uint
        static void TestUInt()
        {
            uint value = 123u;
            PacketConstructor constructor = new(0, 4);
            constructor.WriteUInt(value);
            PacketDestructor destructor = new(constructor.ResultBytes());
            uint newv = destructor.ReadUInt();

            Assert.AreEqual(value, newv);
        }
        #endregion

        #region int
        static void TestInt()
        {
            int value = 123;
            PacketConstructor constructor = new(0, 4);
            constructor.WriteInt(value);
            PacketDestructor destructor = new(constructor.ResultBytes());
            int newv = destructor.ReadInt();

            Assert.AreEqual(value, newv);
        }
        #endregion
    }
}

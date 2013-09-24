using NUnit.Framework;
using System;
using System.Collections;

namespace JsonHelper.Test
{
    [TestFixture()]
    public class Test
    {
        [Test]
        public void SimpleObject()
        {
            object obj = JsonHelper.DecodeJson<Hashtable>("{\"hello\":\"world\",\"foo\":\"bar\",\"tool\":7}");
            Assert.AreEqual(typeof(Hashtable), obj.GetType());

            Hashtable table = (Hashtable)obj;
            Assert.AreEqual("world", table["hello"]);
            Assert.AreEqual("bar", table["foo"]);
            Assert.AreEqual(7, table["tool"]);
        }

        [Test]
        public void SimpleNestedObject()
        {
            object obj = JsonHelper.DecodeJson<Hashtable>("{\"hello\":{\"foo\": \"bar\"}}");
            Assert.AreEqual(typeof(Hashtable), obj.GetType());

            Hashtable table = (Hashtable)obj;
            Assert.IsTrue(table["hello"] is Hashtable);

            Hashtable helloTable = (Hashtable)table["hello"];
            Assert.AreEqual("bar", helloTable["foo"]);
        }

        [Test]
        public void SimpleArray()
        {
            object obj = JsonHelper.DecodeJson<ArrayList>("[10,15,20]");
            Assert.AreEqual(typeof(ArrayList), obj.GetType());

            ArrayList list = (ArrayList)obj;
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(10, list[0]);
            Assert.AreEqual(15, list[1]);
            Assert.AreEqual(20, list[2]);
        }

        [Test]
        public void MixedArrayObject()
        {
            object obj = JsonHelper.DecodeJson<ArrayList>("[{\"hello\":\"world\",\"foo\":\"bar\",\"tool\":7},15,[20,30,40]]");
            Assert.AreEqual(typeof(ArrayList), obj.GetType());

            ArrayList list = (ArrayList)obj;
            Assert.AreEqual(3, list.Count, "List count is wrong");
            Assert.AreEqual(15, list[1]);

            Assert.IsTrue(list[0] is Hashtable);
            Hashtable hash = (Hashtable)list[0];

            Assert.AreEqual("world", hash["hello"]);
            Assert.AreEqual("bar", hash["foo"]);
            Assert.AreEqual(7, hash["tool"]);

            Assert.IsTrue(list[2] is ArrayList);
            ArrayList innerList = (ArrayList)list[2];
            Assert.AreEqual(3, innerList.Count);
            Assert.AreEqual(20, innerList[0]);
            Assert.AreEqual(30, innerList[1]);
            Assert.AreEqual(40, innerList[2]);

        }
    }
}


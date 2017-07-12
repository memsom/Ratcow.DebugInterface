using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ratcow.Debugging.Server.Tests
{
    [TestClass]
    public class MainUnitTest
    {
        string hello = "hello";
        string bye = "bye";
        string cool = "cool";
        int ten = 10;
        int fifteen = 15;
        int twentyfive = 25;


        [TestMethod]
        public void BasicTest_ValueTypes()
        {
            var i = ten;
            var s = hello;

            var dbs = new DebugInterface();

            //this seems horrible
            dbs.RegisterValue(new Ref<int>(() => i, (v) => { i = v; }), "i");
            dbs.RegisterValue(new Ref<string>(() => s, (v) => { s = v; }), "s");

            var vn = dbs.GetVariableNames();
            Assert.IsTrue(vn.Length > 0);
            Assert.IsTrue(vn.Length == 2);
            Assert.AreEqual(vn[0], "i");
            Assert.AreEqual(vn[1], "s");

            var si = dbs.GetVariableValue("i");
            var ss = dbs.GetVariableValue("s");
            Assert.AreEqual(si, ten.ToString());
            Assert.AreEqual(ss, $"\"{hello}\"");

            i = twentyfive;
            s = bye;

            si = dbs.GetVariableValue("i");
            ss = dbs.GetVariableValue("s");

            Assert.AreEqual(si, twentyfive.ToString());
            Assert.AreEqual(ss, $"\"{bye}\"");

            TestSubRoutine(dbs);

            si = dbs.GetVariableValue("i");
            ss = dbs.GetVariableValue("s");

            Assert.AreEqual(si, fifteen.ToString());
            Assert.AreEqual(ss, $"\"{cool}\"");
            Assert.AreEqual(i, fifteen);
            Assert.AreEqual(s, cool);
        }

        void TestSubRoutine(DebugInterface dbs)
        {
            var si = dbs.GetVariableValue("i");
            var ss = dbs.GetVariableValue("s");

            Assert.AreEqual(si, twentyfive.ToString());
            Assert.AreEqual(ss, $"\"{bye}\"");

            //slight hack
            ((Ref<int>)dbs.RegisteredVariables[0].Reference).Value = fifteen;
            ((Ref<string>)dbs.RegisteredVariables[1].Reference).Value = cool;

            si = dbs.GetVariableValue("i");
            ss = dbs.GetVariableValue("s");

            Assert.AreEqual(si, fifteen.ToString());
            Assert.AreEqual(ss, $"\"{cool}\"");
        }

        [TestMethod]
        public void BasicTest_Classes()
        {
            var c = new TestClass
            {
                I = ten,
                S = hello
            };

            var dbs = new DebugInterface();

            //this seems horrible
            dbs.RegisterProperties(c, "c", "I", "S");

            var vn = dbs.GetVariableNames();
            Assert.IsTrue(vn.Length > 0);
            Assert.IsTrue(vn.Length == 2);
            Assert.AreEqual(vn[0], "c.I");
            Assert.AreEqual(vn[1], "c.S");

            var si = dbs.GetVariableValue("c.I");
            var ss = dbs.GetVariableValue("c.S");
            Assert.AreEqual(si, ten.ToString());
            Assert.AreEqual(ss, $"\"{hello}\"");

            c.I = twentyfive;
            c.S = bye;

            si = dbs.GetVariableValue("c.I");
            ss = dbs.GetVariableValue("c.S");

            Assert.AreEqual(si, twentyfive.ToString());
            Assert.AreEqual(ss, $"\"{bye}\"");

            TestSubRoutine2(dbs, c);

            si = dbs.GetVariableValue("c.I");
            ss = dbs.GetVariableValue("c.S");

            Assert.AreEqual(si, fifteen.ToString());
            Assert.AreEqual(ss, $"\"{cool}\"");
            Assert.AreEqual(c.I, fifteen);
            Assert.AreEqual(c.S, cool);
        }

        void TestSubRoutine2(DebugInterface dbs, TestClass c)
        {
            var si = dbs.GetVariableValue("c.I");
            var ss = dbs.GetVariableValue("c.S");

            Assert.AreEqual(si, twentyfive.ToString());
            Assert.AreEqual(ss, $"\"{bye}\"");

            //this is slightly less useful, but I wanted to keep the tests symetrical for now
            c.I = fifteen;
            c.S = cool;

            si = dbs.GetVariableValue("c.I");
            ss = dbs.GetVariableValue("c.S");

            Assert.AreEqual(si, fifteen.ToString());
            Assert.AreEqual(ss, $"\"{cool}\"");
        }

        [TestMethod]
        public void BasicTest_Classes_Static()
        {
            TestClass.SI = ten;
            TestClass.SS = hello;

            var dbs = new DebugInterface();

            //this seems horrible
            dbs.RegisterProperties(typeof(TestClass), "c", "SI", "SS");

            var vn = dbs.GetVariableNames();
            Assert.IsTrue(vn.Length > 0);
            Assert.IsTrue(vn.Length == 2);
            Assert.AreEqual(vn[0], "c.SI");
            Assert.AreEqual(vn[1], "c.SS");

            var si = dbs.GetVariableValue("c.SI");
            var ss = dbs.GetVariableValue("c.SS");
            Assert.AreEqual(si, ten.ToString());
            Assert.AreEqual(ss, $"\"{hello}\"");

            TestClass.SI = twentyfive;
            TestClass.SS = bye;

            si = dbs.GetVariableValue("c.SI");
            ss = dbs.GetVariableValue("c.SS");
            Assert.AreEqual(si, twentyfive.ToString());
            Assert.AreEqual(ss, $"\"{bye}\"");
        }


        [TestMethod]
        public void BasicTest_Classes_StartupAction()
        {
            var c = new TestClass
            {
                I = ten,
                S = hello
            };

            DebugInterface.StartupAction += (d) =>
            {
                d.RegisterProperties(c, "c", "I", "S");
            };

            var dbs = new DebugInterface();

            var vn = dbs.GetVariableNames();
            Assert.IsTrue(vn.Length > 0);
            Assert.IsTrue(vn.Length == 2);
            Assert.AreEqual(vn[0], "c.I");
            Assert.AreEqual(vn[1], "c.S");

            var si = dbs.GetVariableValue("c.I");
            var ss = dbs.GetVariableValue("c.S");
            Assert.AreEqual(si, ten.ToString());
            Assert.AreEqual(ss, $"\"{hello}\"");

            c.I = twentyfive;
            c.S = bye;

            si = dbs.GetVariableValue("c.I");
            ss = dbs.GetVariableValue("c.S");

            Assert.AreEqual(si, twentyfive.ToString());
            Assert.AreEqual(ss, $"\"{bye}\"");

            TestSubRoutine2(dbs, c);

            si = dbs.GetVariableValue("c.I");
            ss = dbs.GetVariableValue("c.S");

            Assert.AreEqual(si, fifteen.ToString());
            Assert.AreEqual(ss, $"\"{cool}\"");
            Assert.AreEqual(c.I, fifteen);
            Assert.AreEqual(c.S, cool);
        }

        [TestMethod]
        public void BasicTest_Classes_StartupAction2()
        {
            var c = new TestClass
            {
                I = ten,
                S = hello
            };

            Action<DebugInterface> startupAction = (d) =>
            {
                d.RegisterProperties(c, "c", "I", "S");
            };

            var dbs = new DebugInterface(startupAction);

            var vn = dbs.GetVariableNames();
            Assert.IsTrue(vn.Length > 0);
            Assert.IsTrue(vn.Length == 2);
            Assert.AreEqual(vn[0], "c.I");
            Assert.AreEqual(vn[1], "c.S");

            var si = dbs.GetVariableValue("c.I");
            var ss = dbs.GetVariableValue("c.S");
            Assert.AreEqual(si, ten.ToString());
            Assert.AreEqual(ss, $"\"{hello}\"");

            c.I = twentyfive;
            c.S = bye;

            si = dbs.GetVariableValue("c.I");
            ss = dbs.GetVariableValue("c.S");

            Assert.AreEqual(si, twentyfive.ToString());
            Assert.AreEqual(ss, $"\"{bye}\"");

            TestSubRoutine2(dbs, c);

            si = dbs.GetVariableValue("c.I");
            ss = dbs.GetVariableValue("c.S");

            Assert.AreEqual(si, fifteen.ToString());
            Assert.AreEqual(ss, $"\"{cool}\"");
            Assert.AreEqual(c.I, fifteen);
            Assert.AreEqual(c.S, cool);
        }


        [TestMethod]
        public void BasicTest_Classes_store()
        {
            var c = new TestClassOuter
            {
                Value = new TestClass
                {
                    I = ten,
                    S = hello
                }
            };

            var dbs = new DebugInterface();

            //this seems horrible
            dbs.RegisterProperties(c, "c", "Value");

            var vn = dbs.GetVariableNames();
            Assert.IsTrue(vn.Length > 0);
            Assert.IsTrue(vn.Length == 1);
            Assert.AreEqual(vn[0], "c.Value");

            var sc = dbs.GetVariableValue("c.Value");

            Assert.IsNotNull(sc);
            Assert.IsTrue(sc.Length > 0);
        }


        [TestMethod]
        public void BasicTest_Classes_SetValue()
        {
            var c = new TestClass
            {
                I = ten,
                S = hello
            };

            var dbs = new DebugInterface();

            //this seems horrible
            dbs.RegisterProperties(c, "c", "I", "S");

            var vn = dbs.GetVariableNames();
            Assert.IsTrue(vn.Length > 0);
            Assert.IsTrue(vn.Length == 2);
            Assert.AreEqual(vn[0], "c.I");
            Assert.AreEqual(vn[1], "c.S");

            var si = dbs.GetVariableValue("c.I");
            var ss = dbs.GetVariableValue("c.S");
            Assert.AreEqual(si, ten.ToString());
            Assert.AreEqual(ss, $"\"{hello}\"");

            //c.I = twentyfive;
            //c.S = bye;

            Assert.IsTrue(dbs.SetVariableValue("c.I", twentyfive.ToString()));
            Assert.IsTrue(dbs.SetVariableValue("c.S", $"\"{bye}\""));

            si = dbs.GetVariableValue("c.I");
            ss = dbs.GetVariableValue("c.S");

            Assert.AreEqual(si, twentyfive.ToString());
            Assert.AreEqual(ss, $"\"{bye}\"");
        }
    }
}

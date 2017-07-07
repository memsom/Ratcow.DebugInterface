using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ratcow.Debugging.Server.Tests
{

    public class TestClassOuter
    {
        public TestClassOuter()
        {
            Value = new TestClass();
        }
        public TestClass Value{get; set;}
    }
    public class TestClass
    {
        public string S { get; set; }
        public int I { get; set; }

        public static string SS { get; set; }
        public static int SI { get; set; }
    }
}

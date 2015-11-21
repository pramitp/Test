using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    [Serializable]
    public class Child1
    {
        private int? _privInt = 8;
        private float _privFloat = 10.0f;
       
        private int myVar;

        public int propDouble
        {
            get { return myVar; }
            private set { myVar = value; }
        }
        
	
        //public double propDouble { get { return 0.1f; } }
        public Child2 child2 = new Child2();
        IDictionary<string, Child2> _dict;
        public Child1()
        {
            //propDouble = 12.6543;
            var dict = new Dictionary<string, Child2>();
            dict.Add("A", new Child2());
            dict.Add("B", new Child2());
            dict.Add("C", new Child2());
            dict.Add("D", new Child2());
            _dict = dict;
        }

    }

    [Serializable]
    public class ComplexClass
    {
        public Lazy<int> lazyInt = new Lazy<int>();
        public Colors color = Colors.Blue;
        public MyStruct myStruct;
        private int _privInt = 10;
        private float _privFloat = 12.0f;
        public double propDouble { get { return 0.5f; } }
        private string _privStr = "Test str";
        public Child3 child3 = new Child3();
        public Child1 child1 = new Child1();
        public List<int> intList = new List<int>();

    }

    [Serializable]
    public class Child2
    {
        private int _privInt = 6;
        private float _privFloat = 8.0f;
        public double propDouble { get {return 0.765;} }
        public ComplexClass SecondObj = null;
        public Child2()
        {
            //propDouble = 10.6543;
        }
    }

    public abstract class AbClass
    {
        private int _abInt = 10;
        public float _abFloat = 0.98f;

    }

    [Serializable]
    public class Child3 : AbClass
    {
        private int _privInt = 4;
        private float _privFloat = 6.0f;
        public double propDouble { get; set; }
        public Child3()
            : this(8.6543)
        {

        }

        public Child3(double d)
        {
            propDouble = d;
        }

        public static void DoSomething()
        {
        }
    }

    public enum Colors
    {
        Violet,
        Indigo,
        Blue,
        Green,
        Yellow,
        Orange,
        Red
    }
    public struct MyStruct
    {
        public double pi;
        
    }
}

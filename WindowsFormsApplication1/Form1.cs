using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ICollection<int>
            Assembly assembly = Assembly.Load("mscorlib.dll");
            var a = assembly.GetTypes();

            foreach (var item in a)
            {
                var i = item.GetInterfaces().Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (i.Count() > 0)
                {
                    float x = 8;
                }
            }
            foreach (var type in a)
            {
                Console.WriteLine(type.Name);
            }
            Console.ReadLine();

            ComplexClass complexObj = new ComplexClass();
            complexObj.intList.Add(10);
            complexObj.intList.Add(20);
            complexObj.intList.Add(30);
            complexObj.myStruct.pi = 3.14;
            complexObj.color = Colors.Red;

            using(MemoryStream ms = new MemoryStream())
            {
                complexObj.Serialize(ms);
                ms.Position = 0;
                ComplexClass obj = ms.DeSerialize<ComplexClass>();
            }
            
        }
    }
}

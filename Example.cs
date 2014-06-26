using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PollingTimer
{
    class Program
    {
        static void Main(string[] args)
        {
            int counter = 0;

            A a = new A() { Value = "Empty" };
            B b = new B() { Value = "Empty" };

            PollingTimer timer = new PollingTimer(
                data =>
                {
                    Console.WriteLine("Begin:");

                    data.Add("AA", new A { Value = "AA" });

                    data["A"] = (data["A"] as A).Value + " A";
                },
                data =>
                {
                    Console.WriteLine(string.Format("{0} Begin: ----------------------------", counter + 1));

                    foreach (var item in data)
                    {
                        Console.WriteLine(string.Format("key:{0}, value:{1}", item.Key, item.Value));
                    }

                    Console.WriteLine(string.Format("{0} End: ----------------------------", counter + 1));
                    Console.WriteLine("");

                    Thread.Sleep(10000);

                    if(counter == 9)
                    {
                        return true;
                    }

                    counter++;

                    return false;
                },
                data =>
                {
                    Console.WriteLine("END!");
                },
                new Dictionary<string, object> { { "A", a }, { "B", b } }
            );

            timer.Start();

            Console.Read();
        }
    }

    public class A
    {
        public string Value
        {
            get;
            set;
        }
    }

    public class B
    {
        public string Value
        {
            get;
            set;
        }
    }
}

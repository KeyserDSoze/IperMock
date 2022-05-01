using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IperMock
{
    internal class Coloquio
    {
        public string A { get; set; }
        public bool SetRoad(int a, double b, string c)
        {
            if (a == 0)
                if (b == 0)
                    if (c == "a")
                        return true;
            return false;
        }
        public (bool A, string C) Soal3(int x, string c, double u)
        {
            return default;
        }
    }
    internal class Sample
    {
        public string GetSomething(string a, int b, double c, Coloquio d)
        {
            var x = d.SetRoad(b, c, a);
            return x ? "a" : "d";
        }
        public bool GetSomething2(string a, int b, double c, Coloquio d)
        {
            var x = d.SetRoad(b, c, a);
            return x;
        }
    }
}

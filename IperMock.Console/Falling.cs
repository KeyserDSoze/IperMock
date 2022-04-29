using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IperMock.Console
{
    public class Falling
    {
        private Dictionary<string, Falling> falling = new();
        public Falling this[string a, string b]
        {
            get { return new(); }
            set { falling.Add(a + b, new()); }
        }
        public string A { get; set; }
        public string B { get; set; }
        public Selius Selius { get; set; }
        public Mari Mari { get; set; }
    }
    public class Fano
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public Fano(string name, string surname)
        {
            Name = name;
            Surname = surname;
        }
    }
    public abstract class Foligno
    {
        private readonly string Pablo;
        public string Cocco { get; set; }
        public Foligno(string pablo)
        {
            Pablo = pablo;
        }
    }
    public class Mari
    {
        public string F { get; set; }
    }
    public interface IFulvius
    {
        double C { get; set; }
    }
    public class Selius
    {
        public int X { get; set; }
        public Fai Fai { get; set; }
        public IFulvius Fulvius { get; set; }
        public Foligno Foligno { get; set; }
    }
    public class Fai
    {
        public int E { get; set; }
        public Fano Fano { get; set; }
    }
}

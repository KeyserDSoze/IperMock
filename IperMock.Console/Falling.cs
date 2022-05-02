using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IperMock.Console
{
    public class Melina
    {
        private readonly Dictionary<string, string> Dictionary;
        public Melina()
        {
            Dictionary = new Dictionary<string, string>();
        }
    }
    public class Felish : IHeaderDictionary
    {
        private readonly Dictionary<string, StringValues> _values;
        public StringValues this[string key] { get => _values[key]; set => _values.Add(key, value); }

        public long? ContentLength { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICollection<string> Keys => throw new NotImplementedException();

        public ICollection<StringValues> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(string key, StringValues value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<string, StringValues> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, StringValues> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, StringValues> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out StringValues value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    public sealed class Fmoa : Fudlish
    {
        public override string B { get; }
        protected override string C { get; set; }
    }
    public abstract class Fudlish
    {
        public string A { get; }
        public abstract string B { get; }
        protected abstract string C { get; set; }
    }
    public interface ISoldi
    {
        string A { get; }
    }

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

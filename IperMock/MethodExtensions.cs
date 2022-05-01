using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IperMock
{
    internal static class MethodExtensions
    {
        public static string ToSignature(this MethodInfo methodInfo) 
            => $"{methodInfo.Name}_{methodInfo.ReturnType.Name}_{string.Join(',', methodInfo.GetParameters().Select(x => x.Name))}";
    }
}

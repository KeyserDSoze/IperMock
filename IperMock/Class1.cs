using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace IperMock
{
    internal class Class1
    {
        public void Add()
        {
            string myClass = "MyClass";
            string reservedIndexerName = "Item";

            // Create an assembly and the class type
            AssemblyName assemblyName = new AssemblyName(myClass + "Assembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            var typeBuilder = moduleBuilder.DefineType(myClass, TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit);

            // create the indexer
            var ciDefaultMemberAttribute = typeof(DefaultMemberAttribute).GetConstructor(new Type[] { typeof(string) });
            var abDefaultMemberAttribute = new CustomAttributeBuilder(ciDefaultMemberAttribute, new object[] { reservedIndexerName });
            typeBuilder.SetCustomAttribute(abDefaultMemberAttribute);
            var indexerProperty = typeBuilder.DefineProperty(reservedIndexerName, PropertyAttributes.None, CallingConventions.ExplicitThis | CallingConventions.HasThis, typeof(string), new Type[] { typeof(int) });
            var indexerDictionary = typeof(Dictionary<int, string>);
            var indexerDictionaryFieldBuilder = typeBuilder.DefineField("indexerDictionary", indexerDictionary, FieldAttributes.Private);

            // create the getter method
            var indexerGetter = typeBuilder.DefineMethod("get_" + reservedIndexerName, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, typeof(string), new Type[] { typeof(int) });
            var custNameGetIL = indexerGetter.GetILGenerator();
            custNameGetIL.DeclareLocal(typeof(string));
            custNameGetIL.Emit(OpCodes.Ldarg_0);
            custNameGetIL.Emit(OpCodes.Ldfld, indexerDictionaryFieldBuilder);
            custNameGetIL.Emit(OpCodes.Ldarg_1);
            custNameGetIL.Emit(OpCodes.Callvirt, indexerDictionary.GetMethod("get_" + reservedIndexerName));
            custNameGetIL.Emit(OpCodes.Ret);
            indexerProperty.SetGetMethod(indexerGetter);

            // create the setter method
            var indexerSetter = typeBuilder.DefineMethod("set_" + reservedIndexerName, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, null, new Type[] { typeof(int), typeof(string) });
            var custNameSetIL = indexerSetter.GetILGenerator();
            custNameSetIL.Emit(OpCodes.Ldarg_0);
            custNameSetIL.Emit(OpCodes.Ldfld, indexerDictionaryFieldBuilder);
            custNameSetIL.Emit(OpCodes.Ldarg_1);
            custNameSetIL.Emit(OpCodes.Ldarg_2);
            custNameSetIL.Emit(OpCodes.Callvirt, indexerDictionary.GetMethod("set_" + reservedIndexerName));
            custNameSetIL.Emit(OpCodes.Ret);
            indexerProperty.SetSetMethod(indexerSetter);

            // create the indexer dictionary in the constructor
            var cibuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var ctorIL = cibuilder.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Newobj, indexerDictionary.GetConstructor(Type.EmptyTypes));
            ctorIL.Emit(OpCodes.Stfld, indexerDictionaryFieldBuilder);
            ctorIL.Emit(OpCodes.Ret);

            // instantiate the type
            dynamic instance = Activator.CreateInstance(typeBuilder.CreateType());

            // usage
            instance[1] = "Michael Jackson";
        }
    }
}

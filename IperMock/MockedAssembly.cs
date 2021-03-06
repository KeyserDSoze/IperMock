using System.Reflection;
using System.Reflection.Emit;

namespace IperMock
{
    internal record MockedType(Type Type);
    internal class DecoratedMock
    {
        public static object? InvokeMethod(object entity, string methodName, params object[] parameters)
        {
            Type type = entity.GetType();
            var method = type.GetMethod(methodName);
            if (method == null)
                return default;
            var result = method.Invoke(entity, parameters);
            if (result == null)
                return default;
            return result;
        }
    }
    internal class MockedAssembly
    {
        public static MockedAssembly Instance { get; } = new();
        public ModuleBuilder Builder { get; }
        private MockedAssembly()
        {
            var assemblyName = new AssemblyName($"Mock{Guid.NewGuid()}");
            assemblyName.SetPublicKey(Assembly.GetExecutingAssembly().GetName().GetPublicKey());
            var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run); ;
            Builder = assembly.DefineDynamicModule(assemblyName.Name!);
        }
        private static readonly Dictionary<Type, MockedType> Types = new();
        public Type? GetTypeByName(Type baseType)
        {
            if (Types.ContainsKey(baseType))
                return Types[baseType].Type;
            else
                return null;
        }
        public static string GetPrivateFieldForPropertyName(string propertyName)
            => $"<{propertyName}>k__BackingField";
        public Type DefineNewImplementation(Type type)
        {
            string name = $"{type.Name}{string.Join('_', type.GetGenericArguments().Select(x => x.Name))}Concretization";
            var createdNames = new Dictionary<string, bool>();
            var typeBuilder = Builder.DefineType(name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);
            if (!type.IsInterface)
                typeBuilder.SetParent(type);
            if (type.IsInterface)
                typeBuilder.AddInterfaceImplementation(type);
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var constructorGenerator = constructorBuilder.GetILGenerator();
            ConfigureProperties(type, typeBuilder, constructorGenerator, createdNames);
            ConfigureMethods(type, typeBuilder, createdNames);
            constructorGenerator.Emit(OpCodes.Ret);
            var createdType = typeBuilder!.CreateType();
            Types.Add(type, new MockedType(createdType!));
            return createdType!;
        }
        private void ConfigureProperties(Type currentType, TypeBuilder typeBuilder, ILGenerator constructorGenerator, Dictionary<string, bool> createdNames)
        {
            foreach (var property in currentType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => !createdNames.ContainsKey(x.Name) && (currentType.IsInterface || x.GetMethod!.IsAbstract)))
            {
                ConfigureProperty(property, typeBuilder, constructorGenerator, createdNames);
            }
            foreach (var subType in currentType.GetInterfaces())
            {
                ConfigureProperties(subType, typeBuilder, constructorGenerator, createdNames);
            }
            if (currentType.BaseType != null && currentType.BaseType != typeof(object))
                ConfigureProperties(currentType.BaseType, typeBuilder, constructorGenerator, createdNames);
        }
        private static Func<object, object> Get(int whatKindOfThingINeedToDo, Type type, string propertyName)
        {
            return type.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue!;
        }
        private static object GetFromDictionary(object dictionary, object key)
        {
            return new();
        }
        private MethodBuilder CreateMethod(MethodInfo methodInfo, TypeBuilder typeBuilder, Dictionary<string, bool> createdNames, Action<ILGenerator> action = null, bool returnDefault = false)
        {
#warning non funziona sicuramente il metodo generics, da testare
#warning da aggiungere che creo anche metodi protected e private protected, togliendo l'override
            string signature = methodInfo.ToSignature();
            if (!createdNames.ContainsKey(signature))
            {
                createdNames.Add(signature, true);
                var parameters = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
                MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    methodInfo.ReturnType, parameters);
                var methodGenerator = methodBuilder.GetILGenerator();
                if (action != null)
                {
                    action.Invoke(methodGenerator);
                }
                else if (returnDefault)
                {
                    if (methodInfo.ReturnType != null && methodInfo.ReturnType != typeof(void))
                    {
                        if (Primitive.Check(methodInfo.ReturnType))
                            methodGenerator.Emit(OpCodes.Ldc_I4_0);
                        else
                            methodGenerator.Emit(OpCodes.Ldnull);
                    }
                }
                else
                {
                    methodGenerator.Emit(OpCodes.Ldstr, methodInfo.Name);
                    methodGenerator.Emit(OpCodes.Stloc_0);
                    methodGenerator.Emit(OpCodes.Ldarg_0);
                    methodGenerator.Emit(OpCodes.Ldloc_0);
                    methodGenerator.Emit(OpCodes.Ldc_I4, parameters.Length);
                    methodGenerator.Emit(OpCodes.Newarr, typeof(object));
                    methodGenerator.Emit(OpCodes.Stloc_1);
                    methodGenerator.Emit(OpCodes.Ldloc_1);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        methodGenerator.Emit(OpCodes.Ldc_I4, i);
                        methodGenerator.Emit(OpCodes.Ldarg, i + 1);
                        if (parameters[i] != typeof(string) && Primitive.Check(parameters[i]))
                            methodGenerator.Emit(OpCodes.Box, parameters[i]);
                        methodGenerator.Emit(OpCodes.Stelem_Ref);
                    }
                    methodGenerator.Emit(OpCodes.Ldloc_1);
                    methodGenerator.Emit(OpCodes.Call, typeof(DecoratedMock).GetMethod("InvokeMethod")!);
                }
                methodGenerator.Emit(OpCodes.Ret);
                return methodBuilder;
            }
            return default!;
        }
        private void ConfigureProperty(PropertyInfo property, TypeBuilder typeBuilder, ILGenerator constructorGenerator, Dictionary<string, bool> createdNames)
        {
            string privateFieldName = GetPrivateFieldForPropertyName(property.Name);

            if (!createdNames.ContainsKey(privateFieldName))
            {
                if (property.GetMethod == null)
                    return;
                var getParameters = property.GetMethod!.GetParameters().Select(x => x.ParameterType).ToArray();

                var isIndexer = getParameters.Length > 0;
                Type propertyType = !isIndexer ? property.PropertyType : typeof(Dictionary<,>).MakeGenericType(typeof(string), property.PropertyType);

                var privateFieldBuilder = typeBuilder.DefineField(privateFieldName, propertyType, FieldAttributes.Private);
                createdNames.Add(privateFieldName, true);

                if (isIndexer)
                {
                    constructorGenerator.Emit(OpCodes.Ldarg_0);
                    constructorGenerator.Emit(OpCodes.Newobj, propertyType.GetConstructor(Type.EmptyTypes)!);
                    constructorGenerator.Emit(OpCodes.Stfld, privateFieldBuilder);
                }

                var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);
                MethodBuilder getMethodBuilder = CreateMethod(property.GetMethod, typeBuilder, createdNames, (generator) =>
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldfld, privateFieldBuilder);
                    if (isIndexer)
                    {
                        for (int i = 0; i < getParameters.Length; i++)
                            generator.Emit(OpCodes.Ldarg_S, i + 1);
                        generator.Emit(OpCodes.Callvirt, typeof(MockedAssembly).GetMethod(nameof(GetFromDictionary), BindingFlags.NonPublic | BindingFlags.Static)!);
                    }
                });
                propertyBuilder.SetGetMethod(getMethodBuilder);

                if (property.SetMethod != null)
                {
                    var setParameters = property.SetMethod.GetParameters().Select(x => x.ParameterType).ToArray();
                    MethodBuilder setMethodBuilder = CreateMethod(property.SetMethod, typeBuilder, createdNames,
                        (generator) =>
                            {
                                generator.Emit(OpCodes.Ldarg_0);
                                generator.Emit(OpCodes.Ldarg_1);
                                if (isIndexer)
                                {
                                    for (int i = 0; i < setParameters.Length; i++)
                                        generator.Emit(OpCodes.Ldarg_S, i + 2);
                                    generator.Emit(OpCodes.Callvirt, privateFieldBuilder.FieldType.GetMethod("Add")!);
                                }
                                else
                                    generator.Emit(OpCodes.Stfld, privateFieldBuilder);
                            });
                    propertyBuilder.SetSetMethod(setMethodBuilder);
                }

            }
        }
        public static void OverrideMethod(object entity, string methodName, object action)
        {
            Type type = entity.GetType();
            Assembly assembly = type.Assembly;
            var what = action.GetType();
            //DynamicMethod dynamicMethod = new DynamicMethod(methodName, );
        }
        private void ConfigureMethods(Type currentType, TypeBuilder typeBuilder, Dictionary<string, bool> createdNames)
        {
            foreach (var method in currentType.GetMethods()
                .Where(x => x.IsAbstract))
            {
                _ = CreateMethod(method, typeBuilder, createdNames, null, true);
            }
            foreach (var subType in currentType.GetInterfaces())
            {
                ConfigureMethods(subType, typeBuilder, createdNames);
            }
            if (currentType.BaseType != null && currentType.BaseType != typeof(object))
                ConfigureMethods(currentType.BaseType, typeBuilder, createdNames);
        }
    }
}
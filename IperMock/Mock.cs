using Rystem.Reflection;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace IperMock
{
#warning problemi noti - ci sono più metodi simili, non funziona con le classi sealed
    public sealed class Mock<T>
    {
        public T Instance { get; }
        private static readonly ModuleBuilder Builder;
        static Mock()
        {
            var assemblyName = new AssemblyName($"Mock{Guid.NewGuid()}");
            assemblyName.SetPublicKey(Assembly.GetExecutingAssembly().GetName().GetPublicKey());
            var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run); ;
            //var assembly = AppDomain.CurrentDomain.ExecuteAssemblyByName(assemblyName)
            Builder = assembly.DefineDynamicModule(assemblyName.Name);
        }
        public Mock()
        {
            Instance = (T)Create<T>();
        }
        private static TEntity Create<TEntity>()
            => (TEntity)Create(typeof(TEntity));
        private static dynamic Create(Type type)
        {
            if (!type.IsArray)
            {
                string name = $"{type.Name}Concretization";
                var buildedType = Builder.GetType(name);
                if (buildedType == null)
                {
                    Dictionary<string, bool> createdNames = new();
                    var typeBuilder = Builder.DefineType($"{type.Name}Concretization", TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);
                    if (!type.IsInterface)
                        typeBuilder.SetParent(type);
                    if (type.IsInterface)
                        typeBuilder.AddInterfaceImplementation(type);
                    var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
                    var generator = constructorBuilder.GetILGenerator();

                    ConfigureAllProperties(type);

                    void ConfigureAllProperties(Type currentType)
                    {
                        foreach (var property in currentType.GetProperties().Where(x => !createdNames.ContainsKey(x.Name)))
                        {
                            ConfigureProperty(property);
                        }
                        foreach (var subType in currentType.GetInterfaces())
                        {
                            ConfigureAllProperties(subType);
                        }
                        if (currentType.BaseType != null && currentType.BaseType != typeof(object))
                            ConfigureAllProperties(currentType.BaseType);
                    }

                    void ConfigureProperty(PropertyInfo property)
                    {
                        string privateFieldName = $"_{property.Name}";
                        string getName = $"get_{property.Name}";
                        string setName = $"set_{property.Name}";

                        if (!createdNames.ContainsKey(privateFieldName))
                        {
                            var getParameters = property.GetMethod!.GetParameters();
                            if (getParameters.Length > 0)
                            {
                                var genericDictionary = typeof(Dictionary<,>).MakeGenericType(getParameters.First().ParameterType, property.PropertyType);
                                var fieldBuilder = typeBuilder.DefineField(privateFieldName, genericDictionary, FieldAttributes.Private);
                                generator.Emit(OpCodes.Newobj, genericDictionary.GetConstructor(Type.EmptyTypes)!);
                                generator.Emit(OpCodes.Stfld, fieldBuilder);

                                var parameterArray = getParameters.Select(x => x.ParameterType).ToArray();

                                MethodBuilder getMethodBuilder = typeBuilder.DefineMethod(getName,
                                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                                    property.GetMethod.ReturnType, parameterArray);
                                var getPropertyGenerator = getMethodBuilder.GetILGenerator();
                                var getLabel = getPropertyGenerator.DefineLabel();
                                var exitGetLabel = getPropertyGenerator.DefineLabel();

                                getPropertyGenerator.MarkLabel(getLabel);
                                getPropertyGenerator.DeclareLocal(property.PropertyType);
                                for (int i = 0; i < getParameters.Length; i++)
                                {
                                    getPropertyGenerator.Emit(OpCodes.Ldarg_S, i);
                                }
                                getPropertyGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                                getPropertyGenerator.Emit(OpCodes.Callvirt, genericDictionary.GetMethod(getName)!);
                                getPropertyGenerator.MarkLabel(exitGetLabel);
                                getPropertyGenerator.Emit(OpCodes.Ret);

                                MethodBuilder setMethodBuilder = typeBuilder.DefineMethod(setName,
                                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                                    property.SetMethod!.ReturnType, parameterArray.Concat(new Type[1] { property.PropertyType }).ToArray());

                                var setPropertyGenerator = setMethodBuilder.GetILGenerator();
                                var setLabel = setPropertyGenerator.DefineLabel();
                                var exitSetLabel = setPropertyGenerator.DefineLabel();

                                setPropertyGenerator.MarkLabel(setLabel);
                                for (int i = 0; i < getParameters.Length; i++)
                                {
                                    setPropertyGenerator.Emit(OpCodes.Ldarg_S, i);
                                }
                                setPropertyGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                                setPropertyGenerator.Emit(OpCodes.Ldarg_S, getParameters.Length);
                                setPropertyGenerator.Emit(OpCodes.Callvirt, genericDictionary.GetMethod(setName)!);
                                setPropertyGenerator.MarkLabel(exitSetLabel);
                                setPropertyGenerator.Emit(OpCodes.Ret);

                                var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);
                                propertyBuilder.SetGetMethod(getMethodBuilder);
                                propertyBuilder.SetSetMethod(setMethodBuilder);

                                createdNames.Add(privateFieldName, true);
                                createdNames.Add(getName, true);
                                createdNames.Add(setName, true);
                            }
                            else
                            {
                                var fieldBuilder = typeBuilder.DefineField(privateFieldName, property.PropertyType, FieldAttributes.Private);
                                MethodBuilder getMethodBuilder = typeBuilder.DefineMethod(getName, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, property.PropertyType, Type.EmptyTypes);
                                var getPropertyGenerator = getMethodBuilder.GetILGenerator();
                                var getLabel = getPropertyGenerator.DefineLabel();
                                var exitGetLabel = getPropertyGenerator.DefineLabel();

                                getPropertyGenerator.MarkLabel(getLabel);
                                getPropertyGenerator.Emit(OpCodes.Ldarg_0);
                                getPropertyGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                                getPropertyGenerator.MarkLabel(exitGetLabel);
                                getPropertyGenerator.Emit(OpCodes.Ret);

                                MethodBuilder setMethodBuilder = typeBuilder.DefineMethod(setName, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[1] { property.PropertyType });
                                var setPropertyGenerator = setMethodBuilder.GetILGenerator();
                                var setLabel = setPropertyGenerator.DefineLabel();
                                var exitSetLabel = setPropertyGenerator.DefineLabel();

                                setPropertyGenerator.MarkLabel(setLabel);
                                setPropertyGenerator.Emit(OpCodes.Ldarg_0);
                                setPropertyGenerator.Emit(OpCodes.Ldarg_1);
                                setPropertyGenerator.Emit(OpCodes.Stfld, fieldBuilder);
                                setPropertyGenerator.Emit(OpCodes.Nop);
                                setPropertyGenerator.MarkLabel(exitSetLabel);
                                setPropertyGenerator.Emit(OpCodes.Ret);

                                var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.None, property.PropertyType, null);
                                propertyBuilder.SetGetMethod(getMethodBuilder);
                                propertyBuilder.SetSetMethod(setMethodBuilder);

                                createdNames.Add(privateFieldName, true);
                                createdNames.Add(getName, true);
                                createdNames.Add(setName, true);
                            }
                        }
                    }

                    ConfigureMethods(type);

                    void ConfigureMethods(Type currentType)
                    {
                        foreach (var method in currentType.GetMethods().Where(x => x.IsAbstract && !createdNames.ContainsKey(x.Name)))
                        {
                            if(method.Name == "Add")
                            {
                                string folash = "";
                            }
                            createdNames.Add(method.Name, true);
                            var methodBuilder = typeBuilder.DefineMethod(method.Name,
                                MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                                method.ReturnType, method.GetParameters().Select(x => x.ParameterType).ToArray());
                            var methodGenerator = methodBuilder.GetILGenerator();
                            methodGenerator.Emit(OpCodes.Ret);
                        }
                        foreach (var subType in currentType.GetInterfaces())
                        {
                            ConfigureMethods(subType);
                        }
                        if (currentType.BaseType != null && currentType.BaseType != typeof(object))
                            ConfigureMethods(currentType.BaseType);
                    }

                    generator.Emit(OpCodes.Ret);

                    return Activator.CreateInstance(typeBuilder!.CreateType()!)!;
                }
                else
                    return Activator.CreateInstance(buildedType!)!;
            }
            else if (type.IsArray)
            {
                return default;
            }
            else
            {
                var constructor = type.GetConstructors().OrderBy(x => x.GetParameters().Length).FirstOrDefault();
                if (constructor != null)
                {
                    return constructor.Invoke(constructor.GetParameters().Select<ParameterInfo, object>(x => default).ToArray());
                }
                else
                    return default;
            }
        }
        public static Mock<T> CreateWithDefaultValues()
        {
            return default;
        }
        private Mock<T> Execute<TProperty>(in Expression<Func<T, TProperty>> navigationPropertyPath, Action<PropertyInfo, object> action)
        {
            var navigationPath = navigationPropertyPath.ToString().Split('.').Skip(1).ToList();
            int counter = 0;
            object theActualObject = Instance!;
            var type = theActualObject.GetType();
            foreach (var navigation in navigationPath)
            {
                var nextType = type.FetchProperties().First(x => x.Name == navigation);
                if (!Primitive.CheckWithNull(nextType.PropertyType) && counter + 1 < navigationPath.Count)
                {
                    var entity = nextType.GetValue(theActualObject);
                    if (entity == null)
                    {
                        entity = Create(nextType.PropertyType);
                        SetValue(nextType, theActualObject, entity);
                    }
                    theActualObject = entity;
                    type = theActualObject.GetType();
                    counter++;
                }
                else
                {
                    action(nextType, theActualObject!);
                    return this;
                }
            }
            return this;
        }
        private static void SetValue(PropertyInfo nextType, object theActualObject, object value)
        {
            theActualObject.GetType().GetProperties()
                .First(x => x.Name == nextType.Name)
                    .SetValue(theActualObject, value);
        }
        public Mock<T> Construct<TProperty>(in Expression<Func<T, TProperty>> navigationPropertyPath, params object[] parameters)
            => Execute(navigationPropertyPath, (nextType, theActualObject) =>
                    Mock<T>.SetValue(nextType, theActualObject, nextType.PropertyType.FectConstructors()
                        .First(x => x.GetParameters().Length == (parameters?.Length ?? 0))
                        .Invoke(parameters)));
        public Mock<T> Default<TProperty>(in Expression<Func<T, TProperty>> navigationPropertyPath)
            => Execute(navigationPropertyPath, (nextType, theActualObject)
                => Mock<T>.SetValue(nextType, theActualObject, Create<TProperty>()!));
        public Mock<T> Set<TProperty>(in Expression<Func<T, TProperty>> navigationPropertyPath, TProperty value)
            => Execute(navigationPropertyPath, (nextType, theActualObject)
                => Mock<T>.SetValue(nextType, theActualObject, value!));
    }
}
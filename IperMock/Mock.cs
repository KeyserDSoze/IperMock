using Rystem.Reflection;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace IperMock
{
#warning problemi noti - ci sono più metodi simili, non funziona con le classi sealed
    public sealed class Mock<T>
    {
        public T Instance { get; }
        public Mock()
        {
            Instance = Create<T>();
        }
        private static TEntity Create<TEntity>()
            => (TEntity)Create(typeof(TEntity));
        private static dynamic Create(Type type)
        {
            if (!type.IsArray && !type.IsSealed)
            {
                var buildedType = MockedAssembly.Instance.GetTypeByName(type);
                if (buildedType == null)
                    return Activator.CreateInstance(MockedAssembly.Instance.DefineNewImplementation(type))!;
                else
                    return Activator.CreateInstance(buildedType)!;
            }
            else if (type.IsArray)
            {
                return Activator.CreateInstance(type)!;
            }
            else
            {
                var constructor = type.GetConstructors().OrderBy(x => x.GetParameters().Length).FirstOrDefault();
                if (constructor != null)
                    return constructor.Invoke(constructor.GetParameters().Select<ParameterInfo, object>(x => default).ToArray());
                else
                    return default!;
            }
        }
        public static Mock<T> CreateWithDefaultValues()
            => default;
        private static readonly PropertyInfo DebugView = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
        private Mock<T> Execute<TProperty>(in Expression<Func<T, TProperty>> navigationPropertyPath, Action<PropertyInfo, object> action)
        {
            var debugView = DebugView.GetValue(navigationPropertyPath).ToString();
            var x = debugView.Split('$').Last().Split('\r').First().Replace("(", string.Empty).Replace(")", string.Empty);
            var navigationPath = x.Split('.').Skip(1).ToList();
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
            var property = theActualObject.GetType().GetProperties().First(x => x.Name == nextType.Name);
            if (property.SetMethod != null)
                property.SetValue(theActualObject, value);
            else
            {
                FieldInfo? fieldInfo = null;
                var toCheck = theActualObject.GetType();
                while (fieldInfo == null)
                {
                    var fields = toCheck.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                    fieldInfo = fields.FirstOrDefault(x => x.Name == $"<{nextType.Name}>k__BackingField");
                    if (toCheck.BaseType != null && toCheck.BaseType != typeof(object))
                        toCheck = toCheck.BaseType;
                    else
                        break;
                }
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(theActualObject, value);
                }
            }
        }
        public Mock<T> Construct<TProperty>(in Expression<Func<T, TProperty>> navigationPath, params object[] parameters)
            => Execute(navigationPath, (nextType, theActualObject) =>
                    Mock<T>.SetValue(nextType, theActualObject, nextType.PropertyType.FectConstructors()
                        .First(x => x.GetParameters().Length == (parameters?.Length ?? 0))
                        .Invoke(parameters)));
        public Mock<T> Default<TProperty>(in Expression<Func<T, TProperty>> navigationPath)
            => Execute(navigationPath, (nextType, theActualObject)
                => Mock<T>.SetValue(nextType, theActualObject, Create<TProperty>()!));
        public Mock<T> Set<TProperty>(in Expression<Func<T, TProperty>> navigationPath, TProperty value)
            => Execute(navigationPath, (nextType, theActualObject)
                => Mock<T>.SetValue(nextType, theActualObject, value!));
        public Mock<T> Add<TProperty>(in Expression<Func<T, IEnumerable<TProperty>>> navigationPath, TProperty value)
            => Execute(navigationPath, (nextType, theActualObject)
                => Add((theActualObject as IEnumerable)!, value!));
        public Mock<T> Method<TFunction>(in Expression<Func<T, TFunction>> navigationPath, TFunction value)
            => Execute(navigationPath, (nextType, theActualObject) =>
            {
                MockedAssembly.OverrideMethod(theActualObject, string.Empty, value);
            });

        public static IEnumerable Add(IEnumerable elements, object value)
        {
            foreach (var element in elements)
                yield return element;
            yield return value;
        }
    }
}
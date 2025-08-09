using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace GraphProcessor
{
    /// <summary>
    /// 实现此接口以在类内部定义要在图形中使用的类型转换。
    /// 示例：
    /// <code>
    /// public class CustomConvertions : ITypeAdapter
    /// {
    ///     public static Vector4 ConvertFloatToVector(float from) => new Vector4(from, from, from, from);
    ///     ...
    /// }
    /// </code>
    /// </summary>
    public abstract class ITypeAdapter // TODO: 当我们有C# 8时将其改回接口
    {
        public virtual IEnumerable<(Type, Type)> GetIncompatibleTypes() { yield break; }
    }

    public static class TypeAdapter
    {
        static Dictionary< (Type from, Type to), Func<object, object> > adapters = new Dictionary< (Type, Type), Func<object, object> >();
        static Dictionary< (Type from, Type to), MethodInfo > adapterMethods = new Dictionary< (Type, Type), MethodInfo >();
        static List< (Type from, Type to)> incompatibleTypes = new List<( Type from, Type to) >();

        [System.NonSerialized]
        static bool adaptersLoaded = false;

#if !ENABLE_IL2CPP
        static Func<object, object> ConvertTypeMethodHelper<TParam, TReturn>(MethodInfo method)
        {
            // 将慢速的MethodInfo转换为快速、强类型的开放委托
            Func<TParam, TReturn> func = (Func<TParam, TReturn>)Delegate.CreateDelegate
                (typeof(Func<TParam, TReturn>), method);

            // 现在创建一个更弱类型的委托，它将调用强类型的委托
            Func<object, object> ret = (object param) => func((TParam)param);
            return ret;
        }
#endif

        static void LoadAllAdapters()
        {
            foreach (Type type in AppDomain.CurrentDomain.GetAllTypes())
            {
                if (typeof(ITypeAdapter).IsAssignableFrom(type))
                {
                    if (type.IsAbstract)
                        continue;
                    
                    var adapter = Activator.CreateInstance(type) as ITypeAdapter;
                    if (adapter != null)
                    {
                        foreach (var types in adapter.GetIncompatibleTypes())
                        {
                            incompatibleTypes.Add((types.Item1, types.Item2));
                            incompatibleTypes.Add((types.Item2, types.Item1));
                        }
                    }
                    
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (method.GetParameters().Length != 1)
                        {
                            Debug.LogError($"忽略转换方法 {method}，因为它没有恰好一个参数");
                            continue;
                        }
                        if (method.ReturnType == typeof(void))
                        {
                            Debug.LogError($"忽略转换方法 {method}，因为它没有返回任何内容");
                            continue;
                        }
                        Type from = method.GetParameters()[0].ParameterType;
                        Type to = method.ReturnType;

                        try {

#if ENABLE_IL2CPP
                            // IL2CPP不支持通过反射调用泛型函数（AOT无法生成模板代码）
                            Func<object, object> r = (object param) => { return (object)method.Invoke(null, new object[]{ param }); };
#else
                            MethodInfo genericHelper = typeof(TypeAdapter).GetMethod("ConvertTypeMethodHelper", 
                                BindingFlags.Static | BindingFlags.NonPublic);
                            
                            // 现在提供类型参数
                            MethodInfo constructedHelper = genericHelper.MakeGenericMethod(from, to);

                            object ret = constructedHelper.Invoke(null, new object[] {method});
                            var r = (Func<object, object>) ret;
#endif

                            adapters.Add((method.GetParameters()[0].ParameterType, method.ReturnType), r);
                            adapterMethods.Add((method.GetParameters()[0].ParameterType, method.ReturnType), method);
                        } catch (Exception e) {
                            Debug.LogError($"加载类型转换方法失败: {method}\n{e}");
                        }
                    }
                }
            }

            // 确保字典包含双向的所有转换
            // 例如：float到vector但没有vector到float
            foreach (var kp in adapters)
            {
                if (!adapters.ContainsKey((kp.Key.to, kp.Key.from)))
                    Debug.LogError($"缺少转换方法。有从 {kp.Key.from} 到 {kp.Key.to} 的转换，但没有从 {kp.Key.to} 到 {kp.Key.from} 的转换");
            }

            adaptersLoaded = true;
        }

        public static bool AreIncompatible(Type from, Type to)
        {
            if (incompatibleTypes.Any((k) => k.from == from && k.to == to))
                return true;
            return false;
        }

        public static bool AreAssignable(Type from, Type to)
        {
            if (!adaptersLoaded)
                LoadAllAdapters();
            
            if (AreIncompatible(from, to))
                return false;

            return adapters.ContainsKey((from, to));
        }

        public static MethodInfo GetConvertionMethod(Type from, Type to) => adapterMethods[(from, to)];

        public static object Convert(object from, Type targetType)
        {
            if (!adaptersLoaded)
                LoadAllAdapters();

            Func<object, object> convertionFunction;
            if (adapters.TryGetValue((from.GetType(), targetType), out convertionFunction))
                return convertionFunction?.Invoke(from);

            return null;
        }
    }
}
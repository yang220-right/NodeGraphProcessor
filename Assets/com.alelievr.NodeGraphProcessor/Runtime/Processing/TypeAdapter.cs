using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace GraphProcessor
{
    /// <summary>
    /// 类型适配器接口
    /// 实现此接口以在类内部定义要在图形中使用的类型转换。
    /// 通过静态方法定义类型转换规则，支持自定义类型间的转换逻辑
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
        /// <summary>
        /// 获取不兼容的类型对
        /// 返回明确声明为不兼容的类型组合，这些类型之间无法进行转换
        /// </summary>
        /// <returns>不兼容的类型对集合</returns>
        public virtual IEnumerable<(Type, Type)> GetIncompatibleTypes() { yield break; }
    }

    /// <summary>
    /// 类型适配器管理器
    /// 负责管理所有类型转换规则，提供类型转换功能
    /// 通过反射自动发现和注册所有ITypeAdapter实现类中的转换方法
    /// </summary>
    public static class TypeAdapter
    {
        /// <summary>
        /// 类型转换委托字典
        /// 存储从源类型到目标类型的转换委托，提供高性能的类型转换
        /// </summary>
        static Dictionary< (Type from, Type to), Func<object, object> > adapters = new Dictionary< (Type, Type), Func<object, object> >();
        
        /// <summary>
        /// 类型转换方法信息字典
        /// 存储从源类型到目标类型的转换方法信息，用于调试和反射
        /// </summary>
        static Dictionary< (Type from, Type to), MethodInfo > adapterMethods = new Dictionary< (Type, Type), MethodInfo >();
        
        /// <summary>
        /// 不兼容类型列表
        /// 存储明确声明为不兼容的类型对
        /// </summary>
        static List< (Type from, Type to)> incompatibleTypes = new List<( Type from, Type to) >();

        /// <summary>
        /// 适配器是否已加载标志
        /// 防止重复加载适配器
        /// </summary>
        [System.NonSerialized]
        static bool adaptersLoaded = false;

#if !ENABLE_IL2CPP
        /// <summary>
        /// 类型转换方法辅助函数
        /// 将慢速的MethodInfo转换为快速、强类型的开放委托
        /// 仅在非IL2CPP环境下使用，因为IL2CPP不支持动态委托创建
        /// </summary>
        /// <typeparam name="TParam">参数类型</typeparam>
        /// <typeparam name="TReturn">返回类型</typeparam>
        /// <param name="method">要转换的方法信息</param>
        /// <returns>类型转换委托</returns>
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

        /// <summary>
        /// 加载所有类型适配器
        /// 通过反射扫描所有ITypeAdapter实现类，注册转换方法和不兼容类型
        /// </summary>
        static void LoadAllAdapters()
        {
            // 遍历当前应用程序域中的所有类型
            foreach (Type type in AppDomain.CurrentDomain.GetAllTypes())
            {
                // 检查类型是否实现了ITypeAdapter接口
                if (typeof(ITypeAdapter).IsAssignableFrom(type))
                {
                    // 跳过抽象类
                    if (type.IsAbstract)
                        continue;
                    
                    // 创建适配器实例并获取不兼容类型
                    var adapter = Activator.CreateInstance(type) as ITypeAdapter;
                    if (adapter != null)
                    {
                        // 收集不兼容的类型对
                        foreach (var types in adapter.GetIncompatibleTypes())
                        {
                            incompatibleTypes.Add((types.Item1, types.Item2));
                            incompatibleTypes.Add((types.Item2, types.Item1));
                        }
                    }
                    
                    // 遍历类型中的所有静态方法
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        // 验证方法签名：必须只有一个参数且有返回值
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
                        
                        // 获取源类型和目标类型
                        Type from = method.GetParameters()[0].ParameterType;
                        Type to = method.ReturnType;

                        try {
#if ENABLE_IL2CPP
                            // IL2CPP不支持通过反射调用泛型函数（AOT无法生成模板代码）
                            // 使用反射调用方法，性能较低但兼容性好
                            Func<object, object> r = (object param) => { return (object)method.Invoke(null, new object[]{ param }); };
#else
                            // 使用泛型辅助方法创建高性能委托
                            MethodInfo genericHelper = typeof(TypeAdapter).GetMethod("ConvertTypeMethodHelper", 
                                BindingFlags.Static | BindingFlags.NonPublic);
                            
                            // 构造泛型方法
                            MethodInfo constructedHelper = genericHelper.MakeGenericMethod(from, to);

                            // 调用辅助方法创建委托
                            object ret = constructedHelper.Invoke(null, new object[] {method});
                            var r = (Func<object, object>) ret;
#endif

                            // 注册转换方法
                            adapters.Add((method.GetParameters()[0].ParameterType, method.ReturnType), r);
                            adapterMethods.Add((method.GetParameters()[0].ParameterType, method.ReturnType), method);
                        } catch (Exception e) {
                            Debug.LogError($"加载类型转换方法失败: {method}\n{e}");
                        }
                    }
                }
            }

            // 验证转换方法的完整性
            // 检查是否缺少反向转换方法
            foreach (var kp in adapters)
            {
                if (!adapters.ContainsKey((kp.Key.to, kp.Key.from)))
                    Debug.LogError($"缺少转换方法。有从 {kp.Key.from} 到 {kp.Key.to} 的转换，但没有从 {kp.Key.to} 到 {kp.Key.from} 的转换");
            }

            adaptersLoaded = true;
        }

        /// <summary>
        /// 检查两个类型是否不兼容
        /// 检查是否在明确声明的不兼容类型列表中
        /// </summary>
        /// <param name="from">源类型</param>
        /// <param name="to">目标类型</param>
        /// <returns>如果不兼容则返回true，否则返回false</returns>
        public static bool AreIncompatible(Type from, Type to)
        {
            if (incompatibleTypes.Any((k) => k.from == from && k.to == to))
                return true;
            return false;
        }

        /// <summary>
        /// 检查两个类型是否可以相互转换
        /// 首先检查是否不兼容，然后检查是否存在转换方法
        /// </summary>
        /// <param name="from">源类型</param>
        /// <param name="to">目标类型</param>
        /// <returns>如果可以转换则返回true，否则返回false</returns>
        public static bool AreAssignable(Type from, Type to)
        {
            // 确保适配器已加载
            if (!adaptersLoaded)
                LoadAllAdapters();
            
            // 检查是否明确声明为不兼容
            if (AreIncompatible(from, to))
                return false;

            // 检查是否存在转换方法
            return adapters.ContainsKey((from, to));
        }

        /// <summary>
        /// 获取指定类型对的转换方法信息
        /// </summary>
        /// <param name="from">源类型</param>
        /// <param name="to">目标类型</param>
        /// <returns>转换方法的MethodInfo</returns>
        public static MethodInfo GetConvertionMethod(Type from, Type to) => adapterMethods[(from, to)];

        /// <summary>
        /// 执行类型转换
        /// 将源对象转换为目标类型
        /// </summary>
        /// <param name="from">源对象</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>转换后的对象，如果转换失败则返回null</returns>
        public static object Convert(object from, Type targetType)
        {
            // 确保适配器已加载
            if (!adaptersLoaded)
                LoadAllAdapters();

            // 尝试获取转换委托
            Func<object, object> convertionFunction;
            if (adapters.TryGetValue((from.GetType(), targetType), out convertionFunction))
                return convertionFunction?.Invoke(from);

            return null;
        }
    }
}
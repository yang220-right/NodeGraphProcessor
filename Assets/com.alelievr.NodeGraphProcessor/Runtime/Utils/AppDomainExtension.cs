using System.Collections.Generic;
using System.Collections;
using System;

namespace GraphProcessor
{
	/// <summary>
	/// 应用程序域扩展工具类
	/// 提供AppDomain的扩展方法，用于安全地获取所有类型
	/// 处理程序集加载异常，确保类型枚举的稳定性
	/// </summary>
	public static class AppDomainExtension
	{
		/// <summary>
		/// 获取应用程序域中的所有类型
		/// 遍历所有已加载的程序集，安全地获取其中的所有类型
		/// </summary>
		/// <param name="domain">应用程序域</param>
		/// <returns>所有类型的枚举器</returns>
		public static IEnumerable< Type >	GetAllTypes(this AppDomain domain)
		{
            // 遍历应用程序域中的所有程序集
            foreach (var assembly in domain.GetAssemblies())
            {
				Type[] types = {};
				
                try {
					// 尝试获取程序集中的所有类型
					types = assembly.GetTypes();
				} catch {
					// 忽略程序集加载异常，继续处理其他程序集
					// 这种情况可能发生在动态加载的程序集或损坏的程序集中
				}

				// 返回当前程序集中的所有类型
				foreach (var type in types)
					yield return type;
			}
		}
	}
}

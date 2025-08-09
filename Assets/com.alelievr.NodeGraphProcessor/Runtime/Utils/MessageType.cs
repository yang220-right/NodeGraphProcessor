namespace GraphProcessor
{
	/// <summary>
	/// 节点消息类型枚举
	/// MessageType仅在UnityEditor中可用，所以我们为运行时创建了自己的版本
	/// 用于定义节点消息的严重程度和显示类型
	/// </summary>
	public enum NodeMessageType
	{
		/// <summary>
		/// 无消息
		/// 表示节点没有消息
		/// </summary>
		None,
		
		/// <summary>
		/// 信息消息
		/// 用于显示一般信息，通常为蓝色
		/// </summary>
		Info,
		
		/// <summary>
		/// 警告消息
		/// 用于显示警告信息，通常为黄色
		/// </summary>
		Warning,
		
		/// <summary>
		/// 错误消息
		/// 用于显示错误信息，通常为红色
		/// </summary>
		Error
	}
}
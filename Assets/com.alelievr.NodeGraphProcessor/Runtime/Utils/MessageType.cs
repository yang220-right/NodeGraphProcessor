namespace GraphProcessor
{
	// MessageType仅在UnityEditor中可用，所以我们为运行时创建了自己的版本
	public enum NodeMessageType
	{
		None,
		Info,
		Warning,
		Error
	}
}
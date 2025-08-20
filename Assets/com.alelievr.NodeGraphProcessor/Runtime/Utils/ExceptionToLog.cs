using UnityEngine;
using System;

namespace GraphProcessor{
  /// <summary>
  /// 异常转日志工具类
  /// 提供安全的委托调用机制，将异常转换为日志输出
  /// 在编辑器中捕获异常并记录到日志，在运行时直接执行
  /// </summary>
  public static class ExceptionToLog{
    /// <summary>
    /// 安全调用委托
    /// 执行传入的委托，如果发生异常则记录到日志中
    /// </summary>
    /// <param name="a">要执行的委托</param>
    public static void Call(Action a){
      #if UNITY_EDITOR
      try{
        // 在编辑器中，尝试执行委托并捕获异常
        #endif
        a?.Invoke();
        #if UNITY_EDITOR
      }
      catch (Exception e){
        // 将异常记录到Unity日志中
        Debug.LogException(e);
      }
      #endif
    }
  }
}
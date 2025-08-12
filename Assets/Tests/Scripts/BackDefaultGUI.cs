using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Examples;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

public class BackDefaultGUI : SerializedMonoBehaviour{
  #region 删除折叠

  #endregion
  #region 删除折叠
  [OnInspectorDispose("@UnityEngine.Debug.Log(\"Dispose event invoked!\")")]
  [ShowInInspector, InfoBox("当您更改此字段的类型，或者将其设置为“空”时，之前的属性设置将会被清除。当您取消选择此示例时，属性设置也会被清除。"), DisplayAsString]
  public BaseClass PolymorphicField;

  public abstract class BaseClass { public override string ToString() { return this.GetType().Name; } }
  public class A : BaseClass { }
  public class B : BaseClass { }
  public class C : BaseClass { }

  #endregion
}
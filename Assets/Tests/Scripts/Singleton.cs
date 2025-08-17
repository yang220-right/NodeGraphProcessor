using System;

public abstract class Singleton<T>{
  private static T ins;

  private static readonly object _lock = new object();

  public static T Ins{
    get{
      lock (_lock){
        if (ins != null)
          return ins;

        ins = (T)Activator.CreateInstance(typeof(T), true);
        (ins as Singleton<T>).Init();
        return ins;
      }
    }
  }

  public virtual void Init(){
  }
}
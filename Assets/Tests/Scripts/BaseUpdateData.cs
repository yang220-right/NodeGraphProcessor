using UnityEngine;

public abstract class BaseUpdateData : MonoBehaviour
{
  public abstract void UpdateData(params object[] args);
}
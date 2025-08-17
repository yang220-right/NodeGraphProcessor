using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ImageScrollUpdateData : BaseUpdateData
{
    public Text text;

    public override void UpdateData(params object[] args){
        var index = (int)args[0];
        Debug.Log($"当前更新{index}");
        var total = (int)args[1];
        if (index < 0){
            index = -index;
            index = total - index % total;
        }else index %= total;
        text.text = index.ToString();
    }
}

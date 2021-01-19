using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OneP.InfinityScrollView;
using UnityEngine.UI;

public class GridItem : InfinityBaseItem
{
    public Text text;
    public override void Reload(int _index)
    {
        base.Reload(_index);
        text.text = "Item_" + (Index + 1);
    }
}

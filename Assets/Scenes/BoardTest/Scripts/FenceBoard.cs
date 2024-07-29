using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceBoard : Board<Fence>
{
    public const int SIZE = 8;
    public override int BoardSize => SIZE;

    // Start is called before the first frame update
    public void Init()
    {
        InitBoard();
    }
}

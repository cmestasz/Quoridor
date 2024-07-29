using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceBoard : Board<Fence>
{
    public const int SIZE = 8;
    public override int boardSize => SIZE;

    // Start is called before the first frame update
    void Start()
    {
        InitBoard();
    }

    // Update is called once per frame
    void Update()
    {

    }


}

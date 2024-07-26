using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : MonoBehaviour
{
    public bool active;
    public bool vertical;

    // Start is called before the first frame update
    void Start()
    {
        active = false;
        vertical = false;
        GetComponent<SpriteRenderer>().color = Color.clear;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool IsVertical()
    {
        return active && vertical;
    }

    public void Build(bool vertical)
    {

        active = true;
        this.vertical = vertical;
        GetComponent<SpriteRenderer>().color = Color.white;
        transform.eulerAngles = new Vector3(0, 0, vertical ? 90 : 0);

    }
}

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
        GetComponent<SpriteRenderer>().color = Color.clear;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Build(bool vertical)
    {
        active = true;
        this.vertical = vertical;
        GetComponent<SpriteRenderer>().color = Color.white;
        transform.eulerAngles = new Vector3(0, 0, vertical ? 90 : 0);
    }

    public void Unbuild()
    {
        active = false;
        GetComponent<SpriteRenderer>().color = Color.clear;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public const int EMPTY = 0, PLAYER = 1;
    [SerializeField] private Sprite[] sprites;
    public int type;
    public float f, g, h;
    public Tile prev;

    // Start is called before the first frame update
    void Start()
    {
        type = EMPTY;
        f = g = h = float.MaxValue;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetState()
    {
        f = g = h = float.MaxValue;
        prev = null;
    }

    public void SetType(int type)
    {
        this.type = type;
        GetComponent<SpriteRenderer>().sprite = sprites[type];
    }

    /*
    public void OnMouseDown()
    {
        if (type == EMPTY)
        {
            SetType(PLAYER);
        }
        else if (type == PLAYER)
        {
            SetType(WALL);
        }
        else if (type == WALL)
        {
            SetType(EMPTY);
        }
    }
    */
}

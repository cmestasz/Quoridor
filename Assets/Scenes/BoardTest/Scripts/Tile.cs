using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public const int EMPTY = 0, PLAYER = 1, WALL = 2;
    [SerializeField] private Sprite[] sprites;
    private int type;


    // Start is called before the first frame update
    void Start()
    {
        type = EMPTY;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetType(int type)
    {
        this.type = type;
        GetComponent<SpriteRenderer>().sprite = sprites[type];
    }

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
}

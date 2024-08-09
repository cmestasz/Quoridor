using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public const int EMPTY = 0, PLAYER = 1, DESTINATION = 2;
    [SerializeField] private Sprite[] sprites;
    public TextMeshPro label;
    public int type;
    public int player = -1;
    public Color color;
    public float f, g, h;
    public Tile prev;

    // Start is called before the first frame update
    void Start()
    {
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

    public void SetColor(Color color)
    {
        this.color = color;
        GetComponent<SpriteRenderer>().color = color;
    }
}

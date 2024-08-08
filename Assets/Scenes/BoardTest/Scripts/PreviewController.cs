using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Sprite fenceSprite;
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private GameObject playerPreviewPrefab;
    private GameObject[] playerPreviews = new GameObject[10];
    private int prevPreviews;
    private SpriteRenderer spriteRenderer;
    private FenceBoard fenceBoard;
    private TileBoard tileBoard;
    private Color previewColor;
    private List<Vector2Int> validMoves = new();
    private Vector3 FAR_AWAY = new(-25, 0, 0);
    private Vector3 ROTATION_0 = new(0, 0, 0);
    private Vector3 ROTATION_90 = new(0, 0, 90);

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < playerPreviews.Length; i++)
        {
            playerPreviews[i] = Instantiate(playerPreviewPrefab, FAR_AWAY, Quaternion.identity);
        }
        fenceBoard = gameManager.fenceBoard;
        tileBoard = gameManager.tileBoard;
        spriteRenderer = GetComponent<SpriteRenderer>();
        previewColor = playerPreviewPrefab.GetComponent<SpriteRenderer>().color;
    }

    public void EndGame()
    {
        for (int i = 0; i < playerPreviews.Length; i++)
        {
            Destroy(playerPreviews[i]);
        }
        Destroy(gameObject);
    }

    public void UpdatePreview()
    {
        if (gameManager.IsCurrentBuilding())
        {
            spriteRenderer.sprite = fenceSprite;
            if (gameManager.IsCurrentVertical())
                transform.eulerAngles = ROTATION_90;
            else
                transform.eulerAngles = ROTATION_0;
            
            for (int i = 0; i < prevPreviews; i++)
            {
                playerPreviews[i].GetComponent<SpriteRenderer>().color = Color.clear;
            }
        }
        else
        {
            spriteRenderer.sprite = playerSprite;
            transform.eulerAngles = ROTATION_0;
            validMoves = gameManager.GetValidMoves();
            for (int i = 0; i < Mathf.Max(validMoves.Count, prevPreviews); i++)
            {
                if (i >= validMoves.Count)
                {
                    playerPreviews[i].transform.position = FAR_AWAY;
                    continue;
                }
                playerPreviews[i].transform.position = tileBoard.RelativeToRealPos(validMoves[i]);
                playerPreviews[i].GetComponent<SpriteRenderer>().color = previewColor;
            }
            prevPreviews = validMoves.Count;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (gameManager.IsCurrentBuilding())
        {
            pos = fenceBoard.LockedRealPos(pos);
        }
        else
        {
            bool found = false;
            for (int i = 0; i < validMoves.Count; i++)
            {
                if (tileBoard.RealToRelativePos(pos) == validMoves[i])
                {
                    pos = tileBoard.RelativeToRealPos(validMoves[i]);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                pos = FAR_AWAY;
            }
        }
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

    }
}

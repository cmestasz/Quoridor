using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PreviewController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Sprite fenceSprite;
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private GameObject playerPreviewPrefab;
    [SerializeField] private Transform previewsParent;
    private readonly GameObject[] playerPreviews = new GameObject[10];
    private int prevPreviews;
    private SpriteRenderer spriteRenderer;
    private FenceBoard fenceBoard;
    private TileBoard tileBoard;
    private Color previewColor;
    private HashSet<Vector2Int> validMovesSet;
    private HashSet<Tuple<Vector2Int, bool>> validBuildsSet;
    private Vector3 FAR_AWAY = new(-25, 0, 0);
    private Vector3 ROTATION_0 = new(0, 0, 0);
    private Vector3 ROTATION_90 = new(0, 0, 90);

    public void Init()
    {
        for (int i = 0; i < playerPreviews.Length; i++)
        {
            playerPreviews[i] = Instantiate(playerPreviewPrefab, FAR_AWAY, Quaternion.identity, previewsParent);
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
            validBuildsSet = gameManager.GetCurrentValidBuilds();
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
            validMovesSet = gameManager.GetCurrentValidMoves();
            List<Vector2Int> validMovesList = validMovesSet.ToList();
            for (int i = 0; i < Mathf.Max(validMovesSet.Count, prevPreviews); i++)
            {
                if (i >= validMovesSet.Count)
                {
                    playerPreviews[i].transform.position = FAR_AWAY;
                    continue;
                }
                playerPreviews[i].transform.position = tileBoard.RelativeToRealPos(validMovesList[i]);
                playerPreviews[i].GetComponent<SpriteRenderer>().color = previewColor;
            }
            prevPreviews = validMovesSet.Count;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (gameManager.IsCurrentBuilding())
        {
            Vector2Int relativePos = fenceBoard.RealToRelativePos(pos);
            pos = fenceBoard.RelativeToRealPos(relativePos);

            Tuple<Vector2Int, bool> build = new(relativePos, gameManager.IsCurrentVertical());
            if (validBuildsSet.Contains(build))
            {
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.color = Color.red;
            }
        }
        else
        {
            if (validMovesSet.Contains(tileBoard.RealToRelativePos(pos)))
            {
                pos = tileBoard.RelativeToRealPos(tileBoard.RealToRelativePos(pos));
            } else {
                pos = FAR_AWAY;
            }
            spriteRenderer.color = previewColor;
        }
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

    }
}

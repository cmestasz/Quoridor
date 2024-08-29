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
    private readonly GameObject[] playerPreviews = new GameObject[10];
    private int prevPreviews;
    private SpriteRenderer spriteRenderer;
    private FenceBoard fenceBoard;
    private TileBoard tileBoard;
    private Color previewColor;
    private List<Vector2Int> validMovesList;
    private HashSet<Tuple<Vector2Int, bool>> validBuildsSet;
    private Vector3 FAR_AWAY = new(-25, 0, 0);
    private Vector3 ROTATION_0 = new(0, 0, 0);
    private Vector3 ROTATION_90 = new(0, 0, 90);

    public void Init()
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
            validMovesList = gameManager.GetCurrentValidMoves().ToList();
            for (int i = 0; i < Mathf.Max(validMovesList.Count, prevPreviews); i++)
            {
                if (i >= validMovesList.Count)
                {
                    playerPreviews[i].transform.position = FAR_AWAY;
                    continue;
                }
                playerPreviews[i].transform.position = tileBoard.RelativeToRealPos(validMovesList[i]);
                playerPreviews[i].GetComponent<SpriteRenderer>().color = previewColor;
            }
            prevPreviews = validMovesList.Count;
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
            bool found = false;
            for (int i = 0; i < validMovesList.Count; i++)
            {
                if (tileBoard.RealToRelativePos(pos) == validMovesList[i])
                {
                    pos = tileBoard.RelativeToRealPos(validMovesList[i]);
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

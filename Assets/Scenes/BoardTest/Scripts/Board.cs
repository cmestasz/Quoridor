using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    private int boardSize = BoardConstants.BOARD_SIZE;
    private int boardScale = BoardConstants.BOARD_SCALE;

    // Start is called before the first frame update
    void Start()
    {
        tilePrefab.transform.localScale = new Vector3(boardScale, 1, boardScale);
        float offset = boardSize * boardScale / 2.0f - boardScale / 2.0f;
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                float x = i * boardScale - offset;
                float y = j * boardScale - offset;
                Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    [SerializeField] private FenceBoard fenceBoard;
    private bool building;
    private bool vertical;
    private Vector2Int[] HORIZONTAL_NEIGHBORS = new Vector2Int[] { new(1, 0), new(-1, 0) };
    private Vector2Int[] VERTICAL_NEIGHBORS = new Vector2Int[] { new(0, 1), new(0, -1) };

    // Start is called before the first frame update
    void Start()
    {
        building = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            building = !building;
            Debug.Log("Building: " + building);
        }
        if (building)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                vertical = !vertical;
                Debug.Log("Vertical: " + vertical);
            }
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int relativePos = fenceBoard.RealToRelativePos(mousePos);
                Fence fence = fenceBoard.GetByRelativePos(relativePos);
                if (fence == null)
                {
                    Debug.Log("Invalid position");
                    return;
                }
                if (fence.active)
                {
                    Debug.Log("Already built");
                    return;
                }
                for (int i = 0; i < 2; i++)
                {
                    Fence neighbor = fenceBoard.GetNeighbor(relativePos.x, relativePos.y, vertical ? VERTICAL_NEIGHBORS[i] : HORIZONTAL_NEIGHBORS[i]);
                    if (neighbor != null && neighbor.active && neighbor.vertical == vertical)
                    {
                        Debug.Log("Invalid position");
                        return;
                    }
                }
                fence.Build(vertical);
            }
        }
    }
}

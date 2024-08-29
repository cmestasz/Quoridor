using static Constants;

public class FenceBoard : Board<Fence>
{
    public override int BoardSize => FENCE_BOARD_SIZE;

    // Start is called before the first frame update
    public void Init()
    {
        InitBoard();
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                Fence fence = tiles[i, j];
                //fence.label.text = $"{i}, {j}";
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                Fence fence = tiles[i, j];
                fence.Unbuild();
            }
        }
    }
}

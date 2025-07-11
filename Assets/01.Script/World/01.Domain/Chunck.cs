using UnityEngine;
using System.Collections.Generic;
public class Chunck
{
    public readonly Vector3 ChunckSize = new Vector3(16, 32, 16);
    private List<Block> _blockList;

    public Chunck(List<Block> blockList)
    {
        _blockList = blockList;
    }


}

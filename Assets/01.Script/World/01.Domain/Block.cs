public class Block
{
    public EBlockType Type { get; private set; }
    public BlockPosition Position { get; private set; }

    public Block(EBlockType type, BlockPosition position)
    {
        Type = type;
        Position = position;
    }
}

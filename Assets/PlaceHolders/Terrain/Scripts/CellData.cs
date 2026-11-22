using UnityEngine;

public class CellData
{
    // null until a tile is chosen.
    // the existence of a tile implies that it is collapsed!
    public TileType? CollapsedTile = null;
    public TileType[] TileOptions;
    public Vector3Int Position;
    
    public CellData(Vector3Int position, TileType[] tileOptions)
    {
        Position = position;
        TileOptions = tileOptions;
    }
}
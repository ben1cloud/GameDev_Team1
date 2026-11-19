using UnityEngine;

public class Cell : MonoBehaviour
{
    // null until a tile is chosen.
    // the existence of a tile implies that it is collapsed!
    public Tile collapsedTile;
    
    public Tile[] tileOptions;
}

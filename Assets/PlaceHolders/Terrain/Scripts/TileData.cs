using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TileType;

public enum TileType
{
    GrassA,
    GrassB,
    PathBendBL,
    PathBendBR,
    PathBendTL,
    PathBendTR,
    PathHorizontal,
    PathVertical,
}

public class TileData
{
    private const string PrefabFolder = "Tiles/";
    private const float Size = 4f;

    public readonly TileType Type;
    
    public readonly TileType[] ValidLeftNeighbours;
    public readonly TileType[] ValidRightNeighbours;
    public readonly TileType[] ValidAboveNeighbours;
    public readonly TileType[] ValidBelowNeighbours;
    public readonly TileType[] ValidUpNeighbours;
    public readonly TileType[] ValidDownNeighbours;

    private readonly GameObject _prefab;

    public TileData(
        TileType type,
        TileType[] left, TileType[] right,
        TileType[] above, TileType[] below,
        TileType[] up, TileType[] down)
    {
        Type = type;
        
        ValidLeftNeighbours = left;
        ValidRightNeighbours = right;
        ValidAboveNeighbours = above;
        ValidBelowNeighbours = below;
        ValidUpNeighbours = up;
        ValidDownNeighbours = down;
        
        _prefab = Resources.Load<GameObject>(PrefabFolder + type);
    }

    public void Instantiate(Vector3 pos)
    {
        var position = new Vector3(pos.x * Size, pos.y * Size, pos.z * Size);
        Object.Instantiate(_prefab, position, _prefab.transform.rotation);
    }
}

public static class TileDefinitions
{
    public static readonly TileData[] All =
    {
        new(GrassA,
            left:  new[] { GrassB, PathBendBL, PathBendTL, PathVertical },
            right: new[] { GrassB, PathBendBR, PathBendTR, PathVertical },
            above: new TileType[] { },
            below: new TileType[] { },
            up:    new[] { GrassB, PathBendTL, PathBendTR, PathHorizontal },
            down:  new[] { GrassB, PathBendBL, PathBendBR, PathHorizontal }),
        
        new(GrassB,
            left:  new[] { GrassA, PathBendBL, PathBendTL, PathVertical },
            right: new[] { GrassA, PathBendBR, PathBendTR, PathVertical },
            above: new TileType[] { },
            below: new TileType[] { },
            up:    new[] { GrassA, PathBendTL, PathBendTR, PathHorizontal },
            down:  new[] { GrassA, PathBendBL, PathBendBR, PathHorizontal }),
        
        new(PathBendBL,
            left:  new[] { PathBendBR, PathBendTR, PathHorizontal },
            right: new[] { GrassA, GrassB, PathBendBR, PathBendTR, PathVertical },
            above: new TileType[] { },
            below: new TileType[] { },
            up:    new[] { GrassA, GrassB, PathBendTL, PathBendTR, PathHorizontal },
            down:  new[] { PathBendTL, PathBendTR, PathVertical }),
        
        new(PathBendBR,
            left:  new[] { GrassA, GrassB, PathBendBL, PathBendTL, PathVertical },
            right: new[] { PathBendBL, PathBendTL, PathHorizontal },
            above: new TileType[] { },
            below: new TileType[] { },
            up:    new[] { GrassA, GrassB, PathBendTL, PathBendTR, PathHorizontal },
            down:  new[] { PathBendTL, PathBendTR, PathVertical }),
        
        new(PathBendTL,
            left:  new[] { PathBendBR, PathBendTR, PathHorizontal },
            right: new[] { GrassA, GrassB, PathBendBR, PathBendTR, PathVertical },
            above: new TileType[] { },
            below: new TileType[] { },
            up:    new[] { PathBendBL, PathBendBR, PathVertical },
            down:  new[] { GrassA, GrassB, PathBendBL, PathBendBR, PathHorizontal }),
        
        new(PathBendTR,
            left:  new[] { GrassA, GrassB, PathBendBL, PathBendTL, PathVertical },
            right: new[] { PathBendBL, PathBendTL, PathHorizontal },
            above: new TileType[] { },
            below: new TileType[] { },
            up:    new[] { PathBendBL, PathBendBR, PathVertical },
            down:  new[] { GrassA, GrassB, PathBendBL, PathBendBR, PathHorizontal }),
        
        new(PathHorizontal,
            left:  new[] { PathBendBR, PathBendTR },
            right: new[] { PathBendBL, PathBendTL },
            above: new TileType[] { },
            below: new TileType[] { },
            up:    new[] { GrassA, GrassB, PathBendTL, PathBendTR },
            down:  new[] { GrassA, GrassB, PathBendBL, PathBendBR }),
        
        new(PathVertical,
            left:  new[] { GrassA, GrassB, PathBendBL, PathBendTL },
            right: new[] { GrassA, GrassB, PathBendBR, PathBendTR },
            above: new TileType[] { },
            below: new TileType[] { },
            up:    new[] { PathBendBL, PathBendBR },
            down:  new[] { PathBendTL, PathBendTR }),
    };

    public static readonly TileType[] AllTypes = (TileType[])System.Enum.GetValues(typeof(TileType));

    private static readonly Dictionary<TileType, TileData> ByType = All.ToDictionary(t => t.Type);
    public static TileData Get(TileType type) => ByType[type];
    public const TileType Backup = GrassA;
}

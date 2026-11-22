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
    public readonly TileType[] ValidUpNeighbours;
    public readonly TileType[] ValidDownNeighbours;
    public readonly TileType[] ValidLeftNeighbours;
    public readonly TileType[] ValidRightNeighbours;

    private readonly GameObject _prefab;

    public TileData(TileType type, TileType[] up, TileType[] down, TileType[] left, TileType[] right)
    {
        Type = type;
        _prefab = Resources.Load<GameObject>(PrefabFolder + type);
        ValidUpNeighbours = up;
        ValidDownNeighbours = down;
        ValidLeftNeighbours = left;
        ValidRightNeighbours = right;
    }

    public GameObject Instantiate(Vector3 pos)
    {
        var position = new Vector3(pos.x * Size, pos.y * Size, pos.z * Size);
        return Object.Instantiate(_prefab, position, _prefab.transform.rotation);
    }
}

public static class TileDefinitions
{
    public static readonly TileData[] All =
    {
        new(GrassA,
            up:    new[] { GrassB, PathBendTL, PathBendTR, PathHorizontal },
            down:  new[] { GrassB, PathBendBL, PathBendBR, PathHorizontal },
            left:  new[] { GrassB, PathBendBL, PathBendTL, PathVertical },
            right: new[] { GrassB, PathBendBR, PathBendTR, PathVertical }),
        
        new(GrassB,
            up:    new[] { GrassA, PathBendTL, PathBendTR, PathHorizontal },
            down:  new[] { GrassA, PathBendBL, PathBendBR, PathHorizontal },
            left:  new[] { GrassA, PathBendBL, PathBendTL, PathVertical },
            right: new[] { GrassA, PathBendBR, PathBendTR, PathVertical }),
        
        new(PathBendBL,
            up:    new[] { GrassA, GrassB, PathBendTL, PathBendTR, PathHorizontal },
            down:  new[] { PathBendTL, PathBendTR, PathVertical },
            left:  new[] { PathBendBR, PathBendTR, PathHorizontal },
            right: new[] { GrassA, GrassB, PathBendBR, PathBendTR, PathVertical }),
        
        new(PathBendBR,
            up:    new[] { GrassA, GrassB, PathBendTL, PathBendTR, PathHorizontal },
            down:  new[] { PathBendTL, PathBendTR, PathVertical },
            left:  new[] { GrassA, GrassB, PathBendBL, PathBendTL, PathVertical },
            right: new[] { PathBendBL, PathBendTL, PathHorizontal }),
        
        new(PathBendTL,
            up:    new[] { PathBendBL, PathBendBR, PathVertical },
            down:  new[] { GrassA, GrassB, PathBendBL, PathBendBR, PathHorizontal },
            left:  new[] { PathBendBR, PathBendTR, PathHorizontal },
            right: new[] { GrassA, GrassB, PathBendBR, PathBendTR, PathVertical }),
        
        new(PathBendTR,
            up:    new[] { PathBendBL, PathBendBR, PathVertical },
            down:  new[] { GrassA, GrassB, PathBendBL, PathBendBR, PathHorizontal },
            left:  new[] { GrassA, GrassB, PathBendBL, PathBendTL, PathVertical },
            right: new[] { PathBendBL, PathBendTL, PathHorizontal }),
        
        new(PathHorizontal,
            up:    new[] { GrassA, GrassB, PathBendTL, PathBendTR },
            down:  new[] { GrassA, GrassB, PathBendBL, PathBendBR },
            left:  new[] { PathBendBR, PathBendTR },
            right: new[] { PathBendBL, PathBendTL }),
        
        new(PathVertical,
            up:    new[] { PathBendBL, PathBendBR },
            down:  new[] { PathBendTL, PathBendTR },
            left:  new[] { GrassA, GrassB, PathBendBL, PathBendTL },
            right: new[] { GrassA, GrassB, PathBendBR, PathBendTR }),
    };

    public static readonly TileType[] AllTypes = (TileType[])System.Enum.GetValues(typeof(TileType));

    private static readonly Dictionary<TileType, TileData> ByType = All.ToDictionary(t => t.Type);
    public static TileData Get(TileType type) => ByType[type];
    public const TileType Backup = GrassA;
}

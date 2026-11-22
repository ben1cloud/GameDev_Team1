using UnityEngine;

public class WaveFunctionGenerator : MonoBehaviour
{
    public Vector3Int dimensions;
    private WaveFunctionCollapse _collapse;
    
    private void Awake()
    {
        _collapse = new WaveFunctionCollapse(dimensions);
        StartCoroutine(_collapse.Generate());
    }
}

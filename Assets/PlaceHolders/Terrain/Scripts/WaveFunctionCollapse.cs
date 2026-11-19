using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    public int dimensions;
    public Tile[] tileObjects;
    public List<Cell> gridComponents;
    public Cell cellObj;

    public int tileSize;
    public Tile backupTile;

    private void Awake()
    {
        gridComponents = new List<Cell>();
        InitializeGrid();
        StartCoroutine(Generate());
    }
    
    void InitializeGrid()
    {
        for (var y = 0; y < dimensions; y++)
        {
            for (var x = 0; x < dimensions; x++)
            {
                var position = new Vector3(x * tileSize, 0, y * tileSize);
                var newCell = Instantiate(cellObj, position,  Quaternion.identity);
                newCell.tileOptions = tileObjects;
                gridComponents.Add(newCell);
            }
        }
    }

    IEnumerator Generate()
    {
        for (var i = 0; i < dimensions * dimensions; i++)
        {
            var cell = GetNextCell();
            CollapseCell(cell);
            UpdateOptions();

            yield return new WaitForSeconds(0.01f);
        }
    }

    Cell GetNextCell()
    {
        var candidates = new List<Cell>(gridComponents);
        
        // remove cells that are done
        candidates.RemoveAll(c => c.collapsedTile is not null);
        
        // keep only the cells with the lowest length
        candidates.Sort((a,b) => a.tileOptions.Length - b.tileOptions.Length);
        candidates.RemoveAll(c => c.tileOptions.Length != candidates[0].tileOptions.Length);
        
        // pick from the candidates
        var randIndex = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[randIndex];
    }

    void CollapseCell(Cell cell)
    {
        var selectedTile = backupTile;
        if (cell.tileOptions.Length > 0)
        {
            var randIndex = UnityEngine.Random.Range(0, cell.tileOptions.Length);
            selectedTile = cell.tileOptions[randIndex];
        }
        
        cell.collapsedTile = selectedTile;
        Instantiate(selectedTile, cell.transform.position, selectedTile.transform.rotation);
    }
    
    void UpdateOptions()
    {
        for (var y = 0; y < dimensions; y++)
        {
            for (var x = 0; x < dimensions; x++)
            {
                var cell = gridComponents[x + y * dimensions];
                UpdateOptionsCell(cell, x, y);
            }
        }
    }

    void UpdateOptionsCell(Cell cell, int x, int y)
    {
        if (cell.collapsedTile is not null) return;

        var newOptions = new HashSet<Tile>(cell.tileOptions);
        if (y > 0)
        {
            var upIdx = x + (y - 1) * dimensions;
            NarrowOptions(newOptions, gridComponents[upIdx], tile => tile.validDownNeighbours);
        }
        if (x < dimensions - 1)
        {
            var leftIdx = (x + 1) + y * dimensions;
            NarrowOptions(newOptions, gridComponents[leftIdx], tile => tile.validRightNeighbours);
        }
        if (y < dimensions - 1)
        {
            var downIdx = x + (y + 1) * dimensions;
            NarrowOptions(newOptions, gridComponents[downIdx], tile => tile.validUpNeighbours);
        }
        if (x > 0)
        {
            var rightIdx = (x - 1) + y * dimensions;
            NarrowOptions(newOptions, gridComponents[rightIdx], tile => tile.validLeftNeighbours);
        }

        cell.tileOptions = newOptions.ToArray();
    }

    void NarrowOptions(HashSet<Tile> newOptions, Cell neighbour, Func<Tile, Tile[]> getAllowedTiles)
    {
        var neighbourOptions = neighbour.collapsedTile is not null
            ? new[] { neighbour.collapsedTile }
            : neighbour.tileOptions;

        var allowedTiles = new HashSet<Tile>();
        foreach (var option in neighbourOptions)
        {
            allowedTiles.UnionWith(getAllowedTiles(option));
        }
        newOptions.IntersectWith(allowedTiles);
    }
}

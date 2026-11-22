    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class WaveFunctionCollapse : MonoBehaviour
    {
        public int dimensions;
        private List<CellData> _cells = new();

        private void Awake()
        {
            InitializeGrid();
            StartCoroutine(Generate());
        }
        
        void InitializeGrid()
        {
            for (var y = 0; y < dimensions; y++)
            {
                for (var x = 0; x < dimensions; x++)
                {
                    var position = new Vector3(x, 0, y);
                    var options = TileDefinitions.AllTypes.ToArray(); // use ToArray to copy
                    
                    var cell = new CellData(position, options);
                    _cells.Add(cell);
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

        CellData GetNextCell()
        {
            var candidates = new List<CellData>(_cells);
            
            // remove cells that are done
            candidates.RemoveAll(c => c.CollapsedTile is not null);
            
            // keep only the cells with the lowest length
            candidates.Sort((a,b) => a.TileOptions.Length - b.TileOptions.Length);
            candidates.RemoveAll(c => c.TileOptions.Length != candidates[0].TileOptions.Length);
            
            // pick from the candidates
            var randIndex = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[randIndex];
        }

        void CollapseCell(CellData cell)
        {
            var selectedTile = TileDefinitions.Backup;
            if (cell.TileOptions.Length > 0)
            {
                var randIndex = UnityEngine.Random.Range(0, cell.TileOptions.Length);
                selectedTile = cell.TileOptions[randIndex];
            }
            
            cell.CollapsedTile = selectedTile;
            TileDefinitions.Get(selectedTile).Instantiate(cell.Position);
        }
        
        void UpdateOptions()
        {
            for (var y = 0; y < dimensions; y++)
            {
                for (var x = 0; x < dimensions; x++)
                {
                    var cell = _cells[x + y * dimensions];
                    UpdateOptionsCell(cell, x, y);
                }
            }
        }

        void UpdateOptionsCell(CellData cell, int x, int y)
        {
            if (cell.CollapsedTile is not null) return;

            var newOptions = new HashSet<TileType>(cell.TileOptions);
            if (y > 0)
            {
                var upIdx = x + (y - 1) * dimensions;
                NarrowOptions(newOptions, _cells[upIdx], tile => tile.ValidDownNeighbours);
            }
            if (x < dimensions - 1)
            {
                var leftIdx = (x + 1) + y * dimensions;
                NarrowOptions(newOptions, _cells[leftIdx], tile => tile.ValidRightNeighbours);
            }
            if (y < dimensions - 1)
            {
                var downIdx = x + (y + 1) * dimensions;
                NarrowOptions(newOptions, _cells[downIdx], tile => tile.ValidUpNeighbours);
            }
            if (x > 0)
            {
                var rightIdx = (x - 1) + y * dimensions;
                NarrowOptions(newOptions, _cells[rightIdx], tile => tile.ValidLeftNeighbours);
            }

            cell.TileOptions = newOptions.ToArray();
        }

        void NarrowOptions(HashSet<TileType> newOptions, CellData neighbour, Func<TileData, TileType[]> getAllowedTiles)
        {
            var neighbourOptions = neighbour.CollapsedTile is not null
                ? new[] { (TileType)neighbour.CollapsedTile }
                : neighbour.TileOptions;

            var allowedTiles = new HashSet<TileType>();
            foreach (var option in neighbourOptions)
            {
                var tileData = TileDefinitions.Get(option);
                allowedTiles.UnionWith(getAllowedTiles(tileData));
            }
            newOptions.IntersectWith(allowedTiles);
        }
    }

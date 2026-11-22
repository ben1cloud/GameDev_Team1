    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class WaveFunctionCollapse
    {
        public readonly CellData[,,] Cells;
        
        private readonly Vector3Int _dimensions;
        private readonly HashSet<CellData> _candidates = new();

        public WaveFunctionCollapse(Vector3Int dimensions)
        {
            _dimensions = dimensions;
            Cells = new CellData[dimensions.x, dimensions.y, dimensions.z];
            
            InitializeGrid();
        }
        
        public IEnumerator Generate()
        {
            for (var i = 0; i < Cells.Length; i++)
            {
                var cell = GetNextCell();
                CollapseCell(cell);
                UpdateOptions();

                yield return new WaitForSeconds(0.01f);
            }
        }

        private void InitializeGrid()
        {
            for (var x = 0; x < _dimensions.x; x++)
            for (var y = 0; y < _dimensions.y; y++)
            for (var z = 0; z < _dimensions.z; z++)
                InitializeCell(new Vector3Int(x, y, z));
        }
        private void InitializeCell(Vector3Int position)
        {
            var options = TileDefinitions.AllTypes.ToArray(); // use ToArray to copy
            var cell = new CellData(position, options);
            
            Cells[position.x, position.y, position.z] = cell;
            _candidates.Add(cell);
        }

        private CellData GetNextCell()
        {
            var newCandidates = new List<CellData>(_candidates);
            
            // keep only the cells with the lowest length
            newCandidates.Sort((a,b) => a.TileOptions.Length - b.TileOptions.Length);
            newCandidates.RemoveAll(c => c.TileOptions.Length != newCandidates[0].TileOptions.Length);
            
            // pick from the candidates
            var randIndex = UnityEngine.Random.Range(0, newCandidates.Count);
            return newCandidates[randIndex];
        }

        private void CollapseCell(CellData cell)
        {
            var selectedTile = TileDefinitions.Backup;
            if (cell.TileOptions.Length > 0)
            {
                var randIndex = UnityEngine.Random.Range(0, cell.TileOptions.Length);
                selectedTile = cell.TileOptions[randIndex];
            }
            
            cell.CollapsedTile = selectedTile;
            _candidates.Remove(cell);
            TileDefinitions.Get(selectedTile).Instantiate(cell.Position);
        }

        private void UpdateOptions()
        {
            for (var x = 0; x < _dimensions.x; x++)
            for (var y = 0; y < _dimensions.y; y++)
            for (var z = 0; z < _dimensions.z; z++)
                UpdateOptionsCell(Cells[x,y,z]);
        }
        private void UpdateOptionsCell(CellData cell)
        {
            if (cell.CollapsedTile is not null) return;

            var x = cell.Position.x;
            var y = cell.Position.y;
            var z = cell.Position.z;

            var newOptions = new HashSet<TileType>(cell.TileOptions);
            if (x > 0)
            {
                var rightCell = Cells[x-1, y, z];
                NarrowOptions(newOptions, rightCell, tile => tile.ValidLeftNeighbours);
            }
            if (x < _dimensions.x - 1)
            {
                var leftCell = Cells[x+1, y, z];
                NarrowOptions(newOptions, leftCell, tile => tile.ValidRightNeighbours);
            }
            if (y > 0)
            {
                var belowCell = Cells[x, y-1, z];
                NarrowOptions(newOptions, belowCell, tile => tile.ValidAboveNeighbours);
            }
            if (y < _dimensions.y - 1)
            {
                var aboveCell = Cells[x, y+1, z];
                NarrowOptions(newOptions, aboveCell, tile => tile.ValidBelowNeighbours);
            }
            if (z > 0)
            {
                var upCell = Cells[x, y, z-1];
                NarrowOptions(newOptions, upCell, tile => tile.ValidDownNeighbours);
            }
            if (z < _dimensions.z - 1)
            {
                var downCell = Cells[x, y, z+1];
                NarrowOptions(newOptions, downCell, tile => tile.ValidUpNeighbours);
            }

            cell.TileOptions = newOptions.ToArray();
        }

        private static void NarrowOptions(
            HashSet<TileType> newOptions, CellData neighbour, Func<TileData, TileType[]> getAllowedTiles)
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

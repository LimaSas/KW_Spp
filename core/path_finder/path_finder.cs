using Godot;
using System;
using System.Collections.Generic;

public class PathFinder : RefCounted
{
	private TileMap _groundTileMap;
	private AStar2D _astar;

	public PathFinder(TileMap groundTileMap)
	{
		_groundTileMap = groundTileMap;
		_InitAstar(_groundTileMap);
	}

	// Initialize AStar with the TileMap data
	private void _InitAstar(TileMap tileMap)
	{
		_astar = new AStar2D();
		Vector2 TILE_ADJ = tileMap.CellSize / 2; // Offset to center of the tile
		var cells = tileMap.GetUsedCells();
		cells.Sort(); // Sort the cells by X and Y coordinates

		for (int i = 0; i < cells.Count; i++)
		{
			var cell = cells[i];
			// Register the point (Vector2) at the center of the tile as an Astar node
			_astar.AddPoint(i, tileMap.MapToLocal(cell) + TILE_ADJ);

			// Array of neighboring points to connect Astar nodes
			var neighbours = new List<Vector2>
			{
				new Vector2(cell.x, cell.y - 1), // top cell
				new Vector2(cell.x - 1, cell.y), // left cell
			};

			foreach (var neighbour in neighbours)
			{
				if (tileMap.GetCellv(neighbour) == TileMap.InvalidCell)
					continue;
				_astar.ConnectPoints(i, _astar.GetClosestPoint(tileMap.MapToLocal(neighbour) + TILE_ADJ));
			}
		}
	}

	// Returns a path (array of points) from a point to another point
	public PackedVector2Array GetMovePath(Vector2 from, Vector2 to)
	{
		return _astar.GetPointPath(
			_astar.GetClosestPoint(from),
			_astar.GetClosestPoint(to)
		);
	}

	// Returns the closest point at the center of the grid cell for the provided point
	public Vector2 GetClosestGridPosition(Vector2 to)
	{
		return _astar.GetPointPosition(_astar.GetClosestPoint(to));
	}

	// Returns an array of all neighboring points within a given radius
	public PackedVector2Array GetPointsInRadius(Vector2 point, int radius)
	{
		PackedVector2Array points = new PackedVector2Array();
		var tileMapCell = _groundTileMap.LocalToMap(point);
		Vector2 TILE_ADJ = _groundTileMap.CellSize / 2;

		Dictionary<Vector2, bool> openedCells = new Dictionary<Vector2, bool>();
		List<Vector2> cellsToOpen = new List<Vector2> { tileMapCell };

		for (int i = 0; i < radius; i++)
		{
			List<Vector2> newCandidates = new List<Vector2>();

			foreach (var cellToOpen in cellsToOpen)
			{
				if (_groundTileMap.GetCellv(cellToOpen) == TileMap.InvalidCell || openedCells.ContainsKey(cellToOpen))
					continue;

				var neighbourCells = new List<Vector2>
				{
					new Vector2(cellToOpen.x - 1, cellToOpen.y),
					new Vector2(cellToOpen.x, cellToOpen.y - 1),
					new Vector2(cellToOpen.x + 1, cellToOpen.y),
					new Vector2(cellToOpen.x, cellToOpen.y + 1)
				};

				newCandidates.AddRange(neighbourCells);
				openedCells[cellToOpen] = true;
				points.Append(_groundTileMap.MapToLocal(cellToOpen) + TILE_ADJ);
			}

			cellsToOpen = newCandidates;
		}

		return points;
	}

	// Checks if two points are equal based on their grid cells
	public bool AreEqualCellPoints(Vector2 a, Vector2 b)
	{
		return _groundTileMap.LocalToMap(a) == _groundTileMap.LocalToMap(b);
	}
}

using Godot;
using System;
using System.Collections.Generic;

public class UnitManager : Node2D
{
	// Preload resources
	// private static readonly Texture PathTileTexture = (Texture)ResourceLoader.Load("res://assets/path-tile-16x16.png");
	private static readonly PackedScene PathAreaTileScene = (PackedScene)ResourceLoader.Load("res://core/path_area_tile/PathAreaTile.tscn");

	private PathFinder _pathFinder;
	private List<Node> _units;
	private Unit _selectedUnit;
	private Node2D _moveAreaContainer;

	public override void _Ready()
	{
		_pathFinder = new PathFinder(GetNode("Ground"));
		_units = new List<Node>(GetNode("Units").GetChildren());
		FixUnitPositions(_units);
		SubscribeToUnitSignals(_units);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouseEvent)
			return;
		if (mouseEvent.ButtonIndex != (int)ButtonList.Left || !mouseEvent.Pressed)
			return;

		if (_selectedUnit != null)
		{
			ClearMoveArea();
			Vector2 mousePosition = GetGlobalMousePosition();
			if (!IsUnitPoint(mousePosition))
			{
				MoveUnitToPoint(_selectedUnit, mousePosition);
			}
		}
	}

	// Adjusts unit positions to be centered in grid cells
	private void FixUnitPositions(List<Node> units)
	{
		foreach (var unit in units)
		{
			unit.Position = _pathFinder.GetClosestGridPosition(unit.Position);
		}
	}

	// Subscribes to unit signals
	private void SubscribeToUnitSignals(List<Node> units)
	{
		foreach (var unit in units)
		{
			(unit as Unit).Connect("left_clicked", this, nameof(HandleUnitSelection), new Array { unit });
			(unit as Unit).Connect("movement_done", this, nameof(HandleUnitMovementEnd), new Array { unit });
		}
	}

	// Handle unit selection
	private void HandleUnitSelection(Unit unit)
	{
		_selectedUnit = unit;
		ClearMoveArea();
		DrawMoveArea(_selectedUnit);
	}

	// Handle end of unit movement
	private void HandleUnitMovementEnd(Unit unit)
	{
		// Handle end of movement if needed
	}

	// Draws the unit's movement area
	private void DrawMoveArea(Unit unit)
	{
		_moveAreaContainer = new Node2D();
		var moveArea = _pathFinder.GetPointsInRadius(unit.Position, unit.MovePoints);
		foreach (var point in moveArea)
		{
			var pathAreaTile = (Node2D)PathAreaTileScene.Instance();
			// pathAreaTile.Sprite.Texture = PathTileTexture; // Use this if you want to apply the texture
			pathAreaTile.Position = point;
			_moveAreaContainer.AddChild(pathAreaTile);
		}
		AddChild(_moveAreaContainer);
	}

	// Clears the move area
	private void ClearMoveArea()
	{
		if (_moveAreaContainer != null)
		{
			RemoveChild(_moveAreaContainer);
			_moveAreaContainer.QueueFree();
			_moveAreaContainer = null;
		}
	}

	// Checks if the point is occupied by a unit
	private bool IsUnitPoint(Vector2 point)
	{
		foreach (var unit in _units)
		{
			if (_pathFinder.AreEqualCellPoints(point, unit.Position))
				return true;
		}
		return false;
	}

	// Moves the unit to a specified point
	private void MoveUnitToPoint(Unit unit, Vector2 point)
	{
		var path = _pathFinder.GetMovePath(unit.Position, point);
		unit.SetPath(path);
		_selectedUnit = null;
	}
}

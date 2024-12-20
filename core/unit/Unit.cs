using Godot;
using System;
using System.Collections.Generic;

public class Unit : CharacterBody2D
{
	[Signal]
	public delegate void MovementDone();
	[Signal]
	public delegate void LeftClicked();

	private const float SPEED = 40.0f;

	[Export]
	public int movePoints = 4;

	private PackedVector2Array path = new PackedVector2Array();

	[OnReady]
	private AnimatedSprite2D animatedSprite;

	public override void _Ready()
	{
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animatedSprite.Play("idle");
	}

	public override void _PhysicsProcess(float delta)
	{
		if (path.Count > 0)
		{
			animatedSprite.Play("run");
			_MoveAlongPath(delta);
			if (path.Count == 0)
			{
				animatedSprite.FlipH = false;
				EmitSignal(nameof(MovementDone));
			}
		}
		else
		{
			animatedSprite.Play("idle");
		}
	}

	public void SetPath(PackedVector2Array _path)
	{
		path = _path;
	}

	private void _MoveAlongPath(float delta)
	{
		float distance = SPEED * delta;
		_RotateToPoint(path[path.Count - 1]);

		for (int i = 0; i < path.Count; i++)
		{
			var nextPoint = path[0];
			var distToNextPoint = Position.DistanceTo(nextPoint);

			if (distance <= distToNextPoint && distance >= 0.0f)
			{
				SetVelocity(Position.DirectionTo(nextPoint).Normalized() * SPEED);
				MoveAndSlide();
				break;
			}
			distance -= distToNextPoint;
			SetVelocity(Position.DirectionTo(nextPoint).Normalized() * SPEED);
			MoveAndSlide();
			path.RemoveAt(0);
		}
	}

	private void _RotateToPoint(Vector2 point)
	{
		animatedSprite.FlipH = Position.x - point.x > 2;
	}

	public void _HandleInputEvent(SubViewport viewport, InputEvent event, int shapeIdx)
	{
		if (event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == (int)ButtonList.Left)
		{
			EmitSignal(nameof(LeftClicked));
		}
	}
}

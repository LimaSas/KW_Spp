using Godot;
using System;

public class MySprite : Sprite2D
{
	private AnimationPlayer _animationPlayer;

	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_animationPlayer.Play("appear");
	}

	// Handles mouse enter event
	public void HandleMouseEntered()
	{
		if (_animationPlayer.CurrentAnimation == "appear")
			return;
		_animationPlayer.Play("highlight");
	}

	// Handles mouse exit event
	public void HandleMouseExited()
	{
		if (_animationPlayer.CurrentAnimation == "appear")
			return;
		_animationPlayer.Play("back_to_normal");
	}
}

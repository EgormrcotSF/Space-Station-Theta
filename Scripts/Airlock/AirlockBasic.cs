using Godot;
using System;
using System.ComponentModel;

public partial class AirlockBasic : StaticBody3D
{
	private CollisionShape3D Collision;
	private AnimatedSprite3D Sprite;
	private Timer CloseTimer;

	private bool Open = false;

	public override void _Ready()
	{
		Collision = GetNode<CollisionShape3D>("CollisionShape3D");
		Sprite = GetNode<AnimatedSprite3D>("AnimatedSprite3D");
		CloseTimer = GetNode<Timer>("CloseTimer");
	}

	private void BodyEnteredHitbox(Node3D Body)
	{
		if (Body.IsInGroup("Humanoid") & Open == false)
		{
			Open = true;
			Sprite.Play("Open");
			Collision.SetDeferred("disabled", true);
			CloseTimer.Start();
		}
	}

	private void CloseTimerTimeout()
	{
		if (Open)
		{
			Open = false;
			Sprite.PlayBackwards("Open");
			Collision.SetDeferred("disabled", false);
		}
	}
}

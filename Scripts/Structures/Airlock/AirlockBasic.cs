//open-source EULA/CLA, see full text in LICENSE.txt
using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class AirlockBasic : StaticBody3D, Interaction
{
	private CollisionShape3D Collision;
	private AnimatedSprite3D Sprite;
	private Timer CloseTimer;
	[Export] private Timer OpenTimer;

	private bool Open = false;
	private bool AirlockAction = false;

	public override void _Ready()
	{
		Collision = GetNode<CollisionShape3D>("CollisionShape3D");
		Sprite = GetNode<AnimatedSprite3D>("AnimatedSprite3D");
		CloseTimer = GetNode<Timer>("CloseTimer");
	}

	public void Interact()
	{
		//This method calls a method with RPC (yes, this is kinda crutch but idk how do i should do normally)
		Rpc("RpcInteract");
	}

	public void PickUp(CharacterBody3D Picker)
	{
		//This method calls a method with RPC (yes, this is kinda crutch but idk how do i should do normally)
		Rpc("RpcInteract");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RpcInteract()
	{
		if (Open)
		{
			CloseAirlock();
		}
		else
		{
			OpenAirlock();
		}
	}

	private void BodyEnteredHitbox(Node3D Body)
	{
		//if a humanoid enters hitbox, door opens
		if (Body.IsInGroup("Humanoid"))
		{
			OpenAirlock();
		}
	}

	private void CloseTimerTimeout()
	{
		//close door when timer ends
		CloseAirlock();
	}

	private void OpenTimerTimeout()
	{
		AirlockAction = false;
		if (Open)
		{
			Collision.SetDeferred("disabled", true);
		}
		else
		{
			Collision.SetDeferred("disabled", false);
			//start door close timer
			CloseTimer.Start();
		}
	}

	private void OpenAirlock()
	{
		if (!Open && !AirlockAction)
		{
			Open = true;
			Sprite.Play("Open");
			AirlockAction = true;
			OpenTimer.Start();
		}
	}

	private void CloseAirlock()
	{
		if (Open && !AirlockAction)
		{
			Open = false;
			Sprite.PlayBackwards("Open");
			AirlockAction = true;
			OpenTimer.Start();
			//if player interacted door before timer ends, timer stops
			CloseTimer.Stop();
		}
	}
}

using Godot;
using System;

public partial class TestItem : RigidBody3D, IPickable
{
	//public void TakeObject(CharacterBody3D Taker)
	//{
	//	Rpc("RpcTakeObject", Taker);
	//}

	//[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	//public void RpcTakeObject(CharacterBody3D Taker)
	//{
	//	Taker.Call("ReturnTakingItem");
	//	QueueFree();
	//}
	public void PickUp(CharacterBody3D Picker)
	{
		//This method calls a method with RPC (yes, this is kinda crutch but idk how do i should do normally)
		Rpc("RpcPickup");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RpcPickup()
	{
		QueueFree();
	}
}

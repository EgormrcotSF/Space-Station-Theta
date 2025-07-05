using Godot;
using System;

public partial class TestItem : RigidBody3D, IPickable
{
	//public void TakeObject(CharacterBody3D Taker)
	//{
		//This method calls a method with RPC (yes, this is kinda crutch but idk how do i should do normally)
	//	Rpc("RpcTakeObject", Taker);
	//}

	//[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	//public void RpcTakeObject(CharacterBody3D Taker)
	//{
	//	Taker.Call("ReturnTakingItem");
	//	QueueFree();
	//}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void PickUp(CharacterBody3D Picker)
	{
		QueueFree();
	}
}

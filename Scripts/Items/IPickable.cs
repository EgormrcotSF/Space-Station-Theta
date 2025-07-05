//open-source EULA/CLA, see full text in LICENSE.txt
using Godot;
using System;

public interface IPickable
{
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	void PickUp(CharacterBody3D Picker);
}

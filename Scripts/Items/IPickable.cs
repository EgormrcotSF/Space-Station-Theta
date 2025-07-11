//open-source EULA/CLA, see full text in LICENSE.txt
using Godot;
using System;

public interface IPickable
{
	void PickUp(CharacterBody3D Picker);
}

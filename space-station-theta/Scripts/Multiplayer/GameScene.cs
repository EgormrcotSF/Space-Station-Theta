using Godot;
using System;

public partial class GameScene : Node3D
{
	[Export] private PackedScene PlayerScene;

	public override void _Ready()
	{
		int index = 0;
		foreach (var item in GameManager.Players)
		{
			PlayerHumanoidTest CurrentPlayer = PlayerScene.Instantiate<PlayerHumanoidTest>();
			CurrentPlayer.Name = item.Id.ToString();
			AddChild(CurrentPlayer);
			foreach (Node3D SpawnPoint in GetTree().GetNodesInGroup("PlayerSpawnPoints"))
			{
				if (int.Parse(SpawnPoint.Name) == index)
				{
					CurrentPlayer.GlobalPosition = SpawnPoint.GlobalPosition;
				}
			}
			index++;
		}
	}
}

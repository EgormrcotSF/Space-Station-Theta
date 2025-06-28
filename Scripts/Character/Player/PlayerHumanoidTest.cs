//open-source EULA/CLA, see full text in SST EULA.txt
using Godot;
using System;

public partial class PlayerHumanoidTest : CharacterBody3D
{
	private Camera3D Camera;
	private RayCast3D InteractRay;

	public float Speed = 2.7f;
	public const float MouseSensitivity = 0.002f;

	//for smooth movement
	//private Vector3 SyncPosition = new Vector3(0, 0, 0);

	public override void _Ready()
	{
		InteractRay = GetNode<RayCast3D>("Camera3D/InteractRay");
		Camera = GetNode<Camera3D>("Camera3D");
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
		{
			Camera.MakeCurrent();
		}

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
		{
			if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				Camera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
				Camera.Rotation = new Vector3
				(
					Mathf.Clamp(Camera.Rotation.X, Mathf.DegToRad(-90), Mathf.DegToRad(90)),
					Camera.Rotation.Y,
					Camera.Rotation.Z
				);
				RotateY(-mouseMotion.Relative.X * MouseSensitivity);
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		//If this is a player that controls this character, he will control this character (this code runs).
		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
		{
			Vector3 velocity = Velocity;

			// Add the gravity.
			if (!IsOnFloor())
			{
				velocity += GetGravity() * (float)delta;
			}

			if (Input.IsActionJustPressed("interact"))
			{
				//i was added a InteractObject != null because i dont want to get a thousands of errors in output
				var InteractObject = InteractRay.GetCollider();
				if (InteractObject != null)
				{
					if (InteractObject.HasMethod("Interact"))
					{
						//when player interacts with interactible object, Interact method runs.
						InteractObject.Call("Interact");
					}
				}
			}
			//make the mouse free when player hold alt
			if (Input.IsActionJustPressed("mouse_free"))
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}

			if (Input.IsActionJustReleased("mouse_free"))
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}

			Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
			Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
			}

			Velocity = velocity;
			MoveAndSlide();
			//SyncPosition = GlobalPosition;
		}
		else
		{
			//Smooth movement without sending extra packets
			//GlobalPosition = SyncPosition.Lerp(SyncPosition, .1f);
		}
	}
}

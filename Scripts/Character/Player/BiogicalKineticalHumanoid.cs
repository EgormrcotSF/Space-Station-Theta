//open-source EULA/CLA, see full text in LICENSE.txt
using Godot;
using System;

public partial class BiogicalKineticalHumanoid : CharacterBody3D
{
	//Node variables
	[Export] private Camera3D Camera;
	[Export] private RayCast3D InteractRay;
	[Export] private MeshInstance3D PlayerSprite;
	[Export] private LineEdit ChatLineEdit;
	[Export] private TextEdit ChatText;
	[Export] private MultiplayerSynchronizer LocalSynchronizer;

	//Physical characteristics
	public float Speed = 2.7f;
	public const float MouseSensitivity = 0.002f;

	//Misc variables
	//If true - disables most of controls
	private bool ControlsDisabled = false;
	//Smooth movement
	private Vector3 SyncPosition = new Vector3(0, 0, 0);
	//Does this player controls this character?
	private bool Authority;

	public override void _Ready()
	{
		LocalSynchronizer.SetMultiplayerAuthority(int.Parse(Name));
		Authority = LocalSynchronizer.GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
		if (Authority)
		{
			Camera.MakeCurrent();
			PlayerSprite.Hide();
		}

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (Authority)
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
			//When player press T, this thing force focus for chat LineEdit
			if (@event.IsActionPressed("OpenChat"))
			{
				if (!ChatLineEdit.HasFocus())
				{
					ControlsDisabled = true;
					ChatLineEdit.GrabFocus();
				}
			}
			//Escape system
			if (@event.IsActionPressed("Escape"))
			{
				if (ChatLineEdit.HasFocus())
				{
					ChatLineEdit.ReleaseFocus();
					ControlsDisabled = false;
				}
				else
				{
					GetTree().Quit();
				}
			}
			//Make the mouse free when player hold alt
			if (@event.IsActionPressed("MouseFree"))
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
			if (@event.IsActionReleased("MouseFree"))
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			//These controls are disabled if ControlsDisabled varible is true
			if (!ControlsDisabled)
			{
				if (Input.IsActionJustPressed("Interact"))
				{
					var InteractCollider = InteractRay.GetCollider();
					if (InteractCollider is Interaction InteractibleItem)
					{
						InteractibleItem.Interact();
					}
				}

				if (Input.IsActionJustPressed("TakeItem"))
				{
					var TakeCollider = InteractRay.GetCollider();
					if (TakeCollider is Interaction PickableItem)
					{
						PickableItem.PickUp(this);
					}
				}
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		//If this is a player that controls this character, he will control this character (this code runs).
		if (Authority)
		{
			Vector3 velocity = Velocity;
			//Add the gravity.
			if (!IsOnFloor())
			{
				velocity += GetGravity() * (float)delta;
			}
			//If ChatLineEdit have focus, controls disables.
			if (ChatLineEdit.HasFocus())
			{
				ControlsDisabled = true;
			}
			//These controls are disabled if ControlsDisabled varible is true
			if (!ControlsDisabled)
			{
				Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBack");
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
			}
			Velocity = velocity;
			MoveAndSlide();
			SyncPosition = GlobalPosition;
		}
		else
		{
			//Smooth movement without sending extra packets
			GlobalPosition = GlobalPosition.Lerp(SyncPosition, .1f);
		}
	}

	//Attached godot events
	public void ChatTextSubmitted(string SubmittedText)
	{
		ChatLineEdit.Clear();
		ControlsDisabled = false;
		ChatLineEdit.ReleaseFocus();
		ChatText.Text += SubmittedText;
	}
}

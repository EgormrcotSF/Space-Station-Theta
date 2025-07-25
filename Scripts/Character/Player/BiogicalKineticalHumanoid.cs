//open-source EULA/CLA, see full text in LICENSE.txt
using Godot;
using System;

public partial class BiogicalKineticalHumanoid : CharacterBody3D
{
	//Node variables
	private Camera3D Camera;
	private RayCast3D InteractRay;
	private MeshInstance3D PlayerSprite;
	private LineEdit ChatLineEdit;
	private TextEdit ChatText;

	//Physical characteristics
	public float Speed = 2.7f;
	public const float MouseSensitivity = 0.002f;

	//Misc variables
	//If true - disables most of controls
	private bool ControlsDisabled = false;
	//Smooth movement
	private Vector3 SyncPosition = new Vector3(0, 0, 0);

	public override void _Ready()
	{
		PlayerSprite = GetNode<MeshInstance3D>("MeshInstance3D");
		InteractRay = GetNode<RayCast3D>("Camera3D/InteractRay");
		Camera = GetNode<Camera3D>("Camera3D");
		ChatLineEdit = GetNode<LineEdit>("CanvasLayer/Chat/ChatLineEdit");
		ChatText = GetNode<TextEdit>("CanvasLayer/Chat/ChatText");
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
		if (GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
		{
			Camera.MakeCurrent();
			PlayerSprite.Hide();
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

			//Add the gravity.
			if (!IsOnFloor())
			{
				velocity += GetGravity() * (float)delta;
			}
			//Chat module
			if (ChatLineEdit.HasFocus())
			{
				ControlsDisabled = true;
			}
			//When player press T, this thing force focus for chat LineEdit
			if (Input.IsActionJustPressed("open_chat"))
			{
				if (!ChatLineEdit.HasFocus())
				{
					ControlsDisabled = true;
					ChatLineEdit.GrabFocus();
				}
			}

			//Make the mouse free when player hold alt
			if (Input.IsActionJustPressed("mouse_free"))
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}

			if (Input.IsActionJustReleased("mouse_free"))
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			//These controls are disabled if ControlsDisabled varible is true
			if (!ControlsDisabled)
			{
				if (Input.IsActionJustPressed("interact"))
				{
					var InteractCollider = InteractRay.GetCollider();
					if (InteractCollider is Interaction InteractibleItem)
					{
						InteractibleItem.Interact();
					}
				}

				if (Input.IsActionJustPressed("takeitem"))
				{
					var TakeCollider = InteractRay.GetCollider();
					if (TakeCollider is Interaction PickableItem)
					{
						PickableItem.PickUp(this);
					}
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

	public void ChatLineEditFocusExited()
	{
		ControlsDisabled = false;
	}
}

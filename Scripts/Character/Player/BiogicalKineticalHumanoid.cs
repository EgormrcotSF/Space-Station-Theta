//open-source EULA/CLA, see full text in LICENSE.txt
using Godot;
using System;

public partial class BiogicalKineticalHumanoid : CharacterBody3D
{
	//Export node variables
	[Export] private Camera3D Camera;
	[Export] private RayCast3D InteractRay;
	[Export] private MeshInstance3D PlayerSprite;
	[Export] private LineEdit ChatLineEdit;
	[Export] private TextEdit ChatText;
	[Export] private MultiplayerSynchronizer LocalSynchronizer;
	[Export] private CanvasLayer UICanvasLayer;
	[Export] private Label3D PopupExternal;
	[Export] private Timer PopupExternalHideTimer;
	[Export] private Timer RefreshDelay;

	//Physical characteristics
	public float Speed = 2.7f;
	public const float MouseSensitivity = 0.002f;

	//Popup constants
	private const int PopupExternalHideDelayChat = 5;

	//Misc variables
	//If true - disables most of controls
	private bool ControlsDisabled = false;
	//Smooth movement
	private Vector3 SyncPosition = new Vector3(0, 0, 0);
	//Does this player controls this character?
	private bool Authority;
	//Character what player controls, empty if Authority = true
	private Node AuthorityPlayerCharacter;

	public override void _Ready()
	{
		LocalSynchronizer.SetMultiplayerAuthority(int.Parse(Name));
		Authority = LocalSynchronizer.GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
		if (Authority)
		{
			AddToGroup("AuthorityPlayerCharacter");
			Camera.MakeCurrent();
			PlayerSprite.Hide();
		}
		else
		{
			UICanvasLayer.QueueFree();
			AuthorityPlayerCharacter = GetTree().GetFirstNodeInGroup("AuthorityPlayerCharacter");
			if (AuthorityPlayerCharacter == null)
			{
				RefreshDelay.Start();
			}
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
	//Chat
	public void ChatTextSubmitted(string SubmittedText)
	{
		ChatLineEdit.Clear();
		ControlsDisabled = false;
		ChatLineEdit.ReleaseFocus();
		Rpc("RpcChatTextSubmitteed", SubmittedText);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RpcChatTextSubmitteed(string SentText)
	{
		if (Authority)
		{
			ChatText.Text += SentText;
		}
		else
		{
			PopupExternalHideTimer.Stop();
			PopupExternal.Text = SentText;
			PopupExternalHideTimer.Start(PopupExternalHideDelayChat);
			AuthorityPlayerCharacter.Call("ToAuthorityChatTextSubmitteed", SentText);
		}
	}

	public void ToAuthorityChatTextSubmitteed(string SentText)
	{
		ChatText.Text += SentText;
	}

	//Popup
	public void PopupExternalHideTimerTimeout()
	{
		PopupExternal.Text = "";
	}

	//Refresh Delay
	public void RefreshDelayTimeout()
	{
		AuthorityPlayerCharacter = GetTree().GetFirstNodeInGroup("AuthorityPlayerCharacter");
		if (AuthorityPlayerCharacter == null)
		{
			RefreshDelay.Start();
		}
	}
}
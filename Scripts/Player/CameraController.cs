using Godot;
using System;
using System.Dynamic;


public partial class CameraController : Node3D
{
	[Export] public float CameraDelta {get; private set;} = 2f;
	[Export] public float MouseSensitivity { get; private set; } = 0.001f;
	[Export] public float JoyPadSensitivity {get; private set;} = 0.01f;
	[Export] public float MaxCameraArmLength{get; private set;} = 20f;
	[Export] public float MinCameraArmLength{get; private set;} = 3f;
	[Export] public float LookAheadRange {get; private set;} = 20f;
	[Export] public float CameraDeadZoneY {get; private set;} = 10f;
	Player _Target;
	float _yawInput = 0.0f;
	float _pitchInput = 0.0f;
	float _currentCameraArmLength;
	Node3D _Camera;
	float _CameraYPivot;
	Node3D _YawPivot;
	Node3D _PitchPivot;
	Godot.Collections.Dictionary _CameraObstacle;

	public Godot.Collections.Dictionary GetCameraObstacle(){
		return _CameraObstacle;
	}

	public Basis GetTwistPivotBasis()
	{
		return _YawPivot.Basis;
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_Camera = GetNode<Camera3D>("YawPivot/PitchPivot/Camera3D");

		_YawPivot = GetNode("YawPivot") as Node3D;
		_PitchPivot = GetNode("YawPivot/PitchPivot") as Node3D;

		_Target = GetParent().GetNode("Player") as Player;

		_currentCameraArmLength = _Camera.Position.Z;

	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		HandleRightJoystick();

        _YawPivot.RotateY(_yawInput * (float)delta);
		_PitchPivot.RotateX(_pitchInput * (float)delta);

		_PitchPivot.Rotation = new Vector3(Math.Clamp(_PitchPivot.Rotation.X, -1.50f, 1.0f), // radians
											_PitchPivot.Rotation.Y,
											_PitchPivot.Rotation.Z
											);

		_yawInput = 0;
		_pitchInput = 0;
	}

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

		SetCameraObstacle();
		MoveCameraPivot();
		AdjustCameraArm();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
		
		if (@event is InputEventMouseMotion eventKey)
		{
			if(Input.MouseMode == Input.MouseModeEnum.Captured)
			{
				_yawInput -= eventKey.Relative.X * MouseSensitivity;
				_pitchInput -= eventKey.Relative.Y * MouseSensitivity;
			}
		}
    }

	private void MoveCameraPivot(){

		Vector3 nextPosition = _Target.GlobalPosition + _Target.GetHorizontalVelocity()/LookAheadRange;
		nextPosition.Y -= _Target.LinearVelocity.Y/(LookAheadRange*3f);

		GlobalPosition = new Vector3(Mathf.Lerp(GlobalPosition.X, nextPosition.X, .4f),
									 Mathf.Lerp(GlobalPosition.Y, nextPosition.Y, .4f),
									 Mathf.Lerp(GlobalPosition.Z, nextPosition.Z, .4f));
		

	}

	
	public bool CheckForCollisionBetween(Vector3 start, Vector3 end){

		var directState = GetWorld3D().DirectSpaceState;
		var rayInfo = PhysicsRayQueryParameters3D.Create(start, end);

		var col = directState.IntersectRay(rayInfo);

		if(col.Count > 0) return true;
		return false;

	}

	private void AdjustCameraArm(){
		
		if (_CameraObstacle.Count > 0){

			_CameraObstacle.TryGetValue("collider", out var collider);
			if(collider.As<Node3D>().IsInGroup("Player")) return;

			_CameraObstacle.TryGetValue("position", out var position);

			float distanceToCollision = (GlobalPosition - (Vector3)position).Length();

			float w = 0.2f;
			_Camera.Position = 
			new Vector3(0f,
						0f,
						Mathf.Lerp(_Camera.Position.Z, distanceToCollision, w));
		}
		else{

			float cameraDelta = 0f;
			if (Input.IsActionJustPressed("Mouse_Wheel_Up")) cameraDelta = -CameraDelta; 
			else if (Input.IsActionJustPressed("Mouse_Wheel_Down")) cameraDelta = CameraDelta;

			_currentCameraArmLength = Mathf.Clamp(_currentCameraArmLength + cameraDelta, MinCameraArmLength, MaxCameraArmLength);

			var currentCameraLocalPos = _Camera.Position;


			if(currentCameraLocalPos.Z == _currentCameraArmLength) return;

			var nextCameraLocalPos = new Vector3(_Camera.Position.X, _Camera.Position.Y,
				Mathf.Lerp(currentCameraLocalPos.Z, _currentCameraArmLength, .05f));

			_Camera.Position = nextCameraLocalPos;

			if(CheckForCollisionBetween(GlobalPosition, _Camera.GlobalPosition)){
				_Camera.Position = currentCameraLocalPos;
			}
		}

	}


	public void SetCameraYPivot(){

		_CameraYPivot = GlobalPosition.Y + 1f;

	}

	public void SetCameraObstacle(){

		var directState = GetWorld3D().DirectSpaceState;
		var rayInfo = PhysicsRayQueryParameters3D.Create(GlobalPosition, _Camera.GlobalPosition);
		_CameraObstacle = directState.IntersectRay(rayInfo);
	}

	private void HandleRightJoystick(){

		Vector2 RightJoyStickAxis = new (Input.GetAxis("Joypad_R_Left", "Joypad_R_Right"), Input.GetAxis("Joypad_R_Down", "Joypad_R_Up"));

		_yawInput -= RightJoyStickAxis.X * JoyPadSensitivity;
		_pitchInput += RightJoyStickAxis.Y * JoyPadSensitivity;

	}

}

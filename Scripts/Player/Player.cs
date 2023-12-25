using Godot;
using System;
using PlayerNS;


public partial class Player : RigidBody3D
{
	[Export] public float MovementSpeed { get; private set; } = 1400.0f;
	[Export] public float RunSteerWeight {get; private set;} = 1.0f;
	[Export] public float JumpForce { get; private set; } = 25.0f;
	public Node3D _YawPivot {get; private set;}

	public AnimationNodeStateMachinePlayback _AnimationStateMachine {get; private set;}
	public AnimationTree _AnimationTree {get; private set;}
	Node3D _PlayerMesh;
	State _State;
	public PhysicsMaterial _NoFriction {get; private set;}
	public PhysicsMaterial _NormalFriction {get; private set;}
	public PhysicsMaterial _HighFriction {get; private set;}

#region Getters

	public State GetState(){
		return _State;
	}
	public Basis GetYawPivotBasis(){
		return _YawPivot.Basis;
	}
	public Vector3 GetMovementInput()
	{
		return new Vector3(
            Input.GetAxis("Move_Left", "Move_Right"),
            0,
            Input.GetAxis("Move_Foward", "Move_Back")
            );
    }
	public Vector3 GetRunForce(float delta)
	{
		Vector3 desireDirection = (_YawPivot.Basis * GetMovementInput()).Normalized();
		Vector3 currentDirection = GetHorizontalVelocity().Normalized();

		float length = GetMovementInput().Length();
		float runSteerWeight = RunSteerWeight;

		if(desireDirection.Dot(currentDirection) < -0.5f){
			runSteerWeight *= 3;
		}

		if(length > 1){
	        return GetMovementInput().Normalized().Length() * MovementSpeed * Mass * delta * (_YawPivot.Basis * GetMovementInput().Normalized() * runSteerWeight + GetHorizontalVelocity().Normalized() ).Normalized();
		}
		else 
			return length * MovementSpeed * Mass * delta * (_YawPivot.Basis * GetMovementInput().Normalized() * runSteerWeight + GetHorizontalVelocity().Normalized() ).Normalized();
    }
	public Vector3 GetRunForceNoSteer(float delta)
	{
        return GetMovementInput().Normalized().Length() * MovementSpeed * Mass * delta * (_YawPivot.Basis * GetMovementInput().Normalized()).Normalized();
    }
	public Vector3 GetAirborneHorizontalForce(float delta)
	{
		Vector3 AirborneHorizontalForce = MovementSpeed * Mass * delta * (_YawPivot.Basis * GetMovementInput().Normalized());

		AirborneHorizontalForce.Y = 0.0f;

        return AirborneHorizontalForce * 2.0f;
    }
	public Vector3 GetJumpForce()
	{
		return Vector3.Up * JumpForce * Mass;
	}
	public Node3D GetPlayerMesh()
	{
		return _PlayerMesh;
	}
	public Vector3 GetPosition()
	{
		return GlobalPosition - new Vector3(0f, 1f, 0f);
	}
	public Vector3 GetHorizontalVelocity()
	{
		return new Vector3(LinearVelocity.X, 0f, LinearVelocity.Z) ;
	}
#endregion

#region Setters
	public void SetNoFrictionMaterial(){
		PhysicsMaterialOverride = _NoFriction;
	}
	public void SetNormalMaterial()
	{
		PhysicsMaterialOverride = _NormalFriction;
	}
	public void SetHighFrictionMaterial(){
		PhysicsMaterialOverride = _HighFriction;
	}

#endregion

    public override void _Ready()
	{
		//_PlayerMesh = GetNode("PlayerVox") as Node3D;
		
		_PlayerMesh = GetNode("GobotSkin") as Node3D;

		//_AnimPlayer = GetNode("PlayerVox/AnimationPlayer") as AnimationPlayer;
		//_AnimPlayer = GetNode("GobotSkin/gobot/AnimationPlayer") as AnimationPlayer;
		_AnimationTree = GetNode("GobotSkin/AnimationTree") as AnimationTree;
		_AnimationStateMachine = (AnimationNodeStateMachinePlayback)_AnimationTree.Get("parameters/StateMachine/playback");

		_NoFriction = GD.Load<PhysicsMaterial>("res://PhysicsMaterials/Player_NoFriction.tres");
		_NormalFriction = GD.Load<PhysicsMaterial>("res://PhysicsMaterials/Player_Normal.tres");

		Input.MouseMode = Input.MouseModeEnum.Captured;

		BodyEntered += OnBodyEntered;

		_YawPivot = (Node3D)GetParent().GetNode("CameraController/YawPivot");

		_State = States.Grounded;
		_State.Start(this);

    }
	public override void _Process(double delta)
	{
		///////////////
		///Mouse
		if(Input.MouseMode == Input.MouseModeEnum.Captured)
		{
            if (Input.IsActionJustPressed("ui_cancel"))
                Input.MouseMode = Input.MouseModeEnum.Visible;
        }
		else if(Input.MouseMode == Input.MouseModeEnum.Visible)
		{
			if(Input.IsActionJustPressed("ui_cancel"))
				Input.MouseMode= Input.MouseModeEnum.Captured;
		}

        //Move Player
        _State.Update(this, (float)delta);

		GD.Print(_State);

    }
	public override void _PhysicsProcess(double delta)
    {
		base._PhysicsProcess(delta);

		_State.PhysicsUpdate(this, (float)delta);
    }

    public void StateTransition(State nextState)
	{
		//_state.End(this);
		_State = nextState;
		_State.Start(this);
	}

	public void OnBodyEntered(Node body){
		
	}
}

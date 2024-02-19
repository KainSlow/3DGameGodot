using Godot;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace PlayerNS //player namespace
{
    public class State
    {
        public virtual void Start(Player player)
        {

        }
    
        public virtual void Update(Player player, float delta)
        {

        }

        public virtual void PhysicsUpdate(Player player, float delta) 
        {

        }

        protected static bool IsTryingToMove(Player player)
        {
            return !player.GetMovementInput().IsZeroApprox();
        }

        protected static bool IsTryingToJump()
        {
            return Input.GetActionRawStrength("Jump") >= 0.5f;
        }

        protected static bool IsTryingToRun()
        {
            if (Input.GetActionRawStrength("Run") == 0) return false;

            return true;
        }

        protected static bool IsGrounded(Player player){

            var directState = player.GetWorld3D().DirectSpaceState;
            var meshBasis = player.GetPlayerMesh().Basis;

            float scale = .4f;
            Vector3 downVector = Vector3.Down * .45f;

            Vector3 globalPosition = player.GlobalPosition + (meshBasis * Vector3.Back).Normalized()* .2f ;
            globalPosition.Y -= 0.7f;

            Vector3 leftForwardOrigin = globalPosition + meshBasis * (Vector3.Left + Vector3.Forward)  * scale;
            Vector3 rightForwardOrigin = globalPosition + meshBasis * (Vector3.Right + Vector3.Forward)  * scale;
            Vector3 leftBackOrigin = globalPosition +  meshBasis * (Vector3.Left + Vector3.Back)  * scale;
            Vector3 rightBackOrigin = globalPosition + meshBasis * (Vector3.Right + Vector3.Back)  * scale;

            PhysicsRayQueryParameters3D [] raycastsInfos = new PhysicsRayQueryParameters3D[]{
                PhysicsRayQueryParameters3D.Create(leftForwardOrigin, leftForwardOrigin + downVector),
                PhysicsRayQueryParameters3D.Create(rightForwardOrigin, rightForwardOrigin + downVector),
                PhysicsRayQueryParameters3D.Create(leftBackOrigin, leftBackOrigin + downVector),
                PhysicsRayQueryParameters3D.Create(rightBackOrigin, rightBackOrigin + downVector),
                PhysicsRayQueryParameters3D.Create(globalPosition, globalPosition + downVector)
            }; 


            foreach(var rayInfo in raycastsInfos){

                var collision = directState.IntersectRay(rayInfo);
                if(collision.Count > 0){
                    collision.TryGetValue("collider", out var col);
                    Node3D collider = (Node3D)col;
                    if(collider.IsInGroup("Ground") || collider.IsInGroup("Platform")){
                        return true;
                    }
                }
            }

            return false;
        }

    }
    public static class States
    {
        public static GroundState Grounded = new();
        public static IdleState Idle = new();
        public static RunningState Running = new ();
        public static JumpingState Jumping = new ();
        public static FalllingState Falling = new();
        public static LedgeGrabState LedgeGrabbing = new();
    }
    public class GroundState : State
    {
        public override void Start(Player player)
        {
            player.GravityScale = 3.0f;
            player.SetNormalMaterial();

            if (IsTryingToMove(player))
            {
                player.StateTransition(States.Running);
                return;
            }

            player.StateTransition(States.Idle);
        }

        public override void Update(Player player, float delta)
        {
            Vector3 at = player.GetPlayerMesh().GlobalPosition + player.GetHorizontalVelocity();
            if(player.GetHorizontalVelocity().Length() >= .2f)
                player.GetPlayerMesh().LookAt(at);
        }

        public override void PhysicsUpdate(Player player, float delta)
        {
            if (IsGrounded(player))
            {
                if(IsTryingToJump()) 
                {
                    player.StateTransition(States.Jumping);
                }
            }
            else
            {
                player._AnimationStateMachine.Travel("Fall");
                player.StateTransition(States.Falling);
            }
        }
    }
    public class AirborneState : State
    {
        public override void Start(Player player)
        {
            player.LinearDamp = 0.0f;
            player.SetNoFrictionMaterial();
        }
        public override void PhysicsUpdate(Player player, float delta)
        {
            if (IsTryingToMove(player))
            {
                player.ApplyCentralForce( player.MovementSpeed * (float)delta  * 50f * (player.GetYawPivotBasis() * player.GetMovementInput().Normalized()));
            }
        }
    }
    public class LedgeGrabState : State{

        public override void Start(Player player)
        {
            player._AnimationStateMachine.Start("EdgeGrab");
            player.GravityScale = 0f;
            player.LinearVelocity = Vector3.Zero;
        }

        public override void Update(Player player, float delta)
        {
            if(Input.IsActionJustPressed("Jump")){
                player.StateTransition(States.Jumping);
            }

            if(IsTryingToMove(player)){

                Vector3 desireDirection = (player.GetYawPivotBasis() * player.GetMovementInput()).Normalized();
                Vector3 currentDirection = (player.GetPlayerMesh().Basis * Vector3.Forward).Normalized();

                if(desireDirection.Dot(currentDirection) < -0.25f){
                    
                    player.GetPlayerMesh().LookAt(player.GetPosition() + (player.GetPlayerMesh().Basis * Vector3.Back));
                    player.StateTransition(States.Falling);
                }
            }

        }

    }
    public class IdleState : GroundState
    {
        public override void Start(Player player)
        {   
            player._AnimationStateMachine.Start("Idle");
            player.SetHighFrictionMaterial();
            player.LinearDamp = 7f;
        }

        public override void Update(Player player, float delta)
        {
            //base.Update(player, delta);

            if (IsTryingToMove(player)) //if moving
            {
                player._AnimationStateMachine.Travel("Run");
                player.StateTransition(States.Running); 
                return;
            }
        }
    }
    public class RunningState : GroundState
    {
        public override void Start(Player player)
        {   
            if(player._AnimationStateMachine.GetCurrentNode() != "Run")
                player._AnimationStateMachine.Start("Run");
            player.LinearDamp = 4f;
            player.SetNoFrictionMaterial();
        }

        public override void Update(Player player, float delta)
        {
            base.Update(player, delta);

            player._AnimationTree.Set("parameters/StateMachine/Run/TimeScale/scale", (player.GetMovementInput().Length() > 1f)? player.GetMovementInput().Normalized().Length(): player.GetMovementInput().Length() );

            if (!IsTryingToMove(player))
            {
                player.StateTransition(States.Idle);
                return;
            }
        }

        public override void PhysicsUpdate(Player player, float delta)
        {
            base.PhysicsUpdate(player, delta);

            if(player.LinearVelocity.LengthSquared() > 2f)
                player.ApplyCentralForce(player.GetRunForce(delta));
            else
                player.ApplyCentralForce(player.GetRunForceNoSteer(delta));
        }
    }
    public class JumpingState : AirborneState
    {
        float currentJumpTime = 0f;
        public override void Start(Player player)
        {
            currentJumpTime = 0f;
            player.SetNoFrictionMaterial();
            player.GravityScale = 1.0f;
            player.LinearVelocity -= new Vector3(0.0f, player.LinearVelocity.Y, 0.0f);
            player.LinearDamp = 4f;
            player.ApplyCentralImpulse(player.GetJumpForce());
            player._AnimationStateMachine.Travel("Jump");
        }

        public override void Update(Player player, float delta)
        {
            currentJumpTime += delta;

            if(currentJumpTime >= player.MaxJumpTime || !IsTryingToJump())
            {
                player._AnimationStateMachine.Travel("Fall");
                player.StateTransition(States.Falling);
                return;
            }

        }
    }
    public class FalllingState : AirborneState
    {
        private static async void WaitForSeconds(Player player, float milliseconds){
            
            var initialState = player.GetState();
            var initialVelocity = player.LinearVelocity.Y;
            var initialPosition = player.GlobalPosition;
            await Task.Delay(TimeSpan.FromMilliseconds(milliseconds));
            var finalState = player.GetState();
            var finalVelocity = player.LinearVelocity.Y;
            var finalPosition = player.GlobalPosition;

            if(initialState == finalState){
                
                WaitForSeconds(player, milliseconds);
                
                if(initialVelocity == finalVelocity && initialPosition == finalPosition){
                    player.ApplyCentralImpulse( 10f * player.Mass * ((player.GetYawPivotBasis() * Vector3.Forward).Normalized() + Vector3.Up) );
                    return;
                }

            }

        }

        private bool IsTryingToLedgeGrab(Player player){
            
            var directState = player.GetWorld3D().DirectSpaceState;

            Basis directionBasis = player.GetPlayerMesh().Basis;

            Vector3 forward = (player.GetYawPivotBasis() * player.GetMovementInput()).Normalized();

            if(forward.LengthSquared() < .2f)

                forward = (directionBasis * Vector3.Forward).Normalized();

            Vector3 hRayO = player.GlobalPosition - Vector3.Up * .55f;
            
            Vector3 hRayTo = hRayO + forward.Normalized() * .4f;

            var hRayQueryParameters = PhysicsRayQueryParameters3D.Create(hRayO, hRayTo, 2);

            var hCollision = directState.IntersectRay(hRayQueryParameters);

            if(hCollision.Count == 0) return false;
            
            hCollision.TryGetValue("position", out var hPos);

            Vector3 vRayO = (Vector3) hPos + Vector3.Up * .35f;
            Vector3 vRayTo = vRayO + Vector3.Down * .35f;

            var vRayQueryParameters = PhysicsRayQueryParameters3D.Create(vRayO, vRayTo, 2);

            var vCollision = directState.IntersectRay(vRayQueryParameters);

            if(vCollision.Count == 0) return false; 

            vCollision.TryGetValue("position", out var vPos);

            if((hRayO - (Vector3)vPos).Length() > .55f) return false;

            /*
            MeshInstance3D mesh = new();
            mesh.Mesh = new SphereMesh();
            mesh.Scale *= .2f;
            mesh.Position = (Vector3)vPos;
            player.GetParent().AddChild(mesh);
            */

            hCollision.TryGetValue("normal", out var normal);

            player.GetPlayerMesh().LookAt(player.GetPosition() - (Vector3)normal);

            return true;
        }
        public override void Start(Player player)
        {
            player.SetNoFrictionMaterial();
            player.LinearDamp = 4f;

            //player.GravityScale = 7.0f;

            if(player._AnimationStateMachine.GetCurrentNode() != "Fall")
                player._AnimationStateMachine.Travel("Fall");

            Random rand = new();

            WaitForSeconds(player, (.5f + (float)rand.NextDouble()) * 1000f);
        }

        public override void Update(Player player, float delta)
        {
            base.Update(player, delta);

        }

        public override void PhysicsUpdate(Player player, float delta)
        {
            base.PhysicsUpdate(player,delta);

            if (IsGrounded(player))
            {
                if (IsTryingToJump())
                {   
                    player._AnimationStateMachine.Start("Jump");
                    player.StateTransition(States.Jumping);
                    return;
                }

                player.StateTransition(States.Grounded);
            }
            else if(IsTryingToLedgeGrab(player)){
                
                player.StateTransition(States.LedgeGrabbing);
                
            }
        }
    }
}

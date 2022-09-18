using DemoDash.entities.weapons;
using Sandbox;

namespace DemoDash.player;

public partial class DemoDashPlayer
{
	[ConVar.Replicated("debug_movement")]
	public static bool DebugMovement { get; set; }

	private Capsule wallJumpCapsule = Capsule.FromHeightAndRadius( 70, 24 );

	// Initial velocity when a slide is initiated.
    [Net, Local, Predicted]
	public Vector3 SlideVelocity { get; set; }

	[Net, Local, Predicted]
	public bool IsDashing { get; set; }

	[Net, Local, Predicted]
	public bool IsSliding { get; set; }

	[Net, Local, Predicted]
	public bool IsOnWall { get; set; }

	[Net, Local, Predicted]
	public bool IsWallJumping { get; set; }

	public Vector3 WallEntityNormal { get; set; }

	readonly private float WallJumpPushForce = 650f;
	readonly private float WallJumpUpForce = 500f;
	readonly private float WallJumpCeilingTolerance = 0.1f;

	private Particles WallSlideParticles { get; set; }

	private void TickSpecialMovement()
    {
		// If the player is on the ground, after dashing, and is still holding down
		// the dash button, then slide on the ground.
		if (IsDashing && GroundEntity != null && Input.Down(InputButton.SecondaryAttack)) {
			Slide();
		}

		// If dashing and not sliding and the player is on the ground then end dash
		// If sliding and the dash button is released then end dash.
		if ((IsDashing && !IsSliding && Controller.GroundEntity != null)
			|| (IsSliding && Input.Released(InputButton.SecondaryAttack))
			|| (IsWallJumping && Controller.GroundEntity != null)) {
			EndDash();
		}

        // If dash button is pressed and not currently sliding or dashing and the dash has recharged then dash.
		if (Input.Pressed(InputButton.SecondaryAttack) && !IsSliding && !IsDashing && TimeSinceDash > DashRechargeTime) {
			StartDash();
		}

		// If diving through the air then rotate the pawn to match the sliding
		// rotation.
		if (GroundEntity == null && IsDashing) {
			// I don't even know how the fuck this works so don't ask me.
			float degrees = Velocity.z.LerpInverse(-300, 300) * -5;
			// Loot at the normal and rotate the normal 90 degrees to match the
			// current player rotation.
			Rotation = Rotation.FromAxis( Velocity.Normal * Rotation.FromYaw(90f).Normal, degrees ) * Rotation;
		}

        // If currently off the ground, check for the ability to wall jump.
		if (GroundEntity == null && TimeSinceJump.Relative > 0.25) {
			TickWallJump();
		} else {
			if (IsOnWall)
				ResetWallJumpEffects();

			IsOnWall = false;
			IsWallJumping = false;
		}

		// if on wall, then detect wall jumps and set animation parameters.
		if (IsOnWall) {
			if (Rand.Int(0, 2) == 1) {
				WallSlideEffects();
			}

            SetAnimParameter( "b_grounded", true );
            SetAnimParameter( "skid", 1.0f );

			var WallJumpFriction = 4.0f;
            Velocity *= new Vector3(1).WithZ( 1.0f - ( Time.Delta * WallJumpFriction ) );
            Rotation = Rotation.LookAt( WallEntityNormal * 10.0f, Vector3.Up );

			// WallJump
			if (Input.Pressed(InputButton.Jump)) {
                PlaySound( "dd.walljump" );
                ApplyAbsoluteImpulse(WallEntityNormal.WithZ(0) * WallJumpPushForce); // wall jump force
                Velocity = Velocity.WithZ( WallJumpUpForce ); // jump force
			}

            // DebugOverlay.Line(
            // 	Position+(Vector3.Up * 30),
            // 	Position+(Vector3.Up * 30) + wallJumpTrace.Normal * 100,
            // 	Color.Yellow, 0, false
            // );

			// Animator.AnimPawn.SetBoneTransform(15, )
			// var t = GetBoneTransform( 15 );
			// SetBoneTransform()
			// SetBone( 15, t.WithPosition( wallJumpTrace.HitPosition ) );
			// var t = GetBoneTransform( 13 );
			// SetBone( 13, t.WithRotation( Rotation.LookAt( wallJumpTrace.HitPosition ) ) );
			// var t = Animator.AnimPawn.GetBoneTransform( 13 );
			// Animator.AnimPawn.SetBone( 13, t.WithRotation( Rotation.LookAt( wallJumpTrace.HitPosition ) ) );
			// Animator.AnimPawn.ResetBone( 13 );
		}

		// Print out the debug movement.
		if (DebugMovement) {
			DebugOverlay.ScreenText( $" onwall[{IsOnWall}]"
				+ $"\n walljumping[{IsWallJumping}]"
				+ $"\n dashing[{IsDashing}]"
				+ $"\n sliding[{IsSliding}]" 
				+ $"\n  y[{(int)Rotation.Yaw()}]\n  r[{(int)Rotation.Roll()}]\n  p[{(int)Rotation.Pitch()}]");
		}
	}

	public void StartDash()
	{
		TimeSinceDash = 0;

		Vector3 jumpV = Controller.WishVelocity;
		if (jumpV.IsNearlyZero( 0.01f ))
			return;

		IsDashing = true;

		if (jumpV == Vector3.Zero)
			return;

		var withZ = GroundEntity == null ? 0 : 150;
		if (GroundEntity != null) {
			GroundEntity = null;
		}

		SetAnimParameter( "b_jump", true );
		ApplyAbsoluteImpulse( jumpV.WithZ(withZ) * 2 );
	}

	public void EndDash()
	{
		IsDashing = false;
		IsSliding = false;
		SlideVelocity = Vector3.Zero;
		TimeSinceDash = 0;
		SetAnimParameter( "skid", 0.0f );
	}

    public void Slide()
    {
        if (!IsSliding) {
            TimeSinceSlide = 0;
            IsSliding = true;
            SlideVelocity = Velocity;
        }

        SetAnimParameter( "duck", 1.0f );
        SetAnimParameter( "skid", 1.0f );

        var i = TimeSinceSlide.Relative.LerpInverse( 2, 0 );
        var jumpV = SlideVelocity * i;

        Velocity = jumpV;
	}

    private void TickWallJump()
    {
        var wallJumpTrace = Trace.Capsule( wallJumpCapsule, Position + wallJumpCapsule.CenterA, Position + wallJumpCapsule.CenterB )
            .WithTag( "solid" )
            .Run();

        // If the wall isn't nearly 90 degrees then it's not considered a
        // wall enough to walljump from.
        // Also if the walljump normal is almost straight down, then this is a ceiling
        // and it shouldn't count for walljumping.
        if (wallJumpTrace.Normal.Angle(Vector3.Up) < 80.0f
            || wallJumpTrace.Normal.z.AlmostEqual(-1, WallJumpCeilingTolerance))
            return;

		// If the player is currently on the wall and the walltrace didn't hit
		// then the player should not be on the wall.
		if (IsOnWall && !wallJumpTrace.Hit)
			ResetWallJumpEffects();

		IsOnWall = wallJumpTrace.Hit;
		if (IsOnWall) {
			// DebugOverlay.Text( $"on wall n[{wallJumpTrace.Normal}]", Position + Vector3.Up * 80 );
			IsWallJumping = true;
			WallEntityNormal = wallJumpTrace.Normal;
		}
    }

	protected void ResetWallJumpEffects()
	{
		// ResetBones();
		// ResetAnimParameters();
	}

	[ClientRpc]
	protected void WallSlideEffects()
	{
		// Assert that this is being called by a client.
		Host.AssertClient();
        // Create particles in front of the weapon (works for first & third person views)
		Particles.Create( "particles/wallslide.vpcf", this, "ball_R", false );
	}
}
internal partial class DemoDashController : WalkingController
{
	[ConVar.Replicated("debug_movement")]
	public static bool DebugMovement { get; set; }

	[Net] public float WallFriction { get; set; } = 4.0f;
	[Net] public float WallJumpPushForce { get; set; } = 650.0f;
	[Net] public float WallJumpUpForce { get; set; } = 500.0f;
	[Net] public float WallJumpCeilingTolerance { get; set; } = 0.1f;
	[Net] public float DashRechargeTime { get; set; } = 0.3f;
	[Net] public float SlideLength { get; set; } = 2.0f;
	[Net] public float RotationCorrectionSpeed { get; set; } = 1.5f;

	[Net, Local, Predicted] public bool Grounded { get; set; }
	[Net, Local, Predicted] public bool PrevGrounded { get; set; }
	[Net, Local, Predicted] public bool Dashing { get; set; }
	[Net, Local, Predicted] public bool PrevDashing { get; set; }
	[Net, Local, Predicted] public bool Sliding { get; set; }
	[Net, Local, Predicted] public bool PrevSliding { get; set; }
	// OnWall is whether this pawn is currently against a wall and ready for a
	// wall jump.
	[Net, Local, Predicted] public bool OnWall { get; set; }
	[Net, Local, Predicted] public bool PrevOnWall{ get; set; }
	// WallJumping is the act of jumping wall to wall.
	[Net, Local, Predicted] public bool WallJumping { get; set; }
	[Net, Local, Predicted] public bool PrevWallJumping { get; set; }
	[Net, Local, Predicted] public TimeUntil TimeUntilSlideEnds { get; set; }

	// [Net, Predicted]
	public Vector3 SlideVelocity { get; set; }
	public Vector3 WallTraceNormal { get; set; }


	TimeSince TimeSinceJump;
	TimeSince TimeSinceDash;
	// Maybe use TimeUntil to remove any MovementEvents.

	public DemoDashController() : base()
	{
		Event.Register( this );
	}

	~DemoDashController()
	{
		Event.Unregister( this );
	}

	public override void Simulate()
	{
		base.Simulate();

		// If the grounded state has changed then call the event.
		if (PrevGrounded != Grounded) {
			TimeSinceJump = 0;
		}
		// If the player is on the ground, after dashing, and is still holding down
		// the dash button, then slide on the ground.
		if (!Sliding && Dashing && Grounded && Input.Down(InputButton.SecondaryAttack)) {
			StartSlide();
		}
		// If the player is currently sliding, then wane the velocity down
		// and trigger the sliding event.
		if (Sliding) {
			var i = TimeUntilSlideEnds.Relative.LerpInverse(0, SlideLength);
			var jumpV = SlideVelocity * i;
			Velocity = jumpV;
		}
		// If dashing and not sliding and the player is on the ground then end dash
		// If sliding and the dash button is released then end dash.
		if ((Dashing && !Sliding && Grounded)
			|| (Sliding && Input.Released(InputButton.SecondaryAttack))
			|| (WallJumping && Grounded)
			|| (Sliding && Velocity.IsNearlyZero(0.1f))
			|| (Sliding && Input.Down(InputButton.Jump))) {

			EndDash();
		}
        // If dash button is pressed and not currently sliding or dashing
		// and the dash has recharged then dash.
		if (Input.Pressed(InputButton.SecondaryAttack)
			&& !Sliding && !Dashing && TimeSinceDash > DashRechargeTime) {

			StartDash();
		}
		// If diving through the air then rotate the pawn to match the sliding
		// rotation.
		if (!Grounded && Dashing) {
			// I don't even know how the fuck this works so don't ask me.
			float degrees = Velocity.z.LerpInverse(-300, 300) * -5;
			// Loot at the normal and rotate the normal 90 degrees to match the
			// current player rotation.
			Rotation = Rotation.FromAxis( Velocity.Normal * Rotation.FromYaw(90f).Normal, degrees ) * Rotation;
		}
    	// If currently off the ground, check for the ability to wall jump.
		if (!Grounded && TimeSinceJump.Relative > 0.25) {
			TickOnWall();
		} else if (OnWall || WallJumping) {
			OnWall = false;
			WallJumping = false;
		}
		// if on a wall, then detect wall jumps.
		if (OnWall) {
			Velocity *= new Vector3( 1 ).WithZ( 1.0f - (Time.Delta * WallFriction) );
			Rotation = Rotation.LookAt( WallTraceNormal * 10.0f, Vector3.Up );

			if (Input.Pressed(InputButton.Jump)) {
				Velocity += WallTraceNormal.WithZ( 0 ) * WallJumpPushForce;
				Velocity = Velocity.WithZ( WallJumpUpForce );
				AddEvent( "walljump" );
			}
		}
		// Stopped being on the wall
		if (OnWall != PrevOnWall) {
			if (OnWall)
				AddEvent( "startonwall" );
			else
				AddEvent( "endonwall" );
		}
		// Readjust the player's rotation to be perpindicular to the ground.
		var angles = Rotation.Angles();
		angles.pitch = 0f;
		angles.roll = 0f;
		Rotation = Rotation.Lerp(
			Rotation,
			Rotation.From( angles ),
			Time.Delta * 1.5f
		);

		PrevGrounded = Grounded;
		PrevWallJumping = WallJumping;
		PrevDashing = Dashing;
		PrevSliding = Sliding;
		PrevOnWall = OnWall;

		// Print out the debug movement.
		if (DebugMovement) {
			DebugOverlay.ScreenText( $" onwall[{OnWall}]"
				+ $"\n walljumping[{WallJumping}]"
				+ $"\n dashing[{Dashing}]"
				+ $"\n sliding[{Sliding}]" 
				+ $"\n  y[{(int)Rotation.Yaw()}]\n  r[{(int)Rotation.Roll()}]\n  p[{(int)Rotation.Pitch()}]");
		}
	}

	public virtual void StartSlide()
	{
		TimeUntilSlideEnds = SlideLength;
		Sliding = true;
		SlideVelocity = Velocity;

		AddEvent( "slide" );
	}

	public override void ClearGroundEntity()
	{
		base.ClearGroundEntity();
		Grounded = false;
	}

	public override void UpdateGroundEntity(TraceResult tr)
	{
		base.UpdateGroundEntity( tr );
		Grounded = true;
	}

	public virtual void StartDash()
	{
		TimeSinceDash = 0;

		Vector3 jumpV = WishVelocity;
		if (jumpV.IsNearlyZero(0.01f))
			return;

		Dashing = true;

		var withZ = !Grounded ? Velocity.z.Clamp(-150, 150) : 150 + Velocity.z;
		if (Grounded) {
			ClearGroundEntity();
			// GroundEntity = null;
			Grounded = !Grounded;
		}

		Velocity = jumpV.WithZ( withZ ) * 2;
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		AddEvent( "dash" );
	}

	public virtual void EndDash()
	{
		Dashing = false;
		Sliding = false;
		SlideVelocity = Vector3.Zero;
		TimeSinceDash = 0;

		AddEvent( "enddash" );
	}

	public virtual void TickOnWall()
	{
		var cap = Capsule.FromHeightAndRadius( BodyHeight, BodyGirth );
		var tr = Trace.Capsule( cap, Position + cap.CenterA, Position + cap.CenterB )
			.WithTag( "solid" )
			.Run();

        // If the wall isn't nearly 90 degrees then it's not considered a
        // wall enough to walljump from.
        // Also if the walljump normal is almost straight down, then this is a ceiling
        // and it shouldn't count for walljumping.
		if ( tr.Normal.Angle(Vector3.Up) < 80.0f
			|| tr.Normal.z.AlmostEqual( -1, WallJumpCeilingTolerance ) )
			return;

		// If the player is currently on the wall and the walltrace didn't hit
		// then the player should not be on the wall.
		if ( OnWall && !tr.Hit ) {
			// reset walljump effects
		}

		OnWall = tr.Hit;
		if (OnWall) {
			WallJumping = true;
			WallTraceNormal = tr.Normal;
		}
	}
}

using DemoDash.entities.weapons;
using Sandbox;

namespace DemoDash.player;

public partial class DemoDashPlayer
{
    private Capsule wallJumpCapsule = Capsule.FromHeightAndRadius( 70, 24 );

	// Initial velocity when a slide is initiated.
    [Net, Local]
	public Vector3 SlideVelocity { get; set; }

	[Net, Local]
	public bool IsDashing { get; set; }

	[Net, Local]
	public bool IsSliding { get; set; }

	[Net, Local]
	public bool IsWallSliding { get; set; }

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
			|| (IsWallSliding && Controller.GroundEntity != null)) {
			EndDash();
		}

        // If dash button is pressed and not currently sliding or dashing and the dash has recharged then dash.
		if (Input.Pressed(InputButton.SecondaryAttack) && !IsSliding && !IsDashing && TimeSinceDash > DashRechargeTime) {
			StartDash();
		}
        // If currently off the ground, check for the ability to wall jump.
		if (GroundEntity == null) {
			TickWallJump();
		}
    }

	public void StartDash()
	{
		IsDashing = true;
		TimeSinceDash = 0;

		// var jumpV = EyeRotation.Forward * 335; // 335 is lunge force.
		Vector3 jumpV = default;
		if (!Controller.WishVelocity.IsNearlyZero( 0.01f )) {
			jumpV = Controller.WishVelocity;
		}

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
		IsWallSliding = false;
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
        if (wallJumpTrace.Normal.Angle(Vector3.Up) < 80.0f)
            return;

        // DebugOverlay.TraceResult( wallJumpTrace );

        // var jumpDir = EyeRotation.Forward;
        // if ( Vector3.Dot( wallJumpTrace.Normal, jumpDir ) >= .3f ) // <= -.3f
        // 	jumpDir = wallJumpTrace.Normal;

        if (wallJumpTrace.Hit) {
            // Wall sliding.
            var WallJumpFriction = 3.0f;

            IsWallSliding = true;

            SetAnimParameter( "b_grounded", true );
            SetAnimParameter( "skid", 1.0f );

            Velocity *= new Vector3(1).WithZ( 1.0f - ( Time.Delta * WallJumpFriction ) );
            Rotation = Rotation.LookAt( wallJumpTrace.Normal * 10.0f, Vector3.Up );

            // WallJump
            if (Input.Pressed(InputButton.Jump)) {
                PlaySound( "dd.walljump" );
                ApplyAbsoluteImpulse(wallJumpTrace.Normal.WithZ(0) * 500); // wall jump force
                Velocity = Velocity.WithZ( 500 ); // jump force
            }

            // DebugOverlay.Line(
            // 	Position+(Vector3.Up * 30),
            // 	Position+(Vector3.Up * 30) + wallJumpTrace.Normal * 100,
            // 	Color.Yellow, 0, false
            // );
        }
    }
}
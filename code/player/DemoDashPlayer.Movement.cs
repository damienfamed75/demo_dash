using DemoDash.entities.weapons;
using Sandbox;

namespace DemoDash.player;

public partial class DemoDashPlayer
{
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

	

	[ClientRpc]
	protected void WallSlideEffects()
	{
		// Assert that this is being called by a client.
		Host.AssertClient();
        // Create particles in front of the weapon (works for first & third person views)
		Particles.Create( "particles/wallslide.vpcf", this, "ball_R", false );
	}
}
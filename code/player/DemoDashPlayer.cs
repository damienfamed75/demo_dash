using System.Threading.Tasks;
using DemoDash.entities.weapons;
using Sandbox;

namespace DemoDash.player;

public partial class DemoDashPlayer : Player
{
	private TimeSince TimeSinceDropped;
	private TimeSince TimeSinceJumpReleased;

	public ClothingContainer Clothing = new();

	public DemoDashPlayer()
	{
		Inventory = new Inventory( this );
	}

	public DemoDashPlayer(Client cli) : this()
	{
		Clothing.LoadFromClient( cli );
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new WalkController{
            WalkSpeed = 270,
            SprintSpeed = 100,
            DefaultSpeed = 270,
            AirAcceleration = 0,
        };

		EnableHideInFirstPerson = true;
		EnableDrawing = true;
		EnableShadowInFirstPerson = true;

		Clothing.DressEntity( this );

		Inventory.Add( new Shotgun(), true );
		Inventory.Add( new Pistol() );

		Animator = new StandardPlayerAnimator();
		CameraMode = new FirstPersonCamera();

		Tags.Add( "player" );
		Health = 100;

		base.Respawn();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
	}

	public override void OnKilled()
	{
		base.OnKilled();
	}

	public override void Simulate( Client cl )
	{
        if (DemoDashGame.CurrentState == DemoDashGame.GameStates.GameEnd)
			return;

		base.Simulate( cl );

		if (Input.ActiveChild != null) {
			ActiveChild = Input.ActiveChild;
		}

		if (LifeState != LifeState.Alive)
			return;

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		// If view button is pressed, change camera mode.
		if (Input.Pressed(InputButton.View)) {
            if (CameraMode is ThirdPersonCamera) {
				CameraMode = new FirstPersonCamera();
			} else {
				CameraMode = new ThirdPersonCamera();
			}
        }

		if (Input.Pressed(InputButton.Drop)) {
			var dropped = Inventory.DropActive();
			// If the dropped item is not null then apply physics and force.
			if (dropped != null) {
				// Throw the item forward and upward.
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRotation.Forward * 500f + Vector3.Up * 100f, true );
				// Apply random rotational force.
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100f, true );
				// Throw the item facing sideways.
				dropped.Rotation = Rotation.FromYaw( 90f ) * EyeRotation;

				TimeSinceDropped = 0;
			}
		}
	}

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
	}

    public void RenderHud(Vector2 screenSize)
    {
        if (LifeState == LifeState.Dead)
			return;

        // TODO render weapon info.
	}
}
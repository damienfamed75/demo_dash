using System.Reflection.Metadata;
using DemoDash.entities.weapons;
using Sandbox;

namespace DemoDash.player;

public partial class DemoDashPlayer : Player
{
	private TimeSince TimeSinceDropped;
	private TimeSince TimeSinceDash;
	private TimeSince TimeSinceSlide;

	[Net, Predicted]
	public TimeSince TimeSinceDamage { get; set; }

	private readonly float DashRechargeTime = 0.3f;
	private readonly float WalkSpeed = 270;
	private readonly float SprintSpeed = 100;
	public readonly float MaxHealth = 100;

	public ClothingContainer Clothing = new();

	private DamageInfo lastDamage;

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
            WalkSpeed = WalkSpeed,
            SprintSpeed = SprintSpeed,
            DefaultSpeed = WalkSpeed,
            AirAcceleration = 45.0f,
			AirControl = 40.0f,
        };

		EnableHideInFirstPerson = true;
		EnableDrawing = true;
		EnableShadowInFirstPerson = true;
		EnableAllCollisions = true;

		Clothing.DressEntity( this );

		Inventory.Add( new Shotgun(), true );
		Inventory.Add( new Pistol() );
		Inventory.Add( new SMG() );

		GiveAmmo( AmmoType.Shotgun, 99 );
		GiveAmmo( AmmoType.Pistol, 160 );

		Animator = new StandardPlayerAnimator();
		CameraMode = new FirstPersonCamera();

		Tags.Add( "player" );
		Health = 100;

		EndDash();

		base.Respawn();
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
		TickSpecialMovement();

		if (Input.Pressed(InputButton.Drop)) {
			var dropped = Inventory.DropActive();
			// If the dropped item is not null then apply physics and force.
			if (dropped != null) {
				// Explicitly set the position of the dropped item in front of the player.
				dropped.Position = EyePosition + EyeRotation.Forward * 5.0f;
				// Throw the item forward and upward.
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRotation.Forward * 500f + Vector3.Up * 100f, true );
				// Apply random rotational force.
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100f, true );
				// Throw the item facing sideways.
				dropped.Rotation = Rotation.FromYaw( 90f ) * EyeRotation;

				TimeSinceDropped = 0;
			}
		}

		// If view button is pressed, change camera mode.
		if (Input.Pressed(InputButton.View)) {
            if (CameraMode is ThirdPersonCamera) {
				CameraMode = new FirstPersonCamera();
			} else {
				CameraMode = new ThirdPersonCamera();
			}
        }
	}

	public void RenderHud(Vector2 screenSize)
    {
        if (LifeState == LifeState.Dead)
			return;

        // TODO render weapon info.
	}

	public int GiveHealth(int amount)
	{
		var total = amount + Health;
		if (total > MaxHealth)
			total = MaxHealth;

		var taken = total - Health;
		Health = total;

		return (int)taken;
	}

	public override void OnKilled()
	{
		base.OnKilled();
		// For certain types of damage, do different things.
		if (lastDamage.Flags.HasFlag(DamageFlags.Blunt))
		{
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}
		// Become a ragdoll.
		BecomeRagdollOnClient(
			Velocity,
			lastDamage.Flags,
			lastDamage.Position,
			lastDamage.Force,
			GetHitboxBone( lastDamage.HitboxIndex )
		);

		Controller = null;
		EnableAllCollisions = false;
		EnableDrawing = false;
		CameraMode = new SpectateRagdollCamera();

		foreach (var child in Children) {
			child.EnableDrawing = false;
		}
		// Drop the currently held item and delete the rest of the items.
		Inventory.DropActive();
		Inventory.DeleteContents();
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );

		TimeSinceDamage = 0;
		bool wasHeadshot = false;

		// If the player was hit in the head (which is HitboxGroup 1)
		// then double the damage.
		if (GetHitboxGroup(info.HitboxIndex) == 1) {
			info.Damage *= 2.0f; // Crit hit
			wasHeadshot = true;
		}

		lastDamage = info;

		if (info.Attacker is DemoDashPlayer attacker) {
			if (Health <= 0) {
				var score = 100;

				score = (int)(score * (attacker.GroundEntity == null ? 1.5f : 1.0f));
				score = (int)(score * (attacker.IsDashing && !attacker.IsSliding ? 2f : 1.0f));
				score = (int)(score * (attacker.IsSliding ? 1.5f : 1.0f));
				score = (int)(score * (attacker.IsWallSliding ? 3f : 1.0f));

				if (wasHeadshot)
					score *= 2;

				Log.Info( $"add [{score}] to [{info.Attacker.Name}]" );
				// attacker.Client.AddInt( "score", score );
				info.Attacker.Client.AddInt( "score", score );
			}
		}

		// TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );
	}

	public override void StartTouch(Entity other)
	{
		if (TimeSinceDropped < 1)
			return;

		base.StartTouch( other );
	}

	public override float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 5f;
	}
}
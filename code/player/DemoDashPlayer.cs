using DemoDash.entities.weapons;
using DemoDash.ui;
using Sandbox;

namespace DemoDash.player;

public partial class DemoDashPlayer : Player
{
	private TimeSince TimeSinceDropped;

	[Net, Predicted]
	public TimeSince TimeSinceDamage { get; set; }

	private readonly float WalkSpeed = 270;
	private readonly float SprintSpeed = 100;
	public readonly float MaxHealth = 50;

	public ClothingContainer Clothing = new();

	private DamageInfo lastDamage;
	private Entity lastWeapon;

	public Sound slideSound { get; set; }
	public Sound wallSlideSound { get; set; }

	public Particles WallSlideParticles { get; set; }

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

		Controller = new DemoDashController{
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

		Inventory.Add( new Pistol(), true);
		// Inventory.Add( new Shotgun(), true );
		// Inventory.Add( new SMG() );

		GiveAmmo( AmmoType.Shotgun, 99 );
		GiveAmmo( AmmoType.Pistol, 120 );

		Animator = new StandardPlayerAnimator();
		CameraMode = new ThirdPersonCamera();

		Tags.Add( "player" );
		Health = MaxHealth;

		base.Respawn();
	}

	public override void Simulate( Client cl )
	{
        if (DemoDashGame.CurrentState == DemoDashGame.GameStates.GameEnd)
			return;

		base.Simulate( cl );

		var ctrlr = GetActiveController();
		if (ctrlr == null)
			return;

		var controller = ctrlr as DemoDashController;
		if (controller != null) {
			EnableSolidCollisions = !controller.HasTag( "noclip" );
			SimulateAnimation(controller);
		}

		if (Input.ActiveChild != null) {
			ActiveChild = Input.ActiveChild;
		}
		// If player is not alive then skip.
		if (LifeState != LifeState.Alive)
			return;

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );
		// If the player tries to drop then drop the active child item.
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
		// If the player is sliding then adjust the volume and pitch of the sliding sound.
		if (controller.Sliding) {
			var i = controller.TimeUntilSlideEnds.Relative.LerpInverse( 0, controller.SlideLength );
			slideSound.SetVolume( i );
			slideSound.SetPitch( i );
		}
		// If the player is on a wall then spawn particles.
		if (controller.OnWall && Rand.Int(0, 2) == 1) {
			Particles.Create( "particles/wallslide.vpcf", this, false );
		}

		// Position the sound listener to match the first person camera position
		// when in third person.
		// Found this in sbox-unicycle-frenzy
		Sound.Listener = new Transform(
			EyePosition + (CameraMode.Position - EyePosition).Normal * 15f,
			CameraMode.Rotation
		);
		// Sound.Listener = new Transform(
		// 	Position + (CameraMode.Position - Position).Normal * 10f,
		// 	CameraMode.Rotation
		// );
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
			var score = 100;
			if (Health <= 0) {

				var atkctrl = attacker.Controller as DemoDashController;

				score = (int)(score * (attacker.GroundEntity == null ? 1.5f : 1.0f));
				score = (int)(score * (atkctrl.Dashing && !atkctrl.Sliding ? 2f : 1.0f));
				score = (int)(score * (atkctrl.Sliding ? 1.5f : 1.0f));
				score = (int)(score * (atkctrl.WallJumping ? 3f : 1.0f));
				score = (int)(score * (atkctrl.OnWall ? 3f : 1.0f));

				if (wasHeadshot)
					score *= 2;

				info.Attacker.Client.AddInt( "score", score );
			}
			attacker.DidDamage( wasHeadshot, Health <= 0, score );
		}

		// TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );
	}

	[ClientRpc]
	public void DidDamage(bool wasHeadshot, bool isDead, int score)
	{
		if (IsServer)
			return;

		if (wasHeadshot) {
			Sound.FromScreen( "dd.headshot" );
		}
		if (isDead) {
			var scorePanel = new Score( score );
			// Randomize whether to spawn number left or right of the player.
			var offRot = Rand.Int( 0, 1 ) == 1 ? EyeRotation.Right : EyeRotation.Left;
			// Random offset going downward.
			var randDownOffset = Rand.Float( 0.0f, 20.0f );
			// Set the position of the score panel.
			scorePanel.Position = EyePosition + offRot * 32.0f + EyeRotation.Down * randDownOffset;
			// Get the camera rotation and then flip around to be right way.
			scorePanel.Rotation = CameraMode.Rotation * Rotation.FromYaw(180.0f);

			float pitch = MathX.LerpInverse(score, 1200.0f, 0.0f, true);
			Sound.FromScreen( "dd.score" ).SetPitch( 0.5f + pitch );
		}
	}

	public override void StartTouch(Entity other)
	{
		if (TimeSinceDropped < 1)
			return;

		base.StartTouch( other );
	}

	public override float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 2f;
	}

	// Every tick, animate the citizen according to the input.
	void SimulateAnimation(DemoDashController controller)
	{
		if ( controller == null )
			return;

		// var turnSpeed = .02f;
		// var iRot = Rotation.LookAt( Input.Rotation.Forward.WithZ( 0 ), Vector3.Up );
		// Rotation = Rotation.Slerp( Rotation, iRot, controller.WishVelocity.Length * turnSpeed * Time.Delta );
		// Lock animation facing wihtin 45 degrees of rotation true to the player's eye rotation.
		// Rotation = Rotation.Clamp( iRot, 45.0f, out var shuffle );

		CitizenAnimationHelper animHelper = new( this );

		animHelper.WithWishVelocity( controller.WishVelocity );
		animHelper.WithVelocity( controller.Velocity );
		// animHelper.WithLookAt( EyePosition + EyeRotation.Forward * 100f, 1f, 1f, 0.5f);
		// animHelper.AimAngle = Input.Rotation;
		// animHelper.FootShuffle = shuffle;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, controller.HasTag( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.VoiceLevel = (Host.IsClient && Client.IsValid()) ? Client.TimeSinceLastVoice < 0.5f ? Client.VoiceLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = GroundEntity != null;
		animHelper.IsSitting = controller.HasTag( "sitting" );
		animHelper.IsWeaponLowered = false;
		//!TODO add rest of params
		// If dashing then jump and play sound.
		if (controller.HasEvent("dash")) {
			animHelper.TriggerJump();
			PlaySound( "dd.walljump" );
		}
		// If sliding then skid, duck, and play skidding sound.
		if (controller.HasEvent("slide")) {
			SetAnimParameter( "duck", 1.0f );
        	SetAnimParameter( "skid", 1.0f );
			slideSound = PlaySound( "dd.skid" );
		}
		// If dashing is over then no more skidding.
		if (controller.HasEvent("enddash")) {
			SetAnimParameter( "skid", 0.0f );
			slideSound.Stop();
		}
		// If wall jumping then jump and play sound.
		if (controller.HasEvent("walljump")) {
			animHelper.TriggerJump();
			PlaySound( "dd.walljump" );
		}
		// If player is just now touching a wall, ready for a wall jump then
		// play the according sound.
		if (controller.HasEvent("startonwall")) {
			wallSlideSound.Stop();
			wallSlideSound = PlaySound( "dd.skid" );
			var i = Velocity.Normal.WithZ( 0 ).Distance( Vector3.Zero ).Clamp( 0.25f, 0.8f );
			wallSlideSound.SetVolume( i );
			wallSlideSound.SetPitch( i );
			SetAnimParameter( "skid", 1.0f );
		}
		// If player is off the wall then stop the wall sound.
		if (controller.HasEvent("endonwall")) {
			wallSlideSound.Stop();
		}
		// If player is currently on a wall then ground them.
		if (controller.OnWall) {
			SetAnimParameter( "b_grounded", true );
		}

		if ( ActiveChild != lastWeapon) {
			animHelper.TriggerDeploy();
		}

		if (ActiveChild is BaseCarriable carry) {
			carry.SimulateAnimator( animHelper );
		} else {
			animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			animHelper.AimBodyWeight = 0.5f;
		}

		lastWeapon = ActiveChild;
	}
}
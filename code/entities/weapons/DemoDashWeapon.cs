using System.Numerics;
using DemoDash.player;
using Sandbox;

namespace DemoDash.entities.weapons;

partial class DemoDashWeapon : BasicWeapon, IUse
{
    public virtual AmmoType AmmoType => AmmoType.Pistol;
	public virtual int ClipSize => 16;
	public virtual float ReloadTime => 3.0f;
	public virtual int Bucket => 1;
	public virtual int BucketWeight => 100;

	public virtual int Order => (Bucket * 10000) + BucketWeight;

    [Net, Predicted]
    public int AmmoClip { get; set; }

    [Net, Predicted]
    public TimeSince TimeSinceReload { get; set; }

    [Net, Predicted]
    public bool IsReloading { get; set; }

    [Net, Predicted]
    public TimeSince TimeSinceDeployed { get; set; }

    public PickupTrigger PickupTrigger { get; protected set; }

	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	/// <summary>
	/// Spawn is called when this weapon is spawned in the world.
	/// 
	/// Set a default model to this weapon and create a pickup trigger.
	/// </summary>
    public override void Spawn()
    {
		base.Spawn();
		// Set the base model to be the rust pistol.
		// SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
		// Create a pickup trigger so a player can use this.
		PickupTrigger = new PickupTrigger {
			Parent = this,
			Position = Position,
			EnableSelfCollisions = false,
			EnableTouch = true,
		};
		// Set the physics body to never sleep.
		PickupTrigger.PhysicsBody.AutoSleep = false;
	}

	public override void Reload()
	{
		if (IsReloading)
			return;

        if (AmmoClip >= ClipSize)
			return;

		TimeSinceReload = 0;

        if (Owner is DemoDashPlayer player) {
            if (player.AmmoCount(AmmoType) <= 0)
				return;
		}

		IsReloading = true;
		(Owner as AnimatedEntity).SetAnimParameter( "b_reload", true );
		StartReloadEffects();
	}

    [ClientRpc]
    public virtual void StartReloadEffects()
    {
        if (ViewModelEntity != null) {
			ViewModelEntity.SetAnimParameter( "reload", true );
		}
    }

	public override void Simulate( IClient player )
	{
		if (TimeSinceDeployed < 0.6f)
			return;

        if (!IsReloading) {
			base.Simulate( player );
		}

        if (IsReloading && TimeSinceReload > ReloadTime) {
			OnReloadFinish();
		}
	}

    public virtual void OnReloadFinish()
    {
		IsReloading = false;
        if (Owner is DemoDashPlayer player) {
			var ammo = player.TakeAmmo( AmmoType, ClipSize - AmmoClip );
            if (ammo == 0)
				return;

			AmmoClip += ammo;
		}
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
	}

	/// <summary>
	/// ShootEffects spawns particles at the weapon and plays the fire animation.
	/// </summary>
    [ClientRpc]
    protected virtual void ShootEffects()
    {
        // Assert that this is being called by a client.
		Game.AssertClient();
        // Create particles in front of the weapon (works for first & third person views)
		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
        // Playback the fire animation on the weapon.
        if (ViewModelEntity != null) {
			ViewModelEntity.SetAnimParameter( "fire", true );
		}
	}

	/// <summary>
	/// AvailableAmmo returns the amount of total ammunition for this weapon's ammo type.
	/// </summary>
    public int AvailableAmmo()
    {
		if ( Owner is not DemoDashPlayer owner )
			return 0;

		return owner.AmmoCount( AmmoType );
	}

	/// <summary>
	/// Remove deletes this entity.
	/// </summary>
	public void Remove()
	{
		Delete();
	}

    [ClientRpc]
    public void CreateTracerEffect(Vector3 hitPosition)
    {
		// Get the muzzle position on our effect entity (either the viewmodel or world model)
		var pos = EffectEntity.GetAttachment( "muzzle" ) ?? Transform;

		var pSys = Particles.Create( "particles/tracer.standard.vpcf" );
		if (pSys != null) {
			pSys.SetPosition( 0, pos.Position );
			pSys.SetPosition( 1, hitPosition );
		}
	}

	/// <summary>
	/// Try to take ammo from the weapon's clip.
	/// If the amount is greater than the clip size then nothing is taken and false is returned.
	/// </summary>
	/// <param name="amount"></param>
	/// <returns>true if there's enough ammo</returns>
    public bool TakeAmmo(int amount)
    {
        if (AmmoClip < amount)
			return false;

		AmmoClip -= amount;
		return true;
	}

    public void DryFire()
    {
		PlaySound( "dd.pistol-dryfire" );
	}
}

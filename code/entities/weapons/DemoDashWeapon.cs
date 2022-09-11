using System.Numerics;
using DemoDash.player;

namespace DemoDash.entities.weapons;

partial class DemoDashWeapon : BaseWeapon
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

    public int AvailableAmmo()
    {
		var owner = Owner as DemoDashPlayer;
        if (owner == null)
            return 0;

		return owner.AmmoCount( AmmoType );
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );
		TimeSinceDeployed = 0;
		IsReloading = false;
	}

	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

    public override void Spawn()
    {
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );

		PickupTrigger = new PickupTrigger {
			Parent = this,
			Position = Position,
		};
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

	public override void Simulate( Client player )
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
		Host.AssertClient();
        // Create particles in front of the weapon (works for first & third person views)
		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
        // Playback the fire animation on the weapon.
        if (ViewModelEntity != null) {
			ViewModelEntity.SetAnimParameter( "fire", true );
		}
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

    public bool IsUsable()
    {
        if (AmmoClip > 0)
			return true;

        if (AmmoType == AmmoType.None)
			return true;

		return AvailableAmmo() > 0;
	}

	public override void OnCarryStart( Entity carrier )
	{
		base.OnCarryStart( carrier );

        if (PickupTrigger.IsValid()) {
			PickupTrigger.EnableTouch = false;
		}
	}

	public override void OnCarryDrop( Entity dropper )
	{
		base.OnCarryDrop( dropper );

        if (PickupTrigger.IsValid()) {
			PickupTrigger.EnableTouch = true;
		}
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

        if (string.IsNullOrEmpty(ViewModelPath))
			return;

		ViewModelEntity = new ViewModel();
		ViewModelEntity.Position = Position;
		ViewModelEntity.Owner = Owner;
		ViewModelEntity.EnableViewmodelRendering = true;
		ViewModelEntity.SetModel( ViewModelPath );
		ViewModelEntity.SetAnimParameter( "deploy", true );
	}

    public void DryFire()
    {
		PlaySound( "dd.pistol-dryfire" );
	}
}
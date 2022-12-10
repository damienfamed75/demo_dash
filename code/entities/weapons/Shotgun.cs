using DemoDash.player;

namespace DemoDash.entities.weapons;

[Library( "dd_shotgun" ), HammerEntity]
[EditorModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" )]
[Title( "Shotgun" ), Category( "Weapons" )]
partial class Shotgun : DemoDashWeapon
{
	public static readonly Model WorldModel = Model.Load( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" );
	public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun.vmdl";
	public override float PrimaryRate => 1.0f;
	public override float SecondaryRate => 1.0f;
	public override AmmoType AmmoType => AmmoType.Shotgun;
	public override int ClipSize => 8;
	public override float ReloadTime => 0.5f;
	public override int Bucket => 2;
	public override int BucketWeight => 200;

    [Net, Predicted]
    public bool StopReloading { get; set; }

    public override void Spawn()
    {
		base.Spawn();

		Model = WorldModel;
		AmmoClip = 6;
	}

    public override void Simulate(IClient owner)
    {
		base.Simulate( owner );

        if (IsReloading && (Input.Pressed(InputButton.PrimaryAttack) || Input.Pressed(InputButton.SecondaryAttack))) {
			StopReloading = true;
		}
	}

    public override void AttackPrimary()
    {
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

        if (!TakeAmmo(1)) {
			DryFire();
            if (AvailableAmmo() > 0) {
				Reload();
			}
			return;
		}

		(Owner as AnimatedEntity).SetAnimParameter( "b_attack", true );

		ShootEffects();
		PlaySound( "rust_pumpshotgun.shoot" );

		ShootBullets( 4, 0.2f, 0.3f, 20.0f, 2.0f );
	}

    [ClientRpc]
	protected override void ShootEffects()
	{
		Game.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

        if (ViewModelEntity != null) {
			ViewModelEntity.SetAnimParameter( "fire", true );
		}
	}

    public override void OnReloadFinish()
    {
		var stop = StopReloading;

		StopReloading = false;
		IsReloading = false;

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

        if (AmmoClip >= ClipSize)
			return;

        if (Owner is DemoDashPlayer player) {
			var ammo = player.TakeAmmo( AmmoType, 1 );
            if (ammo == 0)
				return;

			AmmoClip += ammo;

            if (AmmoClip < ClipSize && !stop) {
				Reload();
			} else {
				FinishReload();
			}
		}
	}

    [ClientRpc]
    protected virtual void FinishReload()
    {
        if (ViewModelEntity != null) {
			ViewModelEntity.SetAnimParameter( "reload_finished", true );
		}
    }

	public override void SimulateAnimator( CitizenAnimationHelper anim )
	{
		// anim.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Shotgun );
		// anim.SetAnimParameter( "aim_body_weight", 1.0f );
		anim.HoldType = CitizenAnimationHelper.HoldTypes.Shotgun;
		anim.AimBodyWeight = 1.0f;
	}
}

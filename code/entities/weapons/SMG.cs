using DemoDash.player;
using DemoDash.util;

namespace DemoDash.entities.weapons;

[Library( "dd_smg" ), HammerEntity]
[EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
[Title( "SMG" ), Category( "Weapons" )]
partial class SMG : DemoDashWeapon
{
	public static readonly Model WorldModel = Model.Load( "weapons/rust_smg/rust_smg.vmdl" );
	public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";

	public override float PrimaryRate => 16.0f;
	public override float SecondaryRate => 1.0f;
	public override int ClipSize => 50;
	public override float ReloadTime => 2.0f;
	public override int Bucket => 2;
	public override int BucketWeight => 100;

    public override void Spawn()
    {
		base.Spawn();

		Model = WorldModel;
		AmmoClip = ClipSize;
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
		PlaySound( "rust_smg.shoot" );

		ShootBullet( 0.1f, 1.5f, 12.0f, 3.0f );
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

	public override void SimulateAnimator( CitizenAnimationHelper anim )
	{
		anim.HoldType = CitizenAnimationHelper.HoldTypes.Rifle;
		anim.AimBodyWeight = 1.0f;
		// anim.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Rifle );
		// anim.SetAnimParameter( "aim_body_weight", 1.0f );
	}
}

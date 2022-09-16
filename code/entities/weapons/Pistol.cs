using System.Runtime.InteropServices;

namespace DemoDash.entities.weapons;

[Library( "dd_pistol" ), HammerEntity]
[EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
[Title( "Pistol" ), Category( "Weapons" )]
partial class Pistol : DemoDashWeapon
{
	public static readonly Model WorldModel = Model.Load( "weapons/rust_pistol/rust_pistol.vmdl" );
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override float PrimaryRate => 12.0f;
	public override float SecondaryRate => 1.0f;
	public override float ReloadTime => 3.0f;

	public override int Bucket => 1;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		AmmoClip = 12;
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( InputButton.PrimaryAttack );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();

			if ( AvailableAmmo() > 0 )
			{
				Reload();
			}

			return;
		}

		ShootEffects();
		PlaySound( "rust_pistol.shoot" );

		ShootBullet( 0.05f, 1, 20.0f, 2.0f );
	}
}
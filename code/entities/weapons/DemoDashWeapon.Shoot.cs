using Sandbox;

namespace DemoDash.entities.weapons;

public partial class DemoDashWeapon
{
	[Net, Predicted]
	public Vector3 ShootStartPosition { get; set; }

	[Net, Predicted]
	public Vector3 ShootForwardRotation { get; set; }

	/// <summary>
	/// Shoot a single bullet.
	/// </summary>
	/// <param name="pos">starting position</param>
	/// <param name="dir">direction of the bullet</param>
	/// <param name="spread">amount of randomness for spreading the bullet</param>
	/// <param name="force">amount of force applied to what the bullet hits</param>
	/// <param name="damage">amount of damage the bullet deals</param>
	/// <param name="bulletSize">the bullet size</param>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;

		// Randomize spread
		forward += (Vector3.Random + Vector3.Random + Vector3.Random) * spread * .25f;
		forward = forward.Normal;

		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !tr.Entity.IsValid() || !IsServer )
				continue;

			// Turn prediction off so any exploding effects don't get culled.
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				Log.Info( $"ent[{tr.Entity.Name}] dmg[{damageInfo}]" );
				tr.Entity.TakeDamage( damageInfo );

				DebugOverlay.Sphere( tr.HitPosition, 12f, Color.White, 5f );
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet from the owner's viewpoint
	/// </summary>
	/// <param name="spread">amount of randomness for spreading the bullet</param>
	/// <param name="force">amount of force applied to what the bullet hits</param>
	/// <param name="damage">amount of damage the bullet deals</param>
	/// <param name="bulletSize">the bullet size</param>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		Rand.SetSeed( Time.Tick );

		// var cam = (Owner as Player).CameraMode;

		// if (IsClient) {
		// 	Log.Info( $"CLI campos[{cam.Position}]" );
		// 	ShootStartPosition = cam.Position;
		// 	ShootForwardRotation = cam.Rotation.Forward;
		// }
		// if (IsServer) {
		// 	Log.Info( $"SRV campos[{cam.Position}]" );
		// }

		// DebugOverlay.Sphere( ShootStartPosition, 20f, Color.Red, 5 );
		// ShootBullet( ShootStartPosition, ShootForwardRotation, spread, force, damage, bulletSize );


		ShootBullet( Owner.EyePosition, Owner.EyeRotation.Forward, spread, force, damage, bulletSize );
	}

	/// <summary>
	/// Shoots multiple bullets from the owner's viewpoint.
	/// This can be useful for shotguns.
	/// </summary>
	/// <param name="numBullets">number of bullets being shot</param>
	/// <param name="spread">amount of randomness for spreading the bullet</param>
	/// <param name="force">amount of force applied to what the bullet hits</param>
	/// <param name="damage">amount of damage the bullet deals</param>
	/// <param name="bulletSize">the bullet size</param>
	public virtual void ShootBullets( int numBullets, float spread, float force, float damage, float bulletSize )
	{
		var cam = (Owner as Player).CameraMode;
		var pos = cam.Position;
		var dir = cam.Rotation.Forward;

		for ( int i = 0; i < numBullets; i++ )
		{
			ShootBullet( pos, dir, spread, force / numBullets, damage, bulletSize );
		}
	}
}

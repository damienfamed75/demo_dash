using DemoDash.entities.weapons;
using Sandbox;

[Library("weapon_respawner"), HammerEntity]
[EditorModel("models/sbox_props/ceiling_light/ceiling_light.vmdl")]
[Title("Weapon Respawner"), Category("Respawner")]
public partial class WeaponRespawner : ModelEntity
{
	[Net]
	public DemoDashWeapon Weapon { get; set; }

	[Net, Predicted]
	TimeSince TimeSinceSpawn { get; set; }

	[Net]
	public SpotLightEntity SpotLight { get; set; }

	[Net]
	public RealTimeUntil TimeUntilRespawn { get; set; }

	[Net]
	public bool IsRespawning { get; set; }

	private readonly float WeaponBob = 300f;
	private readonly int WeaponRotSpeed = 1500;
	private readonly float Offset = 30f;

	public override void Spawn()
	{
		SetModel( "models/sbox_props/ceiling_light/ceiling_light.vmdl" );

		Weapon = PickWeapon( Rand.Int( 0, 2 ) );
		Weapon.Spawn();
		Weapon.PhysicsEnabled = false;
		Weapon.Position = Position + Vector3.Up * 20;

		SpotLight = new SpotLightEntity();
		SpotLight.Brightness = 0.5f;
		SpotLight.EnableShadowCasting = false;
		SpotLight.Rotation = Rotation.LookAt( Vector3.Up );
		SpotLight.Position = Position + Vector3.Up * 5;
		SpotLight.Color = (Color)Color.Parse( "#06c0fc" );
	}

	private static DemoDashWeapon PickWeapon(int i)
	{
		return i switch
		{
			0 => new Pistol(),
			1 => new Shotgun(),
			2 => new SMG(),
			_ => new Pistol(),
		};
	}

	[Event.Tick.Server]
	public async void OnTick()
	{
		if (Weapon == null && !IsRespawning) {
			await WaitForRespawn();
		}
		if (Weapon == null)
			return;
		// If the weapon was picked up by a player then re-enable physics.
		if (Weapon.Owner != null) {
			Weapon.PhysicsEnabled = true;
			Weapon = null;
			return;
		}
		// Rotate the weapon around the yaw
		Weapon.Rotation = Rotation.FromYaw( TimeSinceSpawn % 360f * Time.Delta * WeaponRotSpeed );
		// Bob the weapon up and down.
		var sTime = MathF.Sin( TimeSinceSpawn );
		// RenderColor = (Color)Color.Parse( "#0066fb" );
		Weapon.Position = Position.WithZ(Position.z + sTime * Time.Delta * WeaponBob ) + Vector3.Up * Offset;
	}

	public async Task WaitForRespawn()
	{
		IsRespawning = true;
		TimeUntilRespawn = 30 + Rand.Int( 10, 20 );
		while(TimeUntilRespawn > 0) {
			await Task.DelayRealtimeSeconds( 1.0f );
		}

		Respawn();
	}

	public void Respawn()
	{
		Weapon = PickWeapon( Rand.Int( 0, 2 ) );
		Weapon.PhysicsEnabled = false;
		TimeSinceSpawn = 0;
		IsRespawning = false;
	}
}
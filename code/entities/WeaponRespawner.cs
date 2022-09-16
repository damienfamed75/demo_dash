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

	private readonly float WeaponBob = 300f;
	private readonly int WeaponRotSpeed = 1500;
	private readonly float Offset = 30f;

	public override void Spawn()
	{
		SetModel( "models/sbox_props/ceiling_light/ceiling_light.vmdl" );
		// Respawn a weapon here.
		Respawn();
		// Create the spotlight entity below the weapon.
		SpotLight = new SpotLightEntity {
			Position = Position + Vector3.Up * 3,
			Rotation = Rotation.LookAt(Vector3.Up),
			Brightness = 0.5f,
			Color = (Color)Color.Parse("#06c0fc"),
			EnableShadowCasting = false,
		};
	}

	[Event.Tick.Server]
	public async void OnTick()
	{
		if (Weapon == null)
			return;

		// If the weapon was just picked up by a player then re-enable physics
		// on it and mark the weapon as gone.
		if (Weapon.Owner != null) {
			Weapon.PhysicsEnabled = true;
			// Set the weapon to null so then if a player decides to drop the
			// weapon before a new one is spawned, then it won't be teleported
			// back above this entity.
			Weapon = null;
			await WaitForRespawn();
		}
		// Rotate the weapon around the yaw
		Weapon.Rotation = Rotation.FromYaw( TimeSinceSpawn % 360f * Time.Delta * WeaponRotSpeed );
		// Bob the weapon up and down.
		var sTime = MathF.Sin( TimeSinceSpawn );
		// RenderColor = (Color)Color.Parse( "#0066fb" );
		Weapon.Position = Position.WithZ(Position.z + sTime * Time.Delta * WeaponBob ) + Vector3.Up * Offset;
	}

	/// <summary>
	/// Waits to respawn a new weapon here.
	/// </summary>
	public async Task WaitForRespawn()
	{
		TimeUntilRespawn = 30 + Rand.Int( 10, 20 );
		while(TimeUntilRespawn > 0) {
			await Task.DelayRealtimeSeconds( 1.0f );
		}

		Respawn();
	}

	/// <summary>
	/// Return a new weapon at the provided index.
	/// </summary>
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

	/// <summary>
	/// Respawn a new weapon above this entity.
	/// </summary>
	public void Respawn()
	{
		Weapon = PickWeapon( Rand.Int( 0, 2 ) );
		Weapon.PhysicsEnabled = false;
		TimeSinceSpawn = 0;
	}
}
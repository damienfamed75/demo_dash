using System.Threading.Tasks;
using Sandbox;

namespace DemoDash.player;

public partial class DemoDashPlayer : Player
{
	public DemoDashPlayer()
	{
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new WalkController{
            WalkSpeed = 270,
            SprintSpeed = 100,
            DefaultSpeed = 270,
            AirAcceleration = 0,
        };

		EnableHideInFirstPerson = true;
		EnableDrawing = true;
		EnableShadowInFirstPerson = true;

		Animator = new StandardPlayerAnimator();
		CameraMode = new ThirdPersonCamera();

		base.Respawn();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
	}

	public override void OnKilled()
	{
		base.OnKilled();
	}

	public override void Simulate( Client cl )
	{
        if (DemoDashGame.CurrentState == DemoDashGame.GameStates.GameEnd)
			return;

		base.Simulate( cl );

        // If view button is pressed, change camera mode.
        if (Input.Pressed(InputButton.View)) {
            if (CameraMode is ThirdPersonCamera) {
				CameraMode = new FirstPersonCamera();
			} else {
				CameraMode = new ThirdPersonCamera();
			}
        }
	}

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
	}

    public void RenderHud(Vector2 screenSize)
    {
        if (LifeState == LifeState.Dead)
			return;

        // TODO render weapon info.
	}
}
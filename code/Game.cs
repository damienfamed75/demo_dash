global using Editor;
global using Sandbox;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
using DemoDash.entities;
using DemoDash.player;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace DemoDash;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class DemoDashGame : GameManager
{
	// readonly StandardPostProcess postProcess;

	[Net]
	DemoDashHud Hud { get; set; }

	[Net, Change]
	public int NumClients { get; set; }

	private void OnNumClientsChanged(int oldVal, int newVal)
	{
		Log.Info( $"Number of clients changed. Before[{oldVal}] After[{newVal}]" );
	}

	public DemoDashGame()
	{
		if (IsServer) {
			Hud = new DemoDashHud();

			_ = GameLoopAsync();
		}
		if (IsClient) {
			// postProcess = new StandardPostProcess();
			// PostProcess.Add( postProcess );
		}
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		// var pawn = new Pawn();
		// client.Pawn = pawn;
		var pawn = new DemoDashPlayer(client);
		pawn.Respawn();
		client.Pawn = pawn;
		// FirstPersonCamera

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null ) {
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			pawn.Transform = tx;
		}

		NumClients++;
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );
		NumClients--;
	}

	public override void RenderHud()
	{
		// base.RenderHud();
		var localPawn = Game.LocalPawn as DemoDashPlayer;
		if (localPawn == null)
			return;

		var scale = Screen.Height / 1080.0f;
		var screenSize = Screen.Size / scale;
		var matrix = Matrix.CreateScale( scale );

		// using (Render.Draw2D.MatrixScope(matrix)) {
		// localPawn.Render
		// }
	}

	[ClientRpc]
	public override void OnKilledMessage( long leftid, string left, long rightid, string right, string method )
	{
		Sandbox.UI.KillFeed.Current?.AddEntry( leftid, left, rightid, right, method );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		// var hook = Map.Camera.FindOrCreateHook<DemoDashPostProcessEffect>();

		// hook.Enabled = true;

		// postProcess.Blur.Enabled = false;

		// postProcess.Saturate.Enabled = true;
		// postProcess.Saturate.Amount = 1f;

		// postProcess.ChromaticAberration.Enabled = true;
		// postProcess.ChromaticAberration.Offset = 0.004f;

		// postProcess.FilmGrain.Enabled = false;
		// postProcess.FilmGrain.Intensity = 0f;
		// postProcess.FilmGrain.Response = 0f;

		// if (Local.Pawn is DemoDashPlayer player) {
		// 	var timeSinceDamage = player.TimeSinceDamage.Relative / 2;
		// 	var damageUI = timeSinceDamage.LerpInverse( 0.5f, 0.0f, true ) * 0.2f;

		// 	// If the player was recently damaged, then apply these effects.
		// 	if (damageUI > 0) {
		// 		postProcess.Saturate.Amount -= damageUI;

		// 		postProcess.Blur.Enabled = true;
		// 		postProcess.Blur.Strength = damageUI * 0.5f;

		// 		postProcess.ChromaticAberration.Enabled = true;
		// 		postProcess.ChromaticAberration.Offset += damageUI / 50;
		// 	}

		// 	var lowHealthUI = player.Health.LerpInverse( 50.0f, 0.0f, true );
		// 	if (player.LifeState == LifeState.Dead)
		// 		lowHealthUI = 0;

		// 	// If the player is nearing death, then apply these effects.
		// 	if (lowHealthUI > 0) {
		// 		postProcess.Saturate.Amount -= lowHealthUI;

		// 		postProcess.Blur.Enabled = true;
		// 		postProcess.Blur.Strength = lowHealthUI * 0.10f;

		// 		Audio.SetEffect( "core.player.death.muffle1", lowHealthUI * 0.8f, 2.0f );
		// 	}
		// }
		// // If game state is a warmup, then grey the screen out and add filmgrain.
		// if (CurrentState == GameStates.Warmup) {
		// 	postProcess.FilmGrain.Enabled = true;
		// 	postProcess.FilmGrain.Intensity = .5f;
		// 	postProcess.FilmGrain.Response = 0.5f;
		// 	postProcess.Saturate.Amount = 0.7f;
		// }
	}

	public override void Shutdown()
	{
		All.OfType<WeaponRespawner>().ToList().ForEach( x => x.Delete() );
		base.Shutdown();
	}
}

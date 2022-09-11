global using SandboxEditor;
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
public partial class DemoDashGame : Sandbox.Game
{
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
		}
		if (IsClient) {

		}
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		// var pawn = new Pawn();
		// client.Pawn = pawn;
		var pawn = new player.DemoDashPlayer();
		pawn.Respawn();
		client.Pawn = pawn;
		// FirstPersonCamera

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			pawn.Transform = tx;
		}

		NumClients++;
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );
		NumClients--;
	}

	public override void RenderHud()
	{
		// base.RenderHud();
		var localPawn = Local.Pawn as DemoDashPlayer;
		if (localPawn == null)
			return;

		var scale = Screen.Height / 1080.0f;
		var screenSize = Screen.Size / scale;
		var matrix = Matrix.CreateScale( scale );

		using (Render.Draw2D.MatrixScope(matrix)) {
			// localPawn.Render
		}
	}
}

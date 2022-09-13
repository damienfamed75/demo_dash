using DemoDash.player;

namespace DemoDash;

public partial class DemoDashGame
{
	public static GameStates CurrentState => (Current as DemoDashGame)?.GameState ?? GameStates.Warmup;

	[Net]
	public RealTimeUntil StateTimer { get; set; } = 0f;

	[Net]
	public GameStates GameState { get; set; } = GameStates.Warmup;

    [Net]
	public string NextMap { get; set; } = "";

	public enum GameStates
	{
	    Warmup,
	    Live,
	    GameEnd,
	    MapVote
	}

	[ConCmd.Admin]
	public static void SkipStage()
	{
		if (Current is not DemoDashGame ddg)
			return;

		ddg.StateTimer = 1;
	}

	private async Task GameLoopAsync()
	{
		while(!HasEnoughPlayers()) {
			GameState = GameStates.Warmup;
			StateTimer = 10;
			await WaitStateTimer();
		}

		GameState = GameStates.Live;
		StateTimer = 10 * 60;
		FreshStart();
		await WaitStateTimer();

		GameState = GameStates.GameEnd;
		StateTimer = 10;
		await WaitStateTimer();

		GameState = GameStates.MapVote;
		// var mapVote = new MapVoteEntity();
		await WaitStateTimer();

		// Global.ChangeLevel( mapVote.WinningMap );
	}

	/// <summary>
	/// Waits for a state to pass.
	/// </summary>
	private async Task WaitStateTimer()
	{
		while (StateTimer > 0) {
			await Task.DelayRealtimeSeconds( 1.0f );
		}
		// Extra second, "for fun."
		await Task.DelayRealtimeSeconds( 1.0f );
	}

	/// <summary>
	/// Check if there's at least 2 players.
	/// </summary>
	public bool HasEnoughPlayers()
	{
		if (All.OfType<DemoDashPlayer>().Count() < 2)
			return false;

		return true;
	}

	private void FreshStart()
	{
		// Reset all the client's scores.
		foreach (var cli in Client.All) {
			cli.SetInt( "kills", 0 );
			cli.SetInt( "deaths", 0 );
			cli.SetInt( "score", 0 );
		}
		// Respawn all the players.
		All.OfType<DemoDashPlayer>().ToList().ForEach( x => x.Respawn() );
	}
}
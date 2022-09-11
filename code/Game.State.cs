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
}
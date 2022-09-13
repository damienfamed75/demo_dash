using DemoDash;
using Sandbox.UI;
using Sandbox.UI.Construct;

internal class GameHud : Panel
{
	public Label Timer;
	public Label State;

	public GameHud()
	{
		State = AddChild<Label>( "game-state" );
		Timer = AddChild<Label>( "game-timer" );
	}

	public override void Tick()
	{
		base.Tick();

		var game = Game.Current as DemoDashGame;
		if (!game.IsValid())
			return;

		var span = TimeSpan.FromSeconds( (game.StateTimer * 60).Clamp( 0, float.MaxValue ) );

		if (!game.HasEnoughPlayers()) {
			Timer.Text = span.ToString( @"hh\:mm" );
			State.Text = "Need 2 players";
		} else {
			Timer.Text = span.ToString( @"hh\:mm" );
			State.Text = game.GameState.ToString();
		}

	}
}
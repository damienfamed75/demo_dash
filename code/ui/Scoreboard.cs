using DemoDash;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace DemoDash.ui;

public class Scoreboard : Sandbox.UI.Scoreboard<ScoreboardEntry>
{
	protected override void AddHeader()
	{
		Header = AddChild<Panel>( "header" );
		Header.AddChild<Label>( "name" ).SetText( "Name" );
		Header.AddChild<Label>( "kills" ).SetText( "Kills" );
		Header.AddChild<Label>( "deaths" ).SetText( "Deaths" );
		Header.AddChild<Label>( "ping" ).SetText( "Ping" );
		Header.AddChild<Label>( "score" ).SetText( "Score" );
	}

	RealTimeSince timeSinceSorted;

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible )
			return;

		if ( timeSinceSorted > 0.1f )
		{
			timeSinceSorted = 0;

			// Sort by score and then number of deaths.
			Canvas.SortChildren<ScoreboardEntry>( ( x ) => (-x.Client.GetInt( "score" ) * 1000) + x.Client.GetInt( "deaths" ) );
		}
	}

	public override bool ShouldBeOpen()
	{
		if ( DemoDashGame.CurrentState == DemoDashGame.GameStates.GameEnd )
			return true;

		return base.ShouldBeOpen();
	}
}
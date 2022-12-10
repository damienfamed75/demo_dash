using Sandbox;
// using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace DemoDash.ui;

public partial class ScoreboardEntry : Sandbox.UI.ScoreboardEntry
{
	// public Client Client;

	// public Label PlayerName;
	// public Label Kills;
	// public Label Deaths;
	// public Label Ping;

	public Label Score;

	public ScoreboardEntry() : base()
	{
		// AddClass( "entry" );

		// PlayerName = AddChild<Label>( "name" );
		// Kills = AddChild<Label>( "kills" );
		// Deaths = AddChild<Label>( "deaths" );
		// Ping = AddChild<Label>( "ping" );

		Score = AddChild<Label>( "score" );
	}

	RealTimeSince TimeSinceUpdate = 0;

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible )
			return;

		if ( !Client.IsValid() )
			return;

		if ( TimeSinceUpdate < 0.1f )
			return;

		TimeSinceUpdate = 0;
		UpdateData();
	}

	public override void UpdateData()
	{
		// PlayerName.Text = Client.Name;
		// Kills.Text = Client.GetInt( "kills" ).ToString();
		// Deaths.Text = Client.GetInt( "deaths" ).ToString();
		// Ping.Text = Client.Ping.ToString();
		// SetClass( "me", Client == Local.Client );
		Score.Text = Client.GetInt( "score" ).ToString();
		base.UpdateData();
	}
}

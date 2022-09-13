using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace DemoDash.ui;

public class RootHud : RootPanel
{
	public static RootHud Current;

	public Scoreboard Scoreboard { get; set; }

	public RootHud()
    {
		Current = this;

		StyleSheet.Load( "/resource/styles/hud.scss" );
		SetTemplate( "/resource/templates/hud.html" );

		AddChild<Crosshair>();
		AddChild<InventoryBar>();
		// AddChild<Ammo>();
		// AddChild<Health>();
		AddChild<KillFeed>();
		Scoreboard = AddChild<Scoreboard>();
	}

	public override void Tick()
	{
		base.Tick();
	}
}
using DemoDash.ui;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class RootHud : RootPanel
{
	public static RootHud Current;

	public RootHud()
    {
		Current = this;

		StyleSheet.Load( "/resource/styles/hud.scss" );
		SetTemplate( "/resource/templates/hud.html" );

		AddChild<Crosshair>();
	}
}
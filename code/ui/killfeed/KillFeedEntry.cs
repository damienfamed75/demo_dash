using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Sandbox.UI;

public partial class KillFeedEntry : Panel
{
	public Label Left { get; internal set; }
	public Label Right { get; internal set; }
	public Label Method { get; internal set; }

	public RealTimeSince TimeSinceBorn = 0;

	public KillFeedEntry()
	{
		Left = AddChild<Label>( "left" );
		Right = AddChild<Label>( "method" );
		Method = AddChild<Label>( "right" );
	}

	public override void Tick()
	{
		base.Tick();

		if (TimeSinceBorn > 5) {
			Delete();
		}
	}
}
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Sandbox.UI;

public partial class KillFeed : Panel
{
	public static KillFeed Current;

	public KillFeed()
	{
		Current = this;

		StyleSheet.Load( "/ui/killfeed/KillFeed.scss" );
	}

	public virtual Panel AddEntry(long lsteamid, string left, long rsteamid, string right, string method)
	{
		var entry = Current.AddChild<KillFeedEntry>();

		entry.Left.Text = left;
		entry.Left.SetClass( "me", lsteamid == (Local.Client?.PlayerId) );

		entry.Method.Text = method;

		entry.Right.Text = right;
		entry.Right.SetClass( "me", rsteamid == (Local.Client?.PlayerId) );

		return entry;
	}
}
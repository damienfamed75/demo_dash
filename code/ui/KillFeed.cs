using Sandbox.UI;

public partial class KillFeed : Sandbox.UI.KillFeed
{
	public override Panel AddEntry( long lsteamid, string left, long rsteamid, string right, string method )
	{
		Log.Info( $"{left} killed {right} using {method}" );

		var entry = Current.AddChild<KillFeedEntry>();

		entry.AddClass( method );
		entry.Left.Text = left;
		entry.Left.SetClass( "me", lsteamid == Game.SteamId );
		
		entry.Right.Text = right;
		entry.Right.SetClass( "me", rsteamid == Game.SteamId );

		return entry;
	}
}

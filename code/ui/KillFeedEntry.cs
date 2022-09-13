using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox.UI.Construct;

public partial class KillFeedEntry : Panel
{
	public Label Left { get; internal set; }
	public Label Right { get; internal set; }
	public Panel Icon { get; internal set; }

	public KillFeedEntry()
	{
		Left = AddChild<Label>( "left" );
		Icon = AddChild<Panel>( "icon" );
		Right = AddChild<Label>( "right" );

		_ = RunAsync();
	}

	async Task RunAsync()
	{
		await Task.Delay( 4000 );
		Delete();
	}
}
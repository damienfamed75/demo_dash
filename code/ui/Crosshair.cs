using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace DemoDash.ui;

class Crosshair : Panel
{
	public Crosshair Current;

    public Crosshair()
    {
		Current = this;
		// StyleSheet.Load( "/ui/Crosshair.scss" );
	}
}
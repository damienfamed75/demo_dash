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
	}

	public override void Tick()
	{
		// Move to where the crosshair is.
		this.PositionAtCrosshair();
	}
}
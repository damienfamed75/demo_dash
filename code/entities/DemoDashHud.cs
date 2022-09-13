using DemoDash.player;
using DemoDash.ui;

namespace DemoDash.entities;

public partial class DemoDashHud : HudEntity<RootHud>
{
	[ClientRpc]
	public void OnPlayerDied(DemoDashPlayer player)
	{
		Host.AssertClient();
	}

	[ClientRpc]
	public void ShowDeathScreen(string attackerName)
	{
		Host.AssertClient();
	}
}
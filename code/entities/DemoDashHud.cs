using DemoDash.player;
using DemoDash.ui;

namespace DemoDash.entities;

public partial class DemoDashHud : HudEntity<RootHud>
{
	[ClientRpc]
	public void OnPlayerDied(DemoDashPlayer player)
	{
		Game.AssertClient();
	}

	[ClientRpc]
	public void ShowDeathScreen(string attackerName)
	{
		Game.AssertClient();
	}
}

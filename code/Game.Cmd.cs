global using SandboxEditor;
global using Sandbox;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;

using DemoDash.entities.weapons;
using DemoDash.player;

namespace DemoDash;

public partial class DemoDashGame
{
    [ConCmd.Server("set_ammo")]
    public static void Ammo(int amount)
    {
		var player = ConsoleSystem.Caller.Pawn as DemoDashPlayer;
		var weapon = player.ActiveChild as DemoDashWeapon;
		player.SetAmmo( weapon.AmmoType, amount );
	}

	[ConCmd.Server("damage")]
	public static void Damage(int amount)
	{
		var player = ConsoleSystem.Caller.Pawn as Player;
		var damageInfo = DamageInfo.Generic( amount );
		player.TakeDamage( damageInfo );
	}

	[ConCmd.Server("heal")]
	public static void Heal(int amount = 100)
	{
		var player = ConsoleSystem.Caller.Pawn as DemoDashPlayer;
		player.GiveHealth( amount );
	}
}
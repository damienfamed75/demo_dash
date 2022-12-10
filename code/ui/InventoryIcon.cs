using Sandbox.UI;
using DemoDash.entities.weapons;

namespace DemoDash.ui;

class InventoryIcon : Panel
{
	public DemoDashWeapon Weapon;
	public Panel Icon;

	public InventoryIcon( DemoDashWeapon weapon )
	{
		Weapon = weapon;
		Icon = Add.Panel( "icon" );

		AddClass( weapon.ClassName );
	}

	internal void TickSelection( DemoDashWeapon selectedWeapon )
	{
		SetClass( "active", selectedWeapon == Weapon );
		SetClass( "empty", !Weapon?.IsUsable() ?? true );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Weapon.IsValid() || Weapon.Owner != Game.LocalPawn )
			Delete( true );
	}
}

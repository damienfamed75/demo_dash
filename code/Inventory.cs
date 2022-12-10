using DemoDash.player;
using Sandbox;
using System;
using System.Linq;
using System.Net;
using DemoDash.util;

partial class Inventory : DemoDash.util.BaseInventory
{
	public Inventory(DemoDashPlayer player) : base(player)
	{
	}

	public override bool CanAdd( Entity ent )
	{
		// if the entity is invalid, return false.
		if ( !ent.IsValid())
			return false;

		// If the base cannot add the item, return false.
		if ( !base.CanAdd( ent ) )
			return false;

		// return if carrying type that's the same.
		return !IsCarryingType( ent.GetType() );
	}

	public override bool Add( Entity ent, bool makeActive = false )
	{
		// if the entity is invalid, return false.
		if ( !ent.IsValid() )
			return false;

		// if carrying the same kind of item, return false.
		if ( IsCarryingType( ent.GetType() ) )
			return false;

		return base.Add( ent, makeActive );
	}

	public bool IsCarryingType(Type t)
	{
		// Returns entities that are of type T
		return List.Any( x => x?.GetType() == t );
	}

	public override bool Drop( Entity ent )
	{
		if ( !Game.IsServer )
			return false;
		// If the player doesn't even contain this item in their inventory, return false.
		if ( !Contains( ent ) )
			return false;

		if (ent is DemoDash.util.BaseCarriable bc) {
			bc.OnCarryDrop( Owner );
		}

		return ent.Parent == null;
	}
}

using System.Numerics;
using DemoDash.player;

namespace DemoDash.entities.weapons;

partial class DemoDashWeapon
{
    /// <summary>
	/// OnUse is the interaction between the player and this weapon.
	/// </summary>
    public bool OnUse(Entity user)
    {
		// If this weapon is already owned by someone then it's unusable.
		if (Owner != null)
			return false;
        // If the user is invalid then do nothing.
        if (!user.IsValid())
			return false;
        // Begin interaction.
		user.StartTouch( this );

		return false;
	}

    public bool IsUsable(Entity user)
    {
        var player = user as DemoDashPlayer;

        if (Owner != null)
			return false;

        if (player.Inventory is Inventory inventory)
			return inventory.CanAdd( this );

		return true;
	}

    public bool IsUsable()
    {
		if (AmmoClip > 0)
			return true;

		if (AmmoType == AmmoType.None)
			return true;

		return AvailableAmmo() > 0;
    }
}

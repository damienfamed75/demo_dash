using Sandbox.UI;
using DemoDash.entities.weapons;
using DemoDash.player;

namespace DemoDash.ui;

/// <summary>
/// The main inventory panel, top left of the screen.
/// </summary>
public class InventoryBar : Panel
{
	List<InventoryColumn> columns = new();
	List<DemoDashWeapon> Weapons = new();

	public bool IsOpen;
	DemoDashWeapon SelectedWeapon;

	public InventoryBar()
	{
		for ( int i = 0; i < 6; i++ )
		{
			var icon = new InventoryColumn( i, this );
			columns.Add( icon );
		}
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "active", IsOpen );

		var player = Game.LocalPawn as Player;
		if ( player == null ) return;

		Weapons.Clear();
		Weapons.AddRange( player.Children.Select( x => x as DemoDashWeapon ).Where( x => x.IsValid() && x.IsUsable() ) );

		foreach ( var weapon in Weapons )
		{
			columns[weapon.Bucket].UpdateWeapon( weapon );
		}
	}

	/// <summary>
	/// IClientInput implementation, calls during the client input build.
	/// You can both read and write to input, to affect what happens down the line.
	/// </summary>
	// [Event.BuildInput]

	[Event("buildinput")]
	public void ProcessClientInput()
	{
		// if ( DemoDashGame.CurrentState != DemoDashGame.GameStates.Live ) return;

		bool wantOpen = IsOpen;
		var localPlayer = Game.LocalPawn as DemoDashPlayer;

		// If we're not open, maybe this input has something that will 
		// make us want to start being open?
		wantOpen = wantOpen || Input.MouseWheel != 0;
		wantOpen = wantOpen || Input.Pressed( InputButton.Slot1 );
		wantOpen = wantOpen || Input.Pressed( InputButton.Slot2 );
		wantOpen = wantOpen || Input.Pressed( InputButton.Slot3 );
		wantOpen = wantOpen || Input.Pressed( InputButton.Slot4 );
		wantOpen = wantOpen || Input.Pressed( InputButton.Slot5 );
		wantOpen = wantOpen || Input.Pressed( InputButton.Slot6 );

		if ( Weapons.Count == 0 )
		{
			IsOpen = false;
			return;
		}

		// We're not open, but we want to be
		if ( IsOpen != wantOpen )
		{
			SelectedWeapon = localPlayer?.ActiveChild as DemoDashWeapon;
			IsOpen = true;
		}

		// Not open fuck it off
		if ( !IsOpen ) return;

		//
		// Fire pressed when we're open - select the weapon and close.
		//
		if ( Input.Down( InputButton.PrimaryAttack ) )
		{
			Input.SuppressButton( InputButton.PrimaryAttack );
			// input.ActiveChild = SelectedWeapon;
			//! TODO this line doesn't work.
			(Game.LocalPawn as DemoDashPlayer).ActiveChild = SelectedWeapon;
			IsOpen = false;
			Sound.FromScreen( "ui.button.press" );
			return;
		}

		var sortedWeapons = Weapons.OrderBy( x => x.Order ).ToList();

		// get our current index
		var oldSelected = SelectedWeapon;
		int SelectedIndex = sortedWeapons.IndexOf( SelectedWeapon );
		SelectedIndex = SlotPressInput( SelectedIndex, sortedWeapons );

		// forward if mouse wheel was pressed
		SelectedIndex -= Input.MouseWheel;
		SelectedIndex = SelectedIndex.UnsignedMod( Weapons.Count );

		SelectedWeapon = sortedWeapons[SelectedIndex];

		for ( int i = 0; i < 6; i++ )
		{
			columns[i].TickSelection( SelectedWeapon );
		}

		Input.MouseWheel = 0;

		if ( oldSelected != SelectedWeapon )
		{
			Sound.FromScreen( "ui.navigate.deny" );
		}
	}

	int SlotPressInput( int SelectedIndex, List<DemoDashWeapon> sortedWeapons )
	{
		var columninput = -1;

		if ( Input.Pressed( InputButton.Slot1 ) ) columninput = 0;
		if ( Input.Pressed( InputButton.Slot2 ) ) columninput = 1;
		if ( Input.Pressed( InputButton.Slot3 ) ) columninput = 2;
		if ( Input.Pressed( InputButton.Slot4 ) ) columninput = 3;
		if ( Input.Pressed( InputButton.Slot5 ) ) columninput = 4;
		if ( Input.Pressed( InputButton.Slot6 ) ) columninput = 5;

		if ( columninput == -1 ) return SelectedIndex;

		if ( SelectedWeapon.IsValid() && SelectedWeapon.Bucket == columninput )
		{
			return NextInBucket( sortedWeapons );
		}

		// Are we already selecting a weapon with this column?
		var firstOfColumn = sortedWeapons.Where( x => x.Bucket == columninput ).FirstOrDefault();
		if ( firstOfColumn == null )
		{
			// DOOP sound
			return SelectedIndex;
		}

		return sortedWeapons.IndexOf( firstOfColumn );
	}

	int NextInBucket( List<DemoDashWeapon> sortedWeapons )
	{
		if (SelectedWeapon == null) {
			return 0;
		}

		DemoDashWeapon first = null;
		DemoDashWeapon prev = null;
		foreach ( var weapon in sortedWeapons.Where( x => x.Bucket == SelectedWeapon.Bucket ) )
		{
			if ( first == null ) first = weapon;
			if ( prev == SelectedWeapon ) return sortedWeapons.IndexOf( weapon );
			prev = weapon;
		}

		return sortedWeapons.IndexOf( first );
	}
}

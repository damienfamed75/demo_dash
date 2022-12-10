using DemoDash.entities.weapons;
using DemoDash.player;
using Sandbox.UI;

public class Ammo : Panel
{
	public Label Label;
	public Label Total;

	public Ammo()
    {
		Label = AddChild<Label>();
		Total = AddChild<Label>( "ammo-total" );
	}

    public override void Tick()
    {
		var player = Game.LocalPawn;
        if (player == null)
			return;

		var dplayer = player as DemoDashPlayer;
        if (dplayer.ActiveChild is DemoDashWeapon weapon) {
			Style.Opacity = 1f;
			Style.BackgroundColor = Color.Parse( "#704603" ).Value.WithAlpha( 1f );
			var total = dplayer.AmmoCount( weapon.AmmoType );
			Label.Text = $"{weapon.AmmoClip}";
			Total.Text = $"{total}";
		} else {
			Style.Opacity = 0.0f;
		}
	}
}

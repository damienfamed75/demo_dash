using DemoDash.player;

namespace Sandbox.UI
{
	public static class PanelExtension
	{
		public static void PositionAtCrosshair( this Panel panel )
		{
			panel.PositionAtCrosshair( Game.LocalPawn as DemoDashPlayer );
		}

		public static void PositionAtCrosshair( this Panel panel, DemoDashPlayer player )
		{
			if ( !player.IsValid() ) return;

			var eyePos = player.EyePosition;
			var eyeRot = player.EyeRotation;

			var tr = Trace.Ray( eyePos, eyePos + eyeRot.Forward * 1000 )
				.Size( 1.0f )
				.Ignore( player )
				.UseHitboxes()
				.Run();

			panel.PositionAtWorld( tr.EndPosition );
		}

		public static void PositionAtWorld( this Panel panel, Vector3 position )
		{
			var screenPos = position.ToScreen();

			if ( screenPos.z < 0 )
				return;

			panel.Style.Left = Length.Fraction( screenPos.x );
			panel.Style.Top = Length.Fraction( screenPos.y );
			panel.Style.Dirty();
		}
	}
}

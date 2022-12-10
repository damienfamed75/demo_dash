[SceneCamera.AutomaticRenderHook]
public partial class DemoDashPostProcessEffect : RenderHook
{
	RenderAttributes attributes = new RenderAttributes();
	Material effectMaterial = Material.Load( "materials/ui/basic.vmat" );

	public override void OnStage( SceneCamera target, Stage renderStage )
	{
		// base.OnStage( target, renderStage );

		// Don't render if local pawn is dead
		if (Game.LocalPawn.Health <= 0)
			return;

		if (renderStage == Stage.BeforePostProcess) {
			attributes.Set( "Saturate", true );
			attributes.Set( "Saturate", 1.0f );

			attributes.Set( "CameraFOV", target.FieldOfView.DegreeToRadian()/2 );

			Graphics.GrabFrameTexture( "ColorBuffer", attributes );

			Graphics.Blit( effectMaterial, attributes );
			// Graphics.Blit()
			// target.Attributes.Set( "saturate", true );
			// target.Attributes.Set( "saturate", 1.0f );

			// Graphics.Blit()
			// Log.Info( attributes.ToString() );
		}


	}
}

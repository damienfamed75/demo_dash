using System.Numerics;
using DemoDash.player;

namespace DemoDash.entities.weapons;

partial class DemoDashWeapon {
	/// <summary>
	/// ActiveStart is called when this weapon becomes the owner's ActiveChild.
	/// </summary>
	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );
		TimeSinceDeployed = 0;
		IsReloading = false;

		ActiveStartEffects();
	}

    [ClientRpc]
    public void ActiveStartEffects()
    {
        if (ViewModelEntity != null)
			ViewModelEntity.SetAnimParameter( "deploy", true );
    }

    /// <summary>
	/// Take the ViewModelPath and create a new "carrying" weapon model.
	/// </summary>
	public override void CreateViewModel()
	{
		Host.AssertClient();
        // Check that the viewmodel path is not invalid.
        if (string.IsNullOrEmpty(ViewModelPath))
			return;

        // Create a new empty viewmodel and set the owner.
		ViewModelEntity = new ViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true,
		};
        // Set the viewmodel's model.
		ViewModelEntity.SetModel( ViewModelPath );
	}
}
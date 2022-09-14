using Sandbox;
using Sandbox.UI;

namespace DemoDash.ui;

public class Score : WorldPanel
{
	RealTimeSince TimeSinceBorn;

	public Score(int score)
	{
		StyleSheet.Load( "/resource/styles/score.scss" );
		AddChild<Label>().SetText($"+{score}");
		TimeSinceBorn = 0;
	}

	public override void Tick()
	{
		base.Tick();

		if (TimeSinceBorn > 2.0f) {
			Delete();
		}
	}
}
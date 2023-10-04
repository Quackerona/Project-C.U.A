using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System;

public partial class OptionsBehavior : Node2D
{
	[Export]
	Array<Node> options;

	int curSelection;

	Sprite2D background;
	ParallaxBackground parallax;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		background = GetNode<Sprite2D>("BgLayers/Bg");
		parallax = GetNode<ParallaxBackground>("BgLayers/ParallaxBg");

		switchSelection(0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("uiDown"))
			switchSelection(1);
		if (Input.IsActionJustPressed("uiUp"))
			switchSelection(-1);
		if (Input.IsActionJustPressed("uiAccept"))
		{
			if (options[curSelection].GetType().Equals(typeof(Checkbox)))
				((Checkbox)options[curSelection]).accept();
			if (options[curSelection].GetType().Equals(typeof(Controls)))
				((Controls)options[curSelection]).accept();
		}
		if (Input.IsActionJustPressed("uiEscape"))
			GetTree().ChangeSceneToFile("res://Scenes/OptionMenus/OptionCategoriesMenu.tscn");
			
		parallax.ScrollOffset -= new Vector2(50f * (float)delta, 0);
	}

	void switchSelection(int hit)
	{
		curSelection += hit;

		if (curSelection >= options.Count)
			curSelection = 0;
		if (curSelection < 0)
			curSelection = options.Count - 1;

		for (int i = 0; i < options.Count; i++)
		{
			if (i == curSelection)
			{
				options[i].GetNode<Label>("Label").Modulate = new Color("yellow");
				continue;
			}
			options[i].GetNode<Label>("Label").Modulate = new Color("white");
		}
	}
}

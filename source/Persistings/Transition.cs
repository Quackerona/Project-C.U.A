using Godot;
using System;

public partial class Transition : Node
{
	// Called when the node enters the scene tree for the first time.
	
	TextureRect gradient;
	float expectedAmt;
	public override void _Ready()
	{
		gradient = GetNode<TextureRect>("CanvasLayer/Gradient");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		gradient.Scale = new Vector2(Mathf.Lerp(gradient.Scale.X, expectedAmt, (float)delta / (1f/60f) * 0.1f), 1);
	}

	public void Play(bool reveal = false)
	{
		if (!reveal) expectedAmt = 3;
		else expectedAmt = 0;
	}
}

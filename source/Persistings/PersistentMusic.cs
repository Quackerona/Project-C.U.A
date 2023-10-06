using Godot;
using System;

public partial class PersistentMusic : Node
{
	public AudioStreamPlayer2D mainMenuMusic;
	public AudioStreamPlayer2D confirmNoise;
	public AudioStreamPlayer2D scrollNoise;
	public AudioStreamPlayer2D cancelNoise;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mainMenuMusic = GetNode<AudioStreamPlayer2D>("MainMenuMusic");
		confirmNoise = GetNode<AudioStreamPlayer2D>("Confirm");
		scrollNoise = GetNode<AudioStreamPlayer2D>("Scroll");
		cancelNoise = GetNode<AudioStreamPlayer2D>("Cancel");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

using Discord;
using Godot;
using System;

public partial class TitleBehavior : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Settings settings = GetNode<Settings>("/root/Settings");
		ConfigFile config = new ConfigFile();
		config.Load("user://Settings.cfg");
		
		foreach (string key in config.GetSectionKeys("PlayerSettings"))
		{
			try {
				settings.Set(key, config.GetValue("PlayerSettings", key));
			}
			catch{}
		}

		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}

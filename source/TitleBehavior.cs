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
		
		foreach (string section in config.GetSections())
		{
			switch (section)
			{
				case "PlayerSettings":
					foreach (string key in config.GetSectionKeys(section))
					{
						try{
							settings.Set(key, config.GetValue(section, key));
						}
						catch{};
					}
					break;

				case "PlayerControls":
					foreach (string key in config.GetSectionKeys(section))
					{
						try{
							InputMap.ActionEraseEvents(key);
							InputMap.ActionAddEvent(key, (InputEvent)config.GetValue(section, key));
						}
						catch{};
					}
					break;
			}
		}

		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}

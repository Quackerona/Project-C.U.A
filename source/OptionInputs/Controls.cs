using Godot;
using System;

public partial class Controls : Node
{
	[Export]
	string action;

	bool shouldChange;

	Label indicator;
	
	Settings settings;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		settings = GetNode<Settings>("/root/Settings");

		indicator = GetNode<Label>("Label");
		indicator.Text = OS.GetKeycodeString(((InputEventKey)InputMap.ActionGetEvents(action)[0]).PhysicalKeycode).ToUpper(); //silly ass code but no other way to do it lol
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void accept()
	{
		shouldChange = true;
		indicator.Text = "?";
	}

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

		if (shouldChange)
		{
			if (@event is InputEventKey key)
			{
				if (key.IsPressed())
				{
					InputMap.ActionEraseEvents(action);
					InputMap.ActionAddEvent(action, @event);

					settings.config.SetValue("PlayerControls", action, @event);
					settings.config.Save("user://Settings.cfg");

					indicator.Text = OS.GetKeycodeString(key.PhysicalKeycode).ToUpper();
					shouldChange = false;
				}
			}
		}
    }
}

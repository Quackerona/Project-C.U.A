using Godot;
using System;

public partial class Checkbox : Node, IOption //could've just make this script for the child but it's easier for ppl to understand if i make it the parent
{
	[Export]
	string settingName;

	AnimatedSprite2D checkbox;

	bool selected;

	Settings settings;

    public override void _Ready()
    {
        base._Ready();
		settings = GetNode<Settings>("/root/Settings");
		checkbox = GetNode<AnimatedSprite2D>("Checkbox");

		modifyConfig(OptionsBehavior.config);

		if (selected != (bool)settings.Get(settingName))
			accept();
    }

    public void accept()
	{
		selected = !selected;

		if (selected)
		{
			checkbox.Play("selected");
			checkbox.Offset = new Vector2(-5.75f, -39.095f);
		}
		else
		{
			checkbox.Play("unselected");
			checkbox.Offset = new Vector2();
		}

		settings.Set(settingName, selected);
		modifyConfig(OptionsBehavior.config);
	}

    public void modifyConfig(ConfigFile config)
    {
        config.SetValue("PlayerSettings", settingName, selected);
    }

}

using Godot;
using System;
using System.Collections.Generic;

public partial class Note : Sprite2D
{
	Dictionary<string, Texture2D> noteTex;
	public int noteData;
    public float strumTime;
	public bool shouldHit;
    public bool isSustain;
    public bool isSustainEnd;

	Settings settings;

	public Vector2 RecScale {
		set {
			Scale = value;
			if (isSustain)
			{
				if (!isSustainEnd)
					Scale = new Vector2(value.X, value.Y * Conductor.stepCrochet / 100f * 1.48f * PlayBehavior.instance.SONG.song.speed);
			}
		}
	}

	public override void _Ready()
	{
		settings = GetNode<Settings>("/root/Settings");

		noteTex = new Dictionary<string, Texture2D>();
		
		noteTex.Add("purple", ResourceLoader.Load<Texture2D>("res://assets/images/noteSkins/default/purple.png"));
		noteTex.Add("green", ResourceLoader.Load<Texture2D>("res://assets/images/noteSkins/default/green.png"));
		noteTex.Add("red", ResourceLoader.Load<Texture2D>("res://assets/images/noteSkins/default/red.png"));
		noteTex.Add("blue", ResourceLoader.Load<Texture2D>("res://assets/images/noteSkins/default/blue.png"));

		noteTex.Add("purple hold piece", ResourceLoader.Load<Texture2D>("res://assets/images/noteSkins/default/purple hold piece.png"));
		noteTex.Add("green hold piece", ResourceLoader.Load<Texture2D>("res://assets/images/noteSkins/default/green hold piece.png"));
		noteTex.Add("red hold piece", ResourceLoader.Load<Texture2D>("res://assets/images/noteSkins/default/red hold piece.png"));
		noteTex.Add("blue hold piece", ResourceLoader.Load<Texture2D>("res://assets/images/noteSkins/default/blue hold piece.png"));

		noteTex.Add("purple hold end", ResourceLoader.Load<Texture2D>("res://assets/images/noteSkins/default/purple hold end.png"));
		noteTex.Add("green hold end", ResourceLoader.Load<Texture2D>("res://assets/images/noteSkins/default/green hold end.png"));
		noteTex.Add("red hold end", ResourceLoader.Load<Texture2D>("res://assets/images/noteSkins/default/red hold end.png"));
		noteTex.Add("blue hold end", ResourceLoader.Load<Texture2D>("res://assets/images/noteSkins/default/blue hold end.png"));
	}
	public void resetNote()
	{
		FlipV = false;
		Modulate = new Color(1, 1, 1, 1);

		ZIndex = 2;

		string tail = "";
		if (isSustain)
		{
			tail = " hold piece";
			if (isSustainEnd)
			    tail = " hold end";
            ZIndex = 0;

            Modulate = new Color(1, 1, 1, 0.7f);

			if (settings.downScroll)
			    FlipV = true;
		}

		switch (noteData)
		{
			case 0:
			    Texture = noteTex["purple" + tail];
			    break;
			case 1:
				Texture = noteTex["blue" + tail];
			    break;
			case 2:
			    Texture = noteTex["green" + tail];
			    break;
			case 3:
			    Texture = noteTex["red" + tail];
			    break;
		}
	}
}
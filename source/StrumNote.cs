using Godot;
using System;

public partial class StrumNote : AnimatedSprite2D
{
	[Export]
	string action;

	[Export]
	bool auto; //for opponent or botplay

	[Export]
	Character characterToSing;

	public Sprite2D hitable;
	public Sprite2D hitableSus;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (auto)
			AnimationFinished += () => Play("static");
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.

    public override void _Process(double delta) // didn't add it in _input so it doesn't crash when you don't change the settings
	{ 											// also so that i don't have to use arrays or make separate variables for each controls lol, might be useful for multiple strums in the future
		if (!auto)
		{
			if (Input.IsActionJustPressed(action))
			{
				if (hitable != null)
					goodNoteHit(false);
				else
					Play("pressed");
			}

			if (Input.IsActionPressed(action))
			{
				if (hitableSus != null)
					goodNoteHit(true);
			}

			if (Input.IsActionJustReleased(action))
				Play("static");
		}
		else
		{
			if (hitable != null)
				goodNoteHit(false);
			if (hitableSus != null)
				goodNoteHit(true);
		}
	}

	void goodNoteHit(bool isSustain)
	{
		Play("confirm");
		
		if (!auto)
		    PlayBehavior.instance.healthBar.Value += 0.06;
		PlayBehavior.instance.voices.VolumeDb = 0;
		
		calcRating();

		if (!isSustain)
		{
			characterToSing.playAnim(action);

			PlayBehavior.instance.activeNotes.Remove(hitable);
			PlayBehavior.instance.notes.Release(hitable);
		    hitable = null;
		}
		else
		{
			characterToSing.idleTimer = 0.6f;
			
			PlayBehavior.instance.activeNotes.Remove(hitableSus);
			PlayBehavior.instance.notes.Release(hitableSus);
			hitableSus = null;
		}
	}

	void calcRating()
	{
		if (!auto && hitable != null)
		{
			float ms = Math.Abs(((Note)hitable).strumTime - Conductor.songPosition);
			if (ms <= 45)
			{
				PlayBehavior.instance.popUpRating("sick");
				PlayBehavior.instance.score += 100;
				PlayBehavior.instance.accuracy.Add(100);
			}
			else if (ms > 45 && ms <= 90)
			{
				PlayBehavior.instance.popUpRating("good");
				PlayBehavior.instance.score += 80;
				PlayBehavior.instance.accuracy.Add(80);
			}
			else if (ms > 90 && ms <= 135)
			{
				PlayBehavior.instance.popUpRating("bad");
				PlayBehavior.instance.score += 60;
				PlayBehavior.instance.accuracy.Add(-10);
			}
			else if (ms > 135 && ms >= 157)
			{
				PlayBehavior.instance.popUpRating("shit");
				PlayBehavior.instance.score += 40;
				PlayBehavior.instance.accuracy.Add(-20);
			}
		}
	}

	public void missNote(Sprite2D note)
	{
		PlayBehavior.instance.voices.VolumeDb = -80;
		PlayBehavior.instance.misses++;
		PlayBehavior.instance.healthBar.Value -= 0.04;
		PlayBehavior.instance.score -= 10;
		PlayBehavior.instance.accuracy.Add(-30);

		characterToSing.playAnim(action + "-miss");

		PlayBehavior.instance.activeNotes.Remove(note);
		PlayBehavior.instance.notes.Release(note);

		if (!((Note)note).isSustain) hitable = null;
		else hitableSus = null;
	}
}

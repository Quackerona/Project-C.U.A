using Godot;
using System;

public partial class MusicBeatBehavior : Node2D
{
	public PersistentMusic persistentAudio;
	public DiscordSDK discordSDK;
	public Transition transition;

	public int curStep;
	public int curBeat;
	public int curSection;

	BPMChangeEvent lastEvent;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Conductor._Ready();
		lastEvent = new BPMChangeEvent();

		if (persistentAudio == null)
			persistentAudio = GetNode<PersistentMusic>("/root/MusicNSounds");

		if (discordSDK == null)
			discordSDK = GetNode<DiscordSDK>("/root/DiscordSDK");

		if (transition == null)
			transition = GetNode<Transition>("/root/Transition");
		transition.Play(true);
	}

	int oldStep = 0;
	int oldSection = 0;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Conductor._Process(delta);

		for (int i = 0; i < Conductor.bpmChangeMap.Count; i++)
		{
			if (Conductor.songPosition >= Conductor.bpmChangeMap[i].songTime)
				lastEvent = Conductor.bpmChangeMap[i];
		}

		curStep = lastEvent.stepTime + Mathf.FloorToInt((Conductor.songPosition - lastEvent.songTime) / Conductor.stepCrochet);
        curBeat = Mathf.FloorToInt(curStep / 4);
		curSection = Mathf.FloorToInt(curStep / 16);

        if (curStep > oldStep)
        {
            oldStep = curStep;
            stepHit();
        }

		if (curSection > oldSection)
        {
            oldSection = curSection;
            sectionHit();
        }
	}

	public virtual void stepHit()
    {
        if (curStep % 4 == 0)
            beatHit();
    }
    public virtual void beatHit()
    {
        
    }
	public virtual void sectionHit()
    {
        
    }
	public void switchState(string state)
	{
		transition.Play();
		Timer switchTime = new Timer();
		switchTime.WaitTime = 0.5;
		switchTime.OneShot = true;
		switchTime.Timeout += () => GetTree().ChangeSceneToFile("res://Scenes/" + state + ".tscn");
		AddChild(switchTime);
		switchTime.Start();
	}
}

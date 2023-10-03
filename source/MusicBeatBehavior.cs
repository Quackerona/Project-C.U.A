using Godot;
using System;

public partial class MusicBeatBehavior : Node2D
{
	public int curStep;
	public int curBeat;
	public int curSection;

	BPMChangeEvent lastEvent;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Conductor._Ready();
		lastEvent = new BPMChangeEvent();
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

	protected virtual void stepHit()
    {
        if (curStep % 4 == 0)
            beatHit();
    }
    protected virtual void beatHit()
    {
        
    }
	protected virtual void sectionHit()
    {
        
    }
}

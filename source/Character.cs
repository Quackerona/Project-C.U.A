using Godot;
using System;
using System.Collections.Generic;

public partial class Character : AnimatedSprite2D
{
	public CharacterData charData;

	public float idleTimer;

	public void setCharacter(string character)
	{
		charData = Conductor.loadCharFromJson(character);

		SpriteFrames = ResourceLoader.Load<SpriteFrames>("res://assets/images/characters/" + character + "/anims.tres");
	}

	public void playAnim(string name, bool reversed = false, float idleTimer = 0.2f)
	{
		this.idleTimer = idleTimer;

		Stop(); 

		if (!reversed)
		    Play(name);
		else
			PlayBackwards(name);

		Offset = new Vector2(charData.animationData[name].animationOffsets[0], charData.animationData[name].animationOffsets[1]);
	}

	public void dance()
	{
		if (!IsPlaying() && idleTimer <= 0)
			playAnim("idle");
	}

    public override void _Process(double delta)
	{
		if (!IsPlaying())
			idleTimer -= (float)delta;
	}
}

using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlayBehavior : MusicBeatBehavior
{
	// Important Game Stuff
	public static PlayBehavior instance;

	SubViewport gameView;
	SubViewport hudView;
	Camera2D gameCam;
	Camera2D hudCam;

	Settings settings;
	//
	
	// Song Info
	Vector2 gameCamZoom;
	Vector2 hudCamZoom;
	bool songStarted = false;

	public AudioStreamPlayer2D inst;
	public AudioStreamPlayer2D voices;

	int keys = 4; // todo: support more than 4k
	//
	
	// Chart Stuff
	public SongPack SONG;

	[Export]
	Array<AnimatedSprite2D> strumNotes;
	[Export]
	Array<AnimatedSprite2D> opponentStrumNotes;
	public NotePool notes;
	public List<Sprite2D> activeNotes = new List<Sprite2D>();
	List<List<dynamic>> strumData = new List<List<dynamic>>();
	//

	// Rating
	Tween ratingTween;
	Sprite2D ratingSpr;
	System.Collections.Generic.Dictionary<string, Texture2D> ratingSprCollection;
	public int score = 0;
	public List<float> accuracy = new List<float>() {100};
	public int misses = 0;

	ColorRect scoreBg;
	Label scoreTxt;

	public TextureProgressBar healthBar;
	//

	// Characters
	public AnimatedSprite2D opponent;
	public AnimatedSprite2D protagonist;

	public Sprite2D opponentIcon;
	public Sprite2D protagonistIcon;

	Character opponentScript;
	Character protagonistScript;
	//

	public override void _Ready()
	{
		base._Ready();

		settings = GetNode<Settings>("/root/Settings");

		setSongData();

		discordSDK.changePresence("Now Playing: " + SONG.song.song);

		if (!settings.downScroll)
		{
			for (int i = 0; i < strumNotes.Count; i++)
				strumNotes[i].Position = new Vector2(strumNotes[i].Position.X, 100);
			for (int i = 0; i < opponentStrumNotes.Count; i++)
				opponentStrumNotes[i].Position = new Vector2(opponentStrumNotes[i].Position.X, 100);

			scoreBg.Position = new Vector2(scoreBg.Position.X, 640);
			scoreTxt.Position = new Vector2(scoreBg.Position.X, 646.03f);

			healthBar.Position = new Vector2(healthBar.Position.X, 570.085f);
		}

		if (settings.middleScroll)
		{
			for (int i = 0; i < strumNotes.Count; i++)
				strumNotes[i].Position = new Vector2(strumNotes[i].Position.X - 320f, strumNotes[i].Position.Y);
				
			for (int i = 0; i < opponentStrumNotes.Count; i++)
				opponentStrumNotes[i].Hide();
		}

		if (settings.hideHud)
		{
			scoreBg.Hide();
			scoreTxt.Hide();
			healthBar.Hide();
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Input.IsActionJustPressed("uiEscape"))
			switchState("MainMenu");

		if (songStarted)
		{	
			Conductor.songPosition = inst.GetPlaybackPosition() * 1000f;
			controlNotes();

			changeScoreText();

			if (SONG.song.notes[curSection].mustHitSection)
				gameCam.Position = new Vector2(Mathf.Lerp(gameCam.Position.X, protagonist.Position.X - protagonistScript.charData.cameraOffset[0], (float)delta / (1f/60f) * 0.2f), Mathf.Lerp(gameCam.Position.Y, protagonist.Position.Y + protagonistScript.charData.cameraOffset[1], (float)delta / (1f/60f) * 0.2f));
			else
				gameCam.Position = new Vector2(Mathf.Lerp(gameCam.Position.X, opponent.Position.X + opponentScript.charData.cameraOffset[0], (float)delta / (1f/60f) * 0.2f), Mathf.Lerp(gameCam.Position.Y, opponent.Position.Y + opponentScript.charData.cameraOffset[1], (float)delta / (1f/60f) * 0.2f));

			hudCam.Zoom = new Vector2(Mathf.Lerp(hudCam.Zoom.X, hudCamZoom.X, (float)delta / (1f/60f) * 0.05f), Mathf.Lerp(hudCam.Zoom.Y, hudCamZoom.Y, (float)delta / (1f/60f) * 0.05f));
			gameCam.Zoom = new Vector2(Mathf.Lerp(gameCam.Zoom.X, gameCamZoom.X, (float)delta / (1f/60f) * 0.05f), Mathf.Lerp(gameCam.Zoom.Y, gameCamZoom.Y, (float)delta / (1f/60f) * 0.05f));
		}

		float xThing = healthBar.Position.X + (healthBar.Size.X * ((float)Mathf.Remap(healthBar.Value / 2f * 100f, 0f, 100f, 100f, 0f) * 0.01f));
		protagonistIcon.Position = new Vector2(xThing + 70f, healthBar.Position.Y);
		opponentIcon.Position = new Vector2(xThing - 70f, healthBar.Position.Y);
	}

    public override void stepHit()
    {
        base.stepHit();
    }

    public override void beatHit()
	{
		base.beatHit();

		if (curBeat % 2 == 0)
		{
			opponentScript.dance();
			protagonistScript.dance();
		}

		if (SONG.song.notes[curSection] != null && SONG.song.notes[curSection].changeBPM)
			Conductor.changeBPM(SONG.song.notes[curSection].bpm);
			
		//hudCam.Zoom = new Vector2(1.05f, 1.05f);
		//gameCam.Zoom = new Vector2(1.1f, 1.1f);
	}

    public override void sectionHit()
    {
        base.sectionHit();

		if (SONG.song.notes[curSection].mustHitSection)
			gameCamZoom = new Vector2(1.2f, 1.2f);
		else
			gameCamZoom = new Vector2(0.751f, 0.751f);

		gameCam.Zoom += new Vector2(0.03f, 0.03f);
		hudCam.Zoom += new Vector2(0.03f, 0.03f);
    }

    /////////////////////////////////////////////////////////////////////// tools

    string formatTime(int second)
	{
		string timeString = (second / 60).ToString() + ":";
		int timeStringHelper = second % 60;
		if (timeStringHelper < 10)
			timeString += "0";
		timeString += timeStringHelper;

		return timeString;
	}

	void changeScoreText()
	{
		scoreTxt.Text = $"Score: {score} | Misses: {misses} | Acc: {Math.Round(accuracy.Sum() / accuracy.Count, 2)}% | Duration: {formatTime((int)(inst.Stream.GetLength() - inst.GetPlaybackPosition()))}";
		scoreBg.Size = new Vector2(scoreTxt.Size.X + 100f, 40);
		
		discordSDK.changePresence("Now Playing: " + SONG.song.song, scoreTxt.Text);

		scoreTxt.Position = new Vector2((1280f - scoreTxt.Size.X) / 2, scoreTxt.Position.Y);
		scoreBg.Position = new Vector2((1280f - scoreBg.Size.X) / 2f, scoreBg.Position.Y);
	}

	void startCountdown()
	{
		int counter = 0;

		Sprite2D countdownSprite = new Sprite2D();
		countdownSprite.Position = new Vector2(640, 360);
		AddChild(countdownSprite);

		AudioStreamPlayer2D countdownSound = new AudioStreamPlayer2D();

		Timer countdown = new Timer();
		countdown.WaitTime = Conductor.crochet / 1000;
		countdown.Timeout += () => {
			switch (counter)
			{
				case 0:
					countdownSound.Stop();
					countdownSound.Stream = ResourceLoader.Load<AudioStream>("res://assets/sounds/intro3.ogg");
					countdownSound.Play();
					break;
				case 1:
					countdownSprite.Texture = ResourceLoader.Load<Texture2D>("res://assets/images/ready.png");

					countdownSound.Stop();
					countdownSound.Stream = ResourceLoader.Load<AudioStream>("res://assets/sounds/intro2.ogg");
					countdownSound.Play();
					break;
				case 2:
					countdownSprite.Texture = ResourceLoader.Load<Texture2D>("res://assets/images/set.png");

					countdownSound.Stop();
					countdownSound.Stream = ResourceLoader.Load<AudioStream>("res://assets/sounds/intro1.ogg");
					countdownSound.Play();
					break;
				case 3:
					countdownSprite.Texture = ResourceLoader.Load<Texture2D>("res://assets/images/go.png");
		
					countdownSound.Stop();
					countdownSound.Stream = ResourceLoader.Load<AudioStream>("res://assets/sounds/introGo.ogg");
					countdownSound.Play();
					break;
				case 4:
					RemoveChild(countdown);
					RemoveChild(countdownSound);
					RemoveChild(countdownSprite);
					
					countdown.QueueFree();
					countdownSprite.Free();
					countdownSound.Free();

					inst.Play();
					voices.Play();
					songStarted = true;

					break;
			}
			counter++;
		};
		AddChild(countdown);
		AddChild(countdownSound);
		countdown.Start();

	}

	void setSongData()
	{
		instance = this;

		gameView = GetNode<SubViewport>("Viewports/Game");
		hudView = GetNode<SubViewport>("Viewports/Hud");

		gameCam = gameView.GetNode<Camera2D>("Camera2D");
		hudCam = hudView.GetNode<Camera2D>("Camera2D");

		gameCamZoom = gameCam.Zoom;
		hudCamZoom = hudCam.Zoom;

		scoreBg = hudView.GetNode<ColorRect>("InfoDisplay/Bg");
		scoreTxt = hudView.GetNode<Label>("InfoDisplay/Text");

		ratingSpr = hudView.GetNode<Sprite2D>("Rating");
		ratingSpr.Position = new Vector2(settings.ratingPos[0], settings.ratingPos[1]);

		ratingSprCollection = new System.Collections.Generic.Dictionary<string, Texture2D>();
		ratingSprCollection.Add("sick", ResourceLoader.Load<Texture2D>("res://assets/images/sick.png"));
		ratingSprCollection.Add("good", ResourceLoader.Load<Texture2D>("res://assets/images/good.png"));
		ratingSprCollection.Add("bad", ResourceLoader.Load<Texture2D>("res://assets/images/bad.png"));
		ratingSprCollection.Add("shit", ResourceLoader.Load<Texture2D>("res://assets/images/shit.png"));

		notes = new NotePool(64);

		SONG = Conductor.loadFromJson(Name);
		Conductor.changeBPM(SONG.song.bpm);
		Conductor.mapBPMChanges(SONG);

		protagonist = gameView.GetNode<AnimatedSprite2D>("Protagonist");
		protagonistScript = (Character)protagonist;
		protagonistScript.setCharacter(SONG.song.player1);
		protagonistIcon = hudView.GetNode<Sprite2D>("ProtagonistIcon");
		protagonistIcon.Texture = ResourceLoader.Load<Texture2D>("res://assets/images/characters/" + SONG.song.player1 + "/icon.png");

		opponent = gameView.GetNode<AnimatedSprite2D>("Opponent");
		opponentScript = (Character)opponent;
		opponentScript.setCharacter(SONG.song.player2);
		opponentIcon = hudView.GetNode<Sprite2D>("OpponentIcon");
		opponentIcon.Texture = ResourceLoader.Load<Texture2D>("res://assets/images/characters/" + SONG.song.player2 + "/icon.png");

		healthBar = hudView.GetNode<TextureProgressBar>("Healthbar");
		healthBar.TintUnder = new Color(opponentScript.charData.healthBarColor);
		healthBar.TintProgress = new Color(protagonistScript.charData.healthBarColor);

		inst = GetNode<AudioStreamPlayer2D>("Inst");
		voices = GetNode<AudioStreamPlayer2D>("Voices");

        inst.Stream = ResourceLoader.Load<AudioStream>("res://assets/songs/" + Name.ToString().ToLower() + "/" + "Inst.ogg");
		voices.Stream = ResourceLoader.Load<AudioStream>("res://assets/songs/" + Name.ToString().ToLower() + "/" + "Voices.ogg");

		foreach (SectionData section in SONG.song.notes)
        {
            foreach (List<float> songNotes in section.sectionNotes)
            {
				bool shouldHit = section.mustHitSection;
				if (songNotes[1] > keys - 1)
					shouldHit = !shouldHit;
				
				List<dynamic> temp = new List<dynamic>(){songNotes[0], songNotes[1], songNotes[2], shouldHit};
				strumData.Add(temp);	
            }
        }
		
		startCountdown();
	}

	public void popUpRating(string rating)
	{
		ratingSpr.Modulate = new Color(1, 1, 1, 1);
		ratingSpr.Texture = ratingSprCollection[rating];

		if (ratingTween != null)
			ratingTween.Stop();

		ratingTween = CreateTween();
		ratingTween.TweenProperty(ratingSpr, "position", new Vector2(settings.ratingPos[0], settings.ratingPos[1] - 50f), 0.05f);
		ratingTween.TweenProperty(ratingSpr, "position", new Vector2(settings.ratingPos[0], settings.ratingPos[1]), 0.2f).SetEase(Tween.EaseType.Out);
		ratingTween.Parallel().TweenProperty(ratingSpr, "modulate:a", 0, 0.25f).SetEase(Tween.EaseType.Out);
	}

	void controlNotes()
	{
		for (int i = 0; i < strumData.Count; i++)
		{
			if (strumData[i][0] - Conductor.songPosition < 1800 / SONG.song.speed)
			{
				Sprite2D note = notes.Get(); 
				Note noteScript = (Note)note;
				noteScript.strumTime = strumData[i][0];
				noteScript.noteData = (int)(strumData[i][1] % 4);
				noteScript.isSustain = false;
				noteScript.isSustainEnd = false;
				noteScript.shouldHit = strumData[i][3];
				activeNotes.Add(note);
				if (!hudView.HasNode((string)note.Name))
					hudView.AddChild(note);
				noteScript.resetNote();

				if (strumData[i][2] > 0)
				{
					for (int k = 0; k < Mathf.FloorToInt(strumData[i][2] / Conductor.stepCrochet); k++)
					{
						Sprite2D noteSus = notes.Get();
						Note noteSusScript = (Note)noteSus;
						noteSusScript.noteData = (int)(strumData[i][1] % 4);
						noteSusScript.isSustain = true;
						noteSusScript.isSustainEnd = k == Mathf.FloorToInt(strumData[i][2] / Conductor.stepCrochet) - 1;
						if (noteSusScript.isSustainEnd)
							noteSusScript.strumTime = strumData[i][0] + (Conductor.stepCrochet * k) + Conductor.stepCrochet * 0.655f;
						else
							noteSusScript.strumTime = strumData[i][0] + (Conductor.stepCrochet * k) + Conductor.stepCrochet;
						noteSusScript.shouldHit = strumData[i][3];
						activeNotes.Add(noteSus);
						if (!hudView.HasNode((string)noteSus.Name))
							hudView.AddChild(noteSus);
						noteSusScript.resetNote();
					}
				}

				strumData.RemoveAt(i);
			}
		}

		for (int i = 0; i < activeNotes.Count; i++)
		{
			Note noteScript = (Note)activeNotes[i];
			float directionToGo = (Conductor.songPosition - noteScript.strumTime) * (0.45f * SONG.song.speed);
			if (!settings.downScroll)
				directionToGo = -directionToGo;

			if (noteScript.shouldHit)
			{
				StrumNote strumNoteScript = (StrumNote)strumNotes[noteScript.noteData];

				if (settings.middleScroll)
					activeNotes[i].Show();

				activeNotes[i].Position = new Vector2(strumNotes[noteScript.noteData].Position.X,
					strumNotes[noteScript.noteData].Position.Y + directionToGo);
				noteScript.RecScale = new Vector2(strumNotes[noteScript.noteData].Scale.X, strumNotes[noteScript.noteData].Scale.Y);

				if (noteScript.strumTime < Conductor.songPosition + 160)
				{
					if (noteScript.strumTime > Conductor.songPosition - 200)
					{
						if (!noteScript.isSustain)
						{
							if (strumNoteScript.hitable == null)
								strumNoteScript.hitable = activeNotes[i];
						}
						else
						{
							if (noteScript.strumTime < Conductor.songPosition + 10)
							{
								if (strumNoteScript.hitableSus == null)
									strumNoteScript.hitableSus = activeNotes[i];
							}
						}
					}
					else
						strumNoteScript.missNote(activeNotes[i]);
				}
			}
			else
			{
				StrumNote opponentStrumNoteScript = (StrumNote)opponentStrumNotes[noteScript.noteData];

				activeNotes[i].Position = new Vector2(opponentStrumNotes[noteScript.noteData].Position.X,
					opponentStrumNotes[noteScript.noteData].Position.Y + directionToGo);

				if (settings.middleScroll)
					activeNotes[i].Hide();

				noteScript.RecScale = new Vector2(opponentStrumNotes[noteScript.noteData].Scale.X, opponentStrumNotes[noteScript.noteData].Scale.Y);
			

				if (noteScript.strumTime < Conductor.songPosition + 160)
				{
					if (!noteScript.isSustain)
					{
						if (noteScript.strumTime < Conductor.songPosition)
						{
							if (opponentStrumNoteScript.hitable == null)
								opponentStrumNoteScript.hitable = activeNotes[i];
						}
					}
					else
					{
						if (noteScript.strumTime < Conductor.songPosition + 10)
						{
							if (opponentStrumNoteScript.hitableSus == null)
								opponentStrumNoteScript.hitableSus = activeNotes[i];
						}
					}
				}
			}
		}
	}
}

public class NotePool
{
	int maxCount = 0;
	int activeCount = 0;

	List<Sprite2D> pool;

	public NotePool(int max)
	{
		maxCount = max;
		pool = new List<Sprite2D>(max);
	}

    public Sprite2D createNote()
	{
		Sprite2D noteTemp = new Sprite2D();	
		ulong objID = noteTemp.GetInstanceId();
		noteTemp.SetScript(ResourceLoader.Load("res://source/Note.cs"));

		return (Sprite2D)GodotObject.InstanceFromId(objID);
	}

	public Sprite2D Get()
	{
		if (activeCount >= pool.Count)
		{
			Sprite2D note = createNote();
			note.ProcessMode = Node.ProcessModeEnum.Inherit;
			pool.Add(note);
			activeCount++;
			return note;
		}
		else
		{
			Sprite2D note = null;
			for (int i = 0; i < pool.Count; i++)
			{
				if (pool[i].ProcessMode == Node.ProcessModeEnum.Disabled)
				{
				    note = pool[i];
					break;
				}
			}
			note.Show();
			note.ProcessMode = Node.ProcessModeEnum.Inherit;
			activeCount++;
			return note;
		}
	}

	public void Release(Sprite2D note)
	{
		if (pool.Count > maxCount)
		{
			pool.Remove(note);
			note.Free();
		}
		else
		{
			note.Hide();
			note.ProcessMode = Node.ProcessModeEnum.Disabled;
		}

		activeCount--;
	}
}
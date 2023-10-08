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
			GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");

		if (songStarted)
		{	
			Conductor.songPosition = inst.GetPlaybackPosition() * 1000f;
			controlNotes();

			changeScoreText();

			if (SONG.song.notes[curSection].mustHitSection)
				gameCam.Position = new Vector2(Mathf.Lerp(gameCam.Position.X, protagonist.Position.X - ((Character)protagonist).charData.cameraOffset[0], (float)delta / (1f/60f) * 0.2f), Mathf.Lerp(gameCam.Position.Y, protagonist.Position.Y + ((Character)protagonist).charData.cameraOffset[1], (float)delta / (1f/60f) * 0.2f));
			else
				gameCam.Position = new Vector2(Mathf.Lerp(gameCam.Position.X, opponent.Position.X + ((Character)opponent).charData.cameraOffset[0], (float)delta / (1f/60f) * 0.2f), Mathf.Lerp(gameCam.Position.Y, opponent.Position.Y + ((Character)opponent).charData.cameraOffset[1], (float)delta / (1f/60f) * 0.2f));

			hudCam.Zoom = new Vector2(Mathf.Lerp(hudCam.Zoom.X, hudCamZoom.X, (float)delta / (1f/60f) * 0.05f), Mathf.Lerp(hudCam.Zoom.Y, hudCamZoom.Y, (float)delta / (1f/60f) * 0.05f));
			gameCam.Zoom = new Vector2(Mathf.Lerp(gameCam.Zoom.X, gameCamZoom.X, (float)delta / (1f/60f) * 0.05f), Mathf.Lerp(gameCam.Zoom.Y, gameCamZoom.Y, (float)delta / (1f/60f) * 0.05f));
		}
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
			((Character)PlayBehavior.instance.opponent).dance();
			((Character)PlayBehavior.instance.protagonist).dance();
		}

		if (SONG.song.notes[curSection] != null && SONG.song.notes[curSection].changeBPM)
			Conductor.changeBPM(SONG.song.notes[curSection].bpm);
			
		hudCam.Zoom = new Vector2(1.05f, 1.05f);
		gameCam.Zoom = new Vector2(1.1f, 1.1f);
	}

    public override void sectionHit()
    {
        base.sectionHit();
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
		((Character)protagonist).setCharacter(SONG.song.player1);

		opponent = gameView.GetNode<AnimatedSprite2D>("Opponent");
		((Character)opponent).setCharacter(SONG.song.player2);

		healthBar = hudView.GetNode<TextureProgressBar>("Healthbar");
		healthBar.TintUnder = new Color(((Character)opponent).charData.healthBarColor);
		healthBar.TintProgress = new Color(((Character)protagonist).charData.healthBarColor);

		inst = GetNode<AudioStreamPlayer2D>("Inst");
		voices = GetNode<AudioStreamPlayer2D>("Voices");

        inst.Stream = ResourceLoader.Load<AudioStream>("res://assets/songs/" + Name.ToString().ToLower() + "/" + "Inst.ogg");
		voices.Stream = ResourceLoader.Load<AudioStream>("res://assets/songs/" + Name.ToString().ToLower() + "/" + "Voices.ogg");

		inst.Play();
		voices.Play();

		foreach (SectionData section in SONG.song.notes)
        {
            foreach (List<float> songNotes in section.sectionNotes)
            {
				bool shouldHit = section.mustHitSection;
				if (songNotes[1] > 3)
					shouldHit = !shouldHit;
				
				List<dynamic> temp = new List<dynamic>(){songNotes[0], songNotes[1], songNotes[2], shouldHit};
				strumData.Add(temp);	
            }
        }
		songStarted = true;
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
		foreach (List<dynamic> strum in strumData.ToList())
		{
			if (strum[0] - Conductor.songPosition < 1800 / SONG.song.speed)
			{
				Sprite2D note = notes.Get(); 
				Note noteScript = (Note)note;
				noteScript.strumTime = strum[0];
				noteScript.noteData = (int)(strum[1] % 4);
				noteScript.isSustain = false;
				noteScript.isSustainEnd = false;
				noteScript.shouldHit = strum[3];
				activeNotes.Add(note);
				if (!hudView.HasNode((string)note.Name))
					hudView.AddChild(note);
				noteScript.resetNote();

				if (strum[2] > 0)
				{
					for (int i = 0; i < Mathf.FloorToInt(strum[2] / Conductor.stepCrochet); i++)
					{
						Sprite2D noteSus = notes.Get();
						Note noteSusScript = (Note)noteSus;
						noteSusScript.noteData = (int)(strum[1] % 4);
						noteSusScript.isSustain = true;
						noteSusScript.isSustainEnd = i == Mathf.FloorToInt(strum[2] / Conductor.stepCrochet) - 1;
						if (noteSusScript.isSustainEnd)
							noteSusScript.strumTime = strum[0] + (Conductor.stepCrochet * i) + Conductor.stepCrochet * 0.65f;
						else
							noteSusScript.strumTime = strum[0] + (Conductor.stepCrochet * i) + Conductor.stepCrochet;
						noteSusScript.shouldHit = strum[3];
						activeNotes.Add(noteSus);
						if (!hudView.HasNode((string)noteSus.Name))
							hudView.AddChild(noteSus);
						noteSusScript.resetNote();
					}
				}

				strumData.Remove(strum);
			}
		}
		
		foreach (Sprite2D note in activeNotes.ToList())
		{
			Note noteScript = (Note)note;
			float directionToGo = (Conductor.songPosition - noteScript.strumTime) * (0.45f * SONG.song.speed);
			if (!settings.downScroll)
				directionToGo = -directionToGo;

			if (noteScript.shouldHit)
			{
				StrumNote strumNoteScript = (StrumNote)strumNotes[noteScript.noteData];

				if (settings.middleScroll)
					note.Show();

				note.Position = new Vector2(strumNotes[noteScript.noteData].Position.X,
					strumNotes[noteScript.noteData].Position.Y + directionToGo);
				noteScript.RecScale = new Vector2(strumNotes[noteScript.noteData].Scale.X, strumNotes[noteScript.noteData].Scale.Y);

				if (noteScript.strumTime < Conductor.songPosition + 160)
				{
					if (noteScript.strumTime > Conductor.songPosition - 200)
					{
						if (!noteScript.isSustain)
						{
							if (strumNoteScript.hitable == null)
								strumNoteScript.hitable = note;
						}
						else
						{
							if (noteScript.strumTime < Conductor.songPosition + 10)
							{
								if (strumNoteScript.hitableSus == null)
									strumNoteScript.hitableSus = note;
							}
						}
					}
					else
						strumNoteScript.missNote(noteScript.isSustain);
				}
			}
			else
			{
				StrumNote opponentStrumNoteScript = (StrumNote)opponentStrumNotes[noteScript.noteData];

				note.Position = new Vector2(opponentStrumNotes[noteScript.noteData].Position.X,
					opponentStrumNotes[noteScript.noteData].Position.Y + directionToGo);

				if (settings.middleScroll)
					note.Hide();

				noteScript.RecScale = new Vector2(opponentStrumNotes[noteScript.noteData].Scale.X, opponentStrumNotes[noteScript.noteData].Scale.Y);
			

				if (noteScript.strumTime < Conductor.songPosition + 160)
				{
					if (!noteScript.isSustain)
					{
						if (noteScript.strumTime < Conductor.songPosition)
						{
							if (opponentStrumNoteScript.hitable == null)
								opponentStrumNoteScript.hitable = note;
						}
					}
					else
					{
						if (noteScript.strumTime < Conductor.songPosition + 10)
						{
							if (opponentStrumNoteScript.hitableSus == null)
								opponentStrumNoteScript.hitableSus = note;
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
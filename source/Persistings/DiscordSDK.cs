using Discord;
using Godot;
using System;

public partial class DiscordSDK : Node
{
	Discord.Discord discordSDK;
	
	// Rich Presence
	Activity activity;
	//

	public override void _Ready()
	{
		discordSDK = new Discord.Discord(1158614447032643665, (UInt64)Discord.CreateFlags.NoRequireDiscord);

		activity = new Activity {
			Assets = {
				LargeImage = "logo"
			}
		};

		changePresence();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		discordSDK.RunCallbacks();
	}

	public void changePresence(string Details = "", string State = "")
	{
		activity.State = State;
		activity.Details = Details;

		discordSDK.GetActivityManager().UpdateActivity(activity, presenceChangeResult);
	}

    void presenceChangeResult(Result result)
    {
       
    }
}

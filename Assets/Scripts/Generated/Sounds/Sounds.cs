using UnityEngine;
using FuryLion;

public static class Sounds
{
	public static class Music
	{

		public static AudioClip Main => SoundResources.Get((int)SoundsNames.Main);

		public static AudioClip CannonShot => SoundResources.Get((int)SoundsNames.CannonShot);

		public static AudioClip Buy => SoundResources.Get((int)SoundsNames.Buy);

		public static AudioClip Game => SoundResources.Get((int)SoundsNames.Game);
	}

}
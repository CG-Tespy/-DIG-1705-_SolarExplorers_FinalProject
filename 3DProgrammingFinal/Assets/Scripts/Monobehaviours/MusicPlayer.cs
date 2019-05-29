using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class MusicPlayer : MonoBehaviour 
{
	// Just to make sure there is only one Music Player in any one scene. Otherwise, 
	// this script wouldn't exist.
	public static MusicPlayer S 		{ get; protected set; }
	AudioSource audioSource;
	AudioFader audioFader;
	// Use this for initialization
	void Awake () 
	{
		if (S == null)
			S = this;
		
		else if (S != this)
		{
			Destroy(this.gameObject);
			return;
		}

		DontDestroyOnLoad(this.gameObject);
		audioSource = 		GetComponent<AudioSource>();
		audioFader = 		GetComponent<AudioFader>();
	}

	public void PlaySong(AudioClip clip)
	{
		// Mainly for use in the inspector, so that when you get back to the title
		// menu, the victory/game over music gives way to the title music.
		audioFader.FadeIntoClip(clip, 0.8f, 0.5f, 0.5f); 
	}
	
	
}

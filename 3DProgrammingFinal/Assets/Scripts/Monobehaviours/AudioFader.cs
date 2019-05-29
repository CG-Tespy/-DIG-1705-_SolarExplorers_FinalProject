using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class AudioFader : MonoBehaviour 
{
	public UnityEvent FadeStart 		{ get; protected set; }
	public UnityEvent FadeEnd 			{ get; protected set; }
	public AudioSource audioSource 		{ get; protected set; }
	IEnumerator fadeCoroutine = 		null;
	IEnumerator clipFadeCoroutine =		null;

	void Awake()
	{
		FadeStart = 	new UnityEvent();
		FadeEnd = 		new UnityEvent();
		audioSource = 	GetComponent<AudioSource>();
	}

	public void FadeAudio(float targetVolume, float duration)
	{
		if (fadeCoroutine != null)
			StopCoroutine(fadeCoroutine);
		

		fadeCoroutine = 		Fade(targetVolume, duration);
		StartCoroutine(fadeCoroutine);
	}

	/// <summary>
	/// Fades out of the current clip into the next clip.
	/// </summary>
	public void FadeIntoClip(AudioClip clip, float volumeToPlayAt, 
							float fadeOutDuration, float fadeInDuration)
	{
		if (clipFadeCoroutine != null)
			StopCoroutine(clipFadeCoroutine);

		clipFadeCoroutine = 		ClipFade(clip, volumeToPlayAt, 
											fadeOutDuration, fadeInDuration);
		StartCoroutine(clipFadeCoroutine);
	}

	IEnumerator Fade(float targetVolume, float duration)
	{
		FadeStart.Invoke();
		float timer = 				0;
		float baseVolume = 			audioSource.volume;

		do 
		{
			if (duration == 0)
			{
				audioSource.volume = targetVolume;
				FadeEnd.Invoke();
				yield break;
			}

			timer += 				Time.deltaTime;
			audioSource.volume = Mathf.Lerp(baseVolume, targetVolume, timer / duration);
			yield return null;

		}
		while (timer <= duration);

		// In case the lerping makes timer go past duration.
		audioSource.volume = 		targetVolume;

		// Signify that the fading is done.
		fadeCoroutine = 			null;
		FadeEnd.Invoke();
	}

	IEnumerator ClipFade(AudioClip clip, float volumeToPlayAt, 
						float fadeOutDuration, float fadeInDuration)
	{
		// Fade out of the previous clip...
		fadeCoroutine = 			Fade(0, fadeOutDuration);
		yield return StartCoroutine(fadeCoroutine);
		audioSource.Stop();

		// ... then fade into the next one.
		audioSource.clip = 			clip;
		audioSource.Play();
		fadeCoroutine = 			Fade(volumeToPlayAt, fadeInDuration);
		yield return StartCoroutine(fadeCoroutine);

		clipFadeCoroutine = 		null;
	}
	
}

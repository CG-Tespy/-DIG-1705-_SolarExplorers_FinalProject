using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour 
{
	public static int count 			{ get; protected set; }
	public UnityEvent FadeBegin 		{ get; protected set; }
	public UnityEvent FadeEnd 			{ get; protected set; }
	public CanvasGroup canvasGroup 		{ get; protected set; }
	IEnumerator fadeCoroutine = 		null;
	public bool isFading 				{ get; protected set; }
	Image image;						

	public bool blocksRaycasts
	{
		get { return canvasGroup.blocksRaycasts; }
		set { canvasGroup.blocksRaycasts = value; }
	}	
	public bool interactable 
	{
		get { return canvasGroup.interactable; }
		set { canvasGroup.interactable = value; }
	}

	public float opacity 
	{
		get { return canvasGroup.alpha; }
		set { canvasGroup.alpha = value; }
	}

	// Use this for initialization
	void Awake () 
	{
		// Make sure there is only one ScreenFader in a scene at a time
		if (count > 0)
		{
			Destroy(this.gameObject);
			return;
		}

		count++;

		image = 						GetComponent<Image>();
		canvasGroup = 					GetComponent<CanvasGroup>();
		interactable = 					false;
		blocksRaycasts = 				false;

		// Make this object persist between scenes
		Transform rootObj = 			transform;

		while (rootObj.parent != null)
			rootObj = 					rootObj.parent;

		DontDestroyOnLoad(rootObj.gameObject);

		FadeBegin = 					new UnityEvent();
		FadeEnd = 						new UnityEvent();
		isFading = 						false;
		
	}

	void Start()
	{
		GameController.S.GameOver.AddListener(ResetState);
	}
	
	public void Fade(float targetOpacity, float duration = 1f)
	{
		if (fadeCoroutine != null)
			StopCoroutine(fadeCoroutine);

		fadeCoroutine = 	FadeCoroutine(targetOpacity, duration);
		StartCoroutine(fadeCoroutine);
	}
	IEnumerator FadeCoroutine(float targetOpacity, float duration = 1f)
	{
		isFading = 			true;
		blocksRaycasts = 	true;
		Debug.Log("Fading!");
		FadeBegin.Invoke();
		float baseOpacity = opacity;
		float timer = 		0f;

		while (timer < duration)
		{
			timer += 		Time.deltaTime;
			opacity = 		Mathf.Lerp(baseOpacity, targetOpacity, timer / duration);
			Debug.Log("Current opacity: " + opacity);
			yield return null;
		}

		// Make sure to be precise
		opacity = 			targetOpacity;

		fadeCoroutine = 	null;

		// Makes it so the screen doesn't keep you from interacting with UI elements 
		// when it's done.
		blocksRaycasts = 	false;
		isFading = 			false;
		FadeEnd.Invoke();
	}

	void ResetState()
	{
		Debug.Log("ScreenFader state reset!");
		if (fadeCoroutine != null)
		{
			Debug.Log("Stopping fade cooroutine!");
			StopCoroutine(fadeCoroutine);
		}

		opacity = 		0;
		FadeBegin.RemoveAllListeners();
		FadeEnd.RemoveAllListeners();

	}

	
}

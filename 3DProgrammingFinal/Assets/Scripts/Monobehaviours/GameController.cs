using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour 
{
	public static GameController S 				{ get; protected set; }
	public UnityEvent PlayerWon 				{ get; protected set; }
	public UnityEvent GameBegin					{ get; protected set; }
	public UnityEvent GameOver 					{ get; protected set; }

	public Planet[] planets 					{ get; protected set; }
	PlayerShip player;

	[SerializeField] Text resultText;
	[SerializeField] float fadeOutDuration =  	3f;
	[SerializeField] float fadeInDuration = 	3f;
	[SerializeField] AudioClip titleMusic;
	[SerializeField] AudioClip victoryMusic;
	[SerializeField] AudioClip gameOverMusic;
	[SerializeField] AudioClip stageMusic;
	[SerializeField] string victoryScene;
	[SerializeField] string gameOverScene;
	ScreenFader screenFader;
	AudioSource musicPlayer, sfxPlayer;
	AudioFader musicFader;

	UnityAction victorySequence, deathSequence, resetGameState, fadeIntoGameScene;

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		bool titleScreen = 	scene.name == "titleScreen";

		if (titleScreen)
		{
			// For fading into the title screen
			if (screenFader.opacity > 0)
				screenFader.Fade(0, 1);

			// Make the start button fade into the game scene
			Button startButton = 	GameObject.Find("StartGameButton").GetComponent<Button>();
			startButton.onClick.AddListener(fadeIntoGameScene);
		}

		// Basically reset the game when loading into the game scene
		bool gameScene = 	scene.name == "game";

		if (gameScene)
		{
			screenFader.FadeBegin.RemoveAllListeners();
			screenFader.FadeEnd.RemoveAllListeners();

			if (screenFader.opacity > 0)
				screenFader.Fade(0, 1);

			GetGameSceneComponents();
			musicPlayer.Stop();
			musicPlayer.clip = 			stageMusic;
			musicPlayer.PlayDelayed(1f);
			planets = 					GameObject.FindObjectsOfType<Planet>();
			SubscribeToGameSceneEvents();
		}
		
		Debug.Log("On Scene Loaded!");
		bool endingScene = 	scene.name == victoryScene || scene.name == gameOverScene;

		if (endingScene)
		{
			// Do a fade in so the player can properly see the end scene, and when that's 
			// done, it's officially game over
			Debug.Log("Reached ending scene!");
			screenFader.Fade(0, fadeInDuration);
			screenFader.FadeEnd.AddListener(GameOver.Invoke);

			// Make sure that the yes and no buttons fade out the music when they're selected
			// before loading scenes
			Button yesButton = 		GameObject.Find("YesButton").GetComponent<Button>();
			Button nobutton = 		GameObject.Find("NoButton").GetComponent<Button>();

			UnityAction fadeIntoGameScene = 	new UnityAction(FadeIntoGameScene);
			UnityAction fadeIntoTitleScreen = 	new UnityAction(FadeIntoTitleScreen);

			yesButton.onClick.AddListener(fadeIntoGameScene);
			nobutton.onClick.AddListener(fadeIntoTitleScreen);

		}
	}

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

		// Various objects will need to respond to these
		PlayerWon = 		new UnityEvent();
		GameBegin = 		new UnityEvent();
		GameOver = 			new UnityEvent();

		victorySequence = 	VictorySequence;
		deathSequence = 	DeathSequence;
		resetGameState = 	ResetGameState;
		fadeIntoGameScene = FadeIntoGameScene;

		screenFader = 		GameObject.FindObjectOfType<ScreenFader>();
		musicPlayer = 		GameObject.Find("MusicPlayer").GetComponent<AudioSource>();
		musicFader = 		musicPlayer.GetComponent<AudioFader>();

		if (SceneManager.GetActiveScene().name == "game")
			GetGameSceneComponents();
		
		
		//resultText.text = 	"";
		UnityAction<Scene, LoadSceneMode> onSceneLoaded = OnSceneLoaded;
		SceneManager.sceneLoaded += 		onSceneLoaded;
		
	}

	void Start()
	{
		screenFader.Fade(0, 0);

		if (SceneManager.GetActiveScene().name == "game")
			SubscribeToGameSceneEvents();

		if (SceneManager.GetActiveScene().name == "titleScreen")
		{
			// Make the start button fade into the game scene
			Button startButton = 	GameObject.Find("StartGameButton").GetComponent<Button>();
			startButton.onClick.AddListener(fadeIntoGameScene);
		}

		GameBegin.Invoke();
	}

	void VictorySequence()
	{
		// Play music, fade into a victory screen, and then give the player the 
		// option to either exit the game or start a new one.
		Debug.Log("You win!");
		PlayerWon.RemoveListener(victorySequence);
		//resultText.text = 	"You win! You are now promoted to Admiral!";

		// Fade out of the current song into the next one
		musicFader.FadeIntoClip(victoryMusic, 1, 0.5f, 0.5f);

		screenFader.Fade(1, fadeOutDuration);

		// Make sure the victory scene is loaded when the fade out is done.
		//UnityAction undisplayResultText = 	() => resultText.text = "";

		//screenFader.FadeEnd.AddListener(undisplayResultText);
		screenFader.FadeEnd.AddListener(LoadVictoryScene);
		//screenFader.FadeEnd.AddListener(ResetGameState);

	}

	void DeathSequence()
	{
		Debug.Log("Death sequence!");
		player.statusEvents.Died.RemoveListener(deathSequence);
		// Play music, fade into a death screen, and give the player the option to 
		// either exit the game to start a new one

		//resultText.text = 	"Game Over";
		
		// Fade out of the current song into the next one
		musicFader.FadeIntoClip(gameOverMusic, 1, 0.5f, 0.5f);

		screenFader.Fade(1, fadeOutDuration);

		// Make sure the death scene is loaded when the fade out is done.
		//UnityAction undisplayResultText = 	() => resultText.text = "";
		//screenFader.FadeEnd.AddListener(undisplayResultText);
		screenFader.FadeEnd.AddListener(LoadGameOverScene);
		//screenFader.FadeEnd.AddListener(ResetGameState);
		
	}

	IEnumerator FadeIntoScene(string sceneName, float duration)
	{
		yield return null;
	}

	void LoadVictoryScene()
	{
		screenFader.FadeEnd.RemoveListener(LoadVictoryScene);
		SceneManager.LoadScene(victoryScene);

	}

	void LoadGameOverScene()
	{
		screenFader.FadeEnd.RemoveListener(LoadGameOverScene);
		SceneManager.LoadScene(gameOverScene);

	}

	void ResetGameState()
	{
		GameOver.RemoveListener(resetGameState);
		Debug.Log("Reset game state!");
		player.fuel = 		player.maxFuel;
		player.score = 		0;
		//ScoreManager.S.UpdateDisplay();
		//resultText.text = 	"";

		MissionHandler.S.Unpause();
		player.Unpause();
		FuelDisplayer.S.Unpause();

		planets = 			GameObject.FindObjectsOfType<Planet>();
	}

	void GetGameSceneComponents()
	{
		planets = 			GameObject.FindObjectsOfType<Planet>();
		player = 			GameObject.FindObjectOfType<PlayerShip>();
		sfxPlayer = 		GameObject.Find("SFXPlayer").GetComponent<AudioSource>();
	}

	void SubscribeToGameSceneEvents()
	{
		// Make sure the game over and player won events don't have any 
		// listeners before adding any
		GameOver.RemoveAllListeners();
		PlayerWon.RemoveAllListeners();

		GameOver.AddListener(resetGameState);
		PlayerWon.AddListener(victorySequence);

		player.statusEvents.Died.AddListener(deathSequence);
	}

	IEnumerator LoadSceneDelayed(string sceneName, float delay)
	{
		yield return new WaitForSeconds(delay);
		SceneManager.LoadScene(sceneName);
	}

	void FadeIntoTitleScreen()
	{
		if (!screenFader.isFading)
		{
			musicFader.FadeIntoClip(titleMusic, 0.8f, 1f, 1f);
			screenFader.Fade(1, 1);
			screenFader.FadeEnd.AddListener(() => SceneManager.LoadScene("titleScreen"));
		}
	}

	void FadeIntoGameScene()
	{
		if (!screenFader.isFading)
		{
			musicFader.FadeIntoClip(stageMusic, 1f, 1f, 1f);
			screenFader.Fade(1, 1);
			screenFader.FadeEnd.AddListener(() => SceneManager.LoadScene("game"));
		}
	}

	void FadeIntoScene(string sceneName, AudioClip musicToPlay)
	{
		if (!screenFader.isFading)
		{
			musicFader.FadeIntoClip(musicToPlay, 0.8f, 1, 1);
			screenFader.Fade(1, 1);
			screenFader.FadeEnd.AddListener(() => SceneManager.LoadScene(sceneName));
		}
	}
	
}

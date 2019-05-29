using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour, IPausable
{
	// Displays and manages the player's score
	public static ScoreManager S 				{ get; protected set; }

	// IPausable
	public UnityEvent Paused 					{ get; protected set; }
    public UnityEvent Unpaused 					{ get; protected set; }
	public bool isPaused 						{ get; protected set; }

	// How much the player is rewarded or penalized for their work on missions
	[SerializeField] int successVal = 			1;
	[SerializeField] int penaltyVal = 			1;
	[SerializeField] int deadlinePenaltyVal = 	3;
	PlayerShip player;
	[SerializeField] Text showsNumbers;
	[Tooltip("The score you need to get to beat the game.")]
	public int winScore = 						100;
	MissionHandler missionHandler;
	GameController gameController;

    // Use this for initialization
    void Awake () 
	{
		if (S == null)
			S = this;
			
		Paused = 			new UnityEvent();
		Unpaused = 			new UnityEvent();
		isPaused = 			false;
		player = 			GameObject.FindObjectOfType<PlayerShip>();
	}

	void Start()
	{
		// Watch for when a new mission is assigned, to prepare to reward or 
		// penalize the player
		missionHandler = 	MissionHandler.S;
		missionHandler.NewMissionAssigned.AddListener(OnNewMissionAssigned);

		gameController = 	GameController.S;
		gameController.PlayerWon.AddListener(Pause);

		// Make it so the score is only updated when it needs to be
		player.statusEvents.ScoreLowered.AddListener(UpdateDisplay);
		player.statusEvents.ScoreRaised.AddListener(UpdateDisplay);
		player.statusEvents.ScoreRaised.AddListener(CheckIfWin);
		UpdateDisplay();

	}
	
	public void UpdateDisplay()
	{
		Debug.Log("Updating score display!");
		string newText = 		player.score.ToString() + @"/" + winScore.ToString();
		showsNumbers.text = 	newText;
	}

	void CheckIfWin()
	{
		Debug.Log("Checking if player won");
		
		// If the player got enough points to win, let the game controller know
		if (player.score >= winScore)
			gameController.PlayerWon.Invoke();
	}
	void OnNewMissionAssigned(Mission mission)
	{
		if (!isPaused)
		{
			mission.Accomplished.AddListener(RewardPlayer);
			mission.Failed.AddListener(PenalizePlayer);
			mission.DeadlineMissed.AddListener(PenalizePlayerForDeadline);

			mission.successVal = 			this.successVal;
			mission.penaltyVal = 			this.penaltyVal;
			mission.deadlinePenaltyVal = 	this.deadlinePenaltyVal;
		}
	}

	void RewardPlayer()
	{
		if (!isPaused)
		{
			Debug.Log("Mission accomplished! Reward: " + successVal + " points.");
			player.score += 	successVal;
		}
	}

	void PenalizePlayer()
	{
		if (!isPaused)
		{
			Debug.Log("Went to the wrong planet (mission failed)... Penalty: " + penaltyVal + " points.");
			player.score -= 	penaltyVal;
		}
	}

	void PenalizePlayerForDeadline()
	{
		if (!isPaused)
		{
			Debug.Log("Missed deadline. Penalty: " + deadlinePenaltyVal + " points.");
			player.score -= 	deadlinePenaltyVal;
		}
	}

    public void Pause()
    {
        isPaused = 		true;
		Paused.Invoke();
    }

    public void Unpause()
    {
        isPaused = 		false;
		Unpaused.Invoke();
    }
}

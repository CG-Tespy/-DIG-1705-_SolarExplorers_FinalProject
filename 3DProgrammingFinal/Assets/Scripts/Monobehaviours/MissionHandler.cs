using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Events;

public class MissionHandler : MonoBehaviour, IPausable
{
	// Gives and displays the player's missions. The player can have only one 
	// mission at a time. They only get a new one whenever they fail or accomplish one.
	public static MissionHandler S 			{ get; protected set; }
	PlayerShip player;

	[SerializeField] Image missionTimeFill;
	[SerializeField] Text missionLabel;
	[SerializeField] float minTimeLimit = 		3f;
	[SerializeField] float maxTimeLimit = 		10f;

	List<Mission> missionsInEffect = 		new List<Mission>();
	public MissionEvent NewMissionAssigned 	{ get; protected set; }

    public UnityEvent Paused				{ get; protected set; }

    public UnityEvent Unpaused 				{ get; protected set; }

    public bool isPaused 					{ get; protected set; }

    [SerializeField] Transform uiPlanet = 	null;

	GameController gameController;
	
	// Use this for initialization
	void Awake () 
	{
		if (S == null)
			S = this;

		NewMissionAssigned = 				new MissionEvent();
		NewMissionAssigned.AddListener(UpdateMissionPlanetDisplay);
		player = 							GameObject.FindObjectOfType<PlayerShip>();
		gameController = 					GameObject.FindObjectOfType<GameController>();
		Invoke("AssignFirstMission", 0.5f);

		// Set up IPausable fields
		Paused = 							new UnityEvent();
		Unpaused = 							new UnityEvent();
		isPaused = 							false;

		// Set up stuff that's normally just in the inspector
		missionTimeFill = 					GameObject.Find("MissionTimeFill").GetComponent<Image>();
		missionLabel = 						GameObject.Find("MissionLabel").GetComponent<Text>();

	}

	void Start()
	{
		gameController.PlayerWon.AddListener(Pause);
		gameController.PlayerWon.AddListener(DisableMissionPlanetDisplay);

		player.statusEvents.Died.AddListener(Pause);
	}

	void Update()
	{
		if (!isPaused)
		{
			// Display the remaining time
			Mission mission = 				player.currentMission;
			missionTimeFill.fillAmount = 	mission.timeRemaining / mission.baseTimeLimit;
		}
	}

	void LateUpdate()
	{
		if (!isPaused)
		{
			// Update the missions here to avoid an enumeration-change error
			try
			{
				foreach (Mission mission in missionsInEffect)
					mission.Update();
			}
			catch (System.InvalidOperationException e) 
			{
				// When a mission is removed, it can cause an invalid operation exception
				// due to missionsInEffect changing. So, let's just ignore that.
			}
		}
	}
	
	public Mission GenerateMission()
	{
		if (!isPaused)
		{
			// Randomly pick a planet to travel to that the ship isn't already on
			List<Planet> planets = 		new List<Planet>(GameController.S.planets);
			planets.Remove(player.planetCurrentlyOn);

			int index = 				Random.Range(0, planets.Count);
			Planet planetChosen = 		planets[index];

			// Pick a time limit within the range, and with that, we have enough for
			// a new mission
			float timeLimit = 			Random.Range(minTimeLimit, maxTimeLimit + 1);
			Mission newMission = 		new Mission(timeLimit, planetChosen);
			
			return newMission;
		}
		else 
		{
			string errorMessage = 		"MissionHandler cannot generate missions while paused.";
			throw new System.InvalidOperationException(errorMessage);
		}

	}

	void AssignNewMission()
	{
		Mission newMission = 	GenerateMission();
		player.ReceiveMission(newMission);
		ActivateMission(newMission);
		NewMissionAssigned.Invoke(newMission);
		
		string message = "New mission assigned! Destination: " + newMission.destination.name;
		message += "\nTime limit: " + newMission.baseTimeLimit + " seconds.";

		Debug.Log(message);
	}

	public void ActivateMission(Mission mission)
	{
		if (!isPaused)
		{
			// Makes the time start ticking for the mission, and prepares to throw it 
			// away when it's no longer needed
			missionsInEffect.Add(mission);
			UnityAction removeMission = 		() => missionsInEffect.Remove(mission);

			mission.Accomplished.AddListener(removeMission);
			mission.Accomplished.AddListener(AssignNewMission);

			mission.DeadlineMissed.AddListener(removeMission);
			mission.DeadlineMissed.AddListener(AssignNewMission);
			mission.Failed.AddListener(removeMission);
			mission.Failed.AddListener(AssignNewMission);
		}
		else 
		{
			string errorMessage = 		"MissionHandler cannot activate missions while paused.";
			throw new System.InvalidOperationException(errorMessage);
		}
	}

	void AssignFirstMission()
	{
		Mission firstMission = 			GenerateMission();

		// Make sure that first mission isn't to Earth, since you start there
		if (firstMission.destination.name == "Earth")
			firstMission.destination = 	gameController.planets[0];
		
		player.ReceiveMission(firstMission);
		ActivateMission(firstMission);
		NewMissionAssigned.Invoke(firstMission);

		string message = "First mission assigned! Destination: " + firstMission.destination.name;
		message += "\nTime limit: " + firstMission.baseTimeLimit + " seconds.";

		Debug.Log(message);
	}
	void UpdateMissionPlanetDisplay(Mission mission)
	{
		if (!isPaused)
		{
			// Make the planet in the HUD look like the one the player is supposed to go to
			Planet missionPlanet = 		mission.destination;
			Material planetMaterial = 	missionPlanet.renderer.material;

			Renderer uiPlanetRenderer = uiPlanet.GetComponent<Renderer>();
			uiPlanetRenderer.enabled = 	true;
			uiPlanetRenderer.material = planetMaterial;

			// And update the label telling you which planet to go to
			missionLabel.text = 		"To\n" + missionPlanet.name + "!";
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

	void DisableMissionPlanetDisplay() 
	{
		Renderer uiPlanetRenderer = 	uiPlanet.GetComponent<Renderer>();
		uiPlanetRenderer.enabled = 		false;
		missionLabel.text = 			"";
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Events;

public class ShipStatusEvents
{
	public UnityEvent ScoreRaised 					{ get; protected set; }
	public UnityEvent ScoreLowered 					{ get; protected set; }
	public UnityEvent MissedDeadline				{ get; protected set; }
	public UnityEvent AccomplishedMission 			{ get; protected set; }
	public UnityEvent FailedMission 				{ get; protected set; }
	public UnityEvent Refueled 						{ get; protected set; }
	public Planet.PlanetEvent DestinationChanged 	{ get; protected set; }
	public UnityEvent ArrivedAtDestination 			{ get; protected set; }

	public UnityEvent Died 							{ get; protected set; }

	public ShipStatusEvents()
	{
		ScoreRaised = 					new UnityEvent();
		ScoreLowered = 					new UnityEvent();
		MissedDeadline = 				new UnityEvent();
		AccomplishedMission = 			new UnityEvent();
		FailedMission = 				new UnityEvent();
		Refueled = 						new UnityEvent();
		DestinationChanged = 			new Planet.PlanetEvent();
		ArrivedAtDestination = 			new UnityEvent();
		Died = 							new UnityEvent();
	}
}

public class PlayerShip : MonoBehaviour, IPausable
{
	public UnityEvent Paused 				{ get; protected set; }
	public UnityEvent Unpaused 				{ get; protected set; }
	public bool isPaused 					{ get; protected set; }
	public float distFromDest = 0f;
	public ShipStatusEvents statusEvents 	{ get; protected set; }
	[SerializeField] int _score = 			0;
	public int score
	{
		get { return _score; }
		set 
		{
			int prevScore = 	_score;
			_score = 			(int) Mathf.Max(0, value);

			if (_score > prevScore)
				statusEvents.ScoreRaised.Invoke();
			else if (_score < prevScore)
				statusEvents.ScoreLowered.Invoke();
		}
	}

	[SerializeField] List<Mission> _missions;
	public List<Mission> missions 
	{ 
		get { return _missions; } 
		protected set { _missions = 	value; }
	}

	// May need to remove this later
	public Mission currentMission = 		null;

	[SerializeField] List<Planet> _destinations;
	public List<Planet> destinations 
	{
		get { return _destinations; }
		protected set { _destinations = value; }
	}

	[SerializeField] float _flySpeed = 				50;
	public float flySpeed
	{
		get { return _flySpeed; }
		set
		{
			_flySpeed = 			value;
			navMeshAgent.speed = 	_flySpeed;
		}
	}
	[SerializeField] float _fuel = 					100;
	public float fuel 
	{
		get { return _fuel; }
		set 
		{
			// Keep fuel from going over the max
			if (value > maxFuel)
				_fuel = 	maxFuel;
			else 
				_fuel = 	value;
		}
	}
	public float maxFuel = 							100;
	[Tooltip("Fuel consumed per second")]
	
	[SerializeField] float fuelConsumptionRate = 	1f;
	bool canMove = 									true;
	bool _moving = 									false;
	bool moving 
	{
		get { return _moving; }
		set 
		{
			_moving = value;
			if (value == false)
			{
				Debug.Log(name + " stopped moving.");
				navMeshAgent.isStopped = 		true;
			}

		}
	}
	[SerializeField] Planet _destination = 			null;
	public Planet destination 
	{
		get { return _destination; }
		set 
		{
			_destination = value;

			// Set the stopping distance based on the size of the planet
			navMeshAgent.stoppingDistance = 	_destination.radius + 10;
			statusEvents.DestinationChanged.Invoke(_destination);
		}
	}

	NavMeshAgent navMeshAgent;
	[SerializeField] Planet testDestination;
	public Planet planetCurrentlyOn { get; protected set; }
	
	// Use this for initialization
	void Awake () 
	{
		Paused = 									new UnityEvent();
		Unpaused = 									new UnityEvent();
		isPaused = 									false;

		planetCurrentlyOn = 						null;
		statusEvents = 								new ShipStatusEvents();
		navMeshAgent = 								GetComponent<NavMeshAgent>();
		navMeshAgent.speed = 						flySpeed;

	}
	
	void Start()
	{
		// Testing. 
		//Debug.Log("Heading for Jupiter.");
		//Invoke("Testing", 1f);

		//transform.LookAt(destination.transform);

		// Make sure to stop moving after reaching destination until cam's caught up
		//cam.FollowPlayerEnd.AddListener(() => canMove = true);

		// Make the ship do nothing when the game's over
		statusEvents.Died.AddListener(Pause);
		GameController.S.PlayerWon.AddListener(Pause);
	}

	// Update is called once per frame
	void Update () 
	{
		ConsumeFuel();
		HandleMovement();
	}

	public void ReceiveMission(Mission mission)
	{
		currentMission = 	mission;

		missions.Add(mission);
		mission.handler = 	this;
		
		// Might need to change the above if you are meant to have more than one mission at once
	}

	void ConsumeFuel()
	{
		if (fuel > 0)
		{
			fuel -= 			fuelConsumptionRate * Time.deltaTime;

			if (fuel <= 0)
				Die();
		}
	}

	public void Die()
	{
		canMove = 			false;
		moving = 			false;
		statusEvents.Died.Invoke();
	}

	void HandleMovement()
	{
		if (canMove && !isPaused)
		{
			CheckDestinationProgress();
			CheckDestinationSelection();

			/*
			// Keep the ship moving to the right planet, since the planet itself will 
			// probably be moving
			if (moving && destination)
				navMeshAgent.SetDestination(destination.transform.position);
			*/
		}
	}

	void CheckDestinationProgress()
	{
		// Stop the spaceship if it is within the range of its destination, and mark the current
		// mission as complete if that's what the mission asked for
		if (moving && WithinRangeOf(destination))
		{
			//canMove = 		false;
			moving = 		false;
			statusEvents.ArrivedAtDestination.Invoke();
			StartCoroutine(LookAtSmooth(destination.transform, 1f));
			RefuelAt(destination);
			planetCurrentlyOn = 	destination;
			
			if (currentMission.destination == this.destination)
			{
				currentMission.Accomplished.Invoke();
				statusEvents.AccomplishedMission.Invoke();
			}
			else 
			{
				// Getting to the wrong planet counts as mission failed
				statusEvents.FailedMission.Invoke();
				currentMission.Failed.Invoke();
				Debug.Log("Went to the wrong planet!");
			}
			
		}
	}

	void CheckDestinationSelection()
	{
		// If the player presses a key tied to a planet, set that planet as the destination
		foreach (Planet planet in GameController.S.planets)
		{
			if (Input.GetKeyDown(planet.key))
			{
				destination = 	planet;
				FlyToDestination();
				return;
			}
		}
	}

	public bool WithinRangeOf(Planet planet)
	{
		Transform planetTrans = planet.transform;
		float distance = 		Vector3.Distance(transform.position, planetTrans.position);

		bool withinRange = 		distance <= (navMeshAgent.stoppingDistance + planet.radius);

		return withinRange;
	}

	public void RefuelAt(Planet planet)
	{
		Debug.Log("Arrived and refueled at " + planet.name);
		fuel += 		planet.fuel;
		planet.fuel = 	0;
	}

	void FlyToDestination()
	{
		if (!isPaused)
		{
			moving = 		true;
			navMeshAgent.isStopped = 	false;
			navMeshAgent.SetDestination(destination.transform.position);
			Debug.Log("Flying to: " + destination.name);
			
		}
	}
	
	IEnumerator LookAtDelayed(Transform toLookAt, float delay)
	{
		yield return new WaitForSeconds(delay);
		transform.LookAt(toLookAt);
	}

	IEnumerator LookAtSmooth(Transform toLookAt, float duration)
	{
		Vector3 relativePos = 		toLookAt.position - transform.position;
		// ^ VectorA - VectorB returns a vector pointing from the latter to the former. Hence 
		// this being what we'll use to get the target rotation.
		Quaternion baseRotation = 	transform.rotation;
		Quaternion targetRotation = Quaternion.LookRotation(relativePos);

		float timer = 				0;

		while (timer <= duration)
		{
			timer += 				Time.deltaTime; 

			transform.rotation = 	Quaternion.Lerp(baseRotation, targetRotation, 
													timer / duration);
			yield return null;
		}
	}

	void Testing()
	{
		destination = 		testDestination;
		FlyToDestination();
	}

	public void Pause()
	{
		isPaused = 			true;
		Paused.Invoke();
	}

	public void Unpause()
	{
		isPaused = 			false;
		Unpaused.Invoke();
	}

}

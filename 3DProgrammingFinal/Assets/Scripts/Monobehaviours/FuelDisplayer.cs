using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class FuelDisplayer : MonoBehaviour, IPausable
{
	public static FuelDisplayer S 		{ get; protected set; }
	PlayerShip player;
	[SerializeField] Image fill;

	// IPausable
    public UnityEvent Paused 			{ get; protected set; }
    public UnityEvent Unpaused 			{ get; protected set; }
    public bool isPaused 				{ get; protected set; }
	GameController gameController;

    // Use this for initialization
    void Awake () 
	{
		if (S == null)
			S = this;
		else if (S != this)
			Destroy(this.gameObject);
			
		player = 			GameObject.FindObjectOfType<PlayerShip>();

		if (fill == null)
			throw new System.NullReferenceException("Fuel displayer needs reference to the fill image.");
		
		gameController = 	GameObject.FindObjectOfType<GameController>();

		// IPausable
		Paused = 			new UnityEvent();
		Unpaused = 			new UnityEvent();
		isPaused = 			false;
	}

	void Start()
	{
		gameController.PlayerWon.AddListener(Pause);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!isPaused)
			fill.fillAmount = 		player.fuel / player.maxFuel;
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

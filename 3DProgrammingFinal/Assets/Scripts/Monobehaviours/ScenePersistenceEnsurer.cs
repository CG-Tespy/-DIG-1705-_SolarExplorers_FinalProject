using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ScenePersistenceEnsurer : MonoBehaviour 
{

	// Use this for initialization
	void Awake () 
	{
		DontDestroyOnLoad(this.gameObject);
	}

	
}

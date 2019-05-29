using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public interface IPausable
{
	UnityEvent Paused 		{ get; }
	UnityEvent Unpaused 	{ get; }

	bool isPaused 			{ get; }

	void Pause();
	void Unpause();
	
}

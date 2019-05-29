using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SolarSystemScaler : MonoBehaviour 
{
	[SerializeField] float scaleBy = 		10f;

	void Start()
	{
		foreach (Transform child in transform)
			child.localScale = child.localScale * scaleBy;
		
	}
	
}

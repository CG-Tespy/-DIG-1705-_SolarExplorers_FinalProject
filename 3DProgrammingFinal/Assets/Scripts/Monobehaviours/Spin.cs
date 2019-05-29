using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Spin : MonoBehaviour 
{
	[SerializeField] Vector3 spinForce;

	// Update is called once per frame
	void Update () 
	{
		transform.eulerAngles += 		spinForce * Time.deltaTime;
	}
	
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class FollowCam : MonoBehaviour 
{
	public float followSpeed = 	5f;
	public Vector3 offset;
	public Transform target;

	PlayerShip player;

	// Use this for initialization
	void Awake () 
	{
		player = 		GameObject.FindObjectOfType<PlayerShip>();
		followSpeed = 	player.flySpeed / 3f;
	}
	

	void LateUpdate()
	{
		FollowTarget();
	}

	void FollowTarget()
	{
		if (target)
		{
			
			Vector3 targetPos = 	target.position;
			targetPos += 			target.up * offset.y;
			//targetPos +=			-target.forward * offset.z;
			targetPos.z -= 			offset.z;
			//targetPos += 			target.right * offset.x;

			Vector3 newPos = 		Vector3.Lerp(transform.position, targetPos, 
												followSpeed * Time.deltaTime);
			
			transform.position = 	targetPos;
		}
	}
	
}

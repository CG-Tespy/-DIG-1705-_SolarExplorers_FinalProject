using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Planet : MonoBehaviour 
{
	public class PlanetEvent : UnityEvent<Planet> {}
	[SerializeField] Transform toOrbit;
	float orbitDistance;
	public float orbitSpeed = 			5;
	float orbitAngle = 					0;
	public KeyCode key;
	public float fuel = 				5f;
	public float maxFuel = 				5f;

	//public SphereCollider sphereCollider	{ get; protected set; }
	public float radius 					{ get { return diameter / 2f; } }
	public float diameter 					
	{ 
		get 
		{ 
			return (transform.localScale.x + transform.localScale.z) / 2f; 
		} 
	}

	new public Renderer renderer 			{ get; protected set; }

	public Vector3 rotationForce;
	public Vector3 size 
	{
		get { return renderer.bounds.size; }
	}

	// Use this for initialization
	void Awake () 
	{
		if (toOrbit)
			orbitAngle = 	Vector3.Angle(transform.position, toOrbit.position);


		renderer = 							GetComponent<Renderer>();
		//sphereCollider = 					GetComponent<SphereCollider>();
		
	}

	void Update()
	{
		// Rotate and regenerate fuel
		transform.eulerAngles += 			rotationForce * Time.deltaTime;

		if (fuel < maxFuel)
			fuel += 		Time.deltaTime / 2;
		if (fuel > maxFuel)
			fuel = 			maxFuel;

		if (toOrbit)
			OrbitTarget();
	}

	void OrbitTarget()
	{
		// Orbit the target on the x and z axes
		Vector3 newPos = 		toOrbit.position;
		orbitDistance = 		Vector3.Distance(transform.position, toOrbit.position);
		float xOffset = 		orbitDistance * Mathf.Sin(orbitAngle);
		float zOffset = 		orbitDistance * Mathf.Cos(orbitAngle);

		newPos.x += 			xOffset;
		newPos.z += 			zOffset;
		transform.position = 	newPos;

		orbitAngle += 			(orbitSpeed / 200) * Time.deltaTime;

		if (orbitAngle >= 360)
			orbitAngle = 		0;
	}
	
}

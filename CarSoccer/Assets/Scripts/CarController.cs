﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AxleInfo {
	public WheelCollider leftWheel;
	public WheelCollider rightWheel;
	public bool motor;
	public bool steering;
}

public class CarController : MonoBehaviour {

	public float speed = 0.0f;						// Car's current speed

	[SerializeField] List<AxleInfo> axleInfos;		// Holds all the axles for our car
	[SerializeField] float maxMotorTorque = 400f;	// How much torque can the motor apply
	[SerializeField] float maxSteeringAngle = 30f;	// How quickly the car will turn
	[SerializeField] float antiRoll = 35000f;		// Reduces amount of body roll should be equal to the spring force
	[SerializeField] float jumpForce = 350000f;		// How hard do we want to jump our car
	[SerializeField] float boostForce = 20000f;		// How much force we need to boost into air
	[SerializeField] Transform centerOfMass;		// Use to readjust the center of mass of the car
	[SerializeField] float spinForce = 5000f;
	[SerializeField] float flipForce = 1000f;

	private Rigidbody rb;							// Our car's rigidbody
	private bool grounded;							// Is the car currently jumping
	private bool flipped;
		
	public void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.centerOfMass = centerOfMass.localPosition;
	}
	
	void OnGUI() {
		GUI.Label(new Rect(10, 10, 100, 20), speed.ToString());
	}
	
	// finds the corresponding visual wheel
	// correctly applies the transform
	private void ApplyLocalPositionToVisuals(WheelCollider collider)
	{	
		// Look for the visual wheel should be a child of wheel collider gameobject
		if (collider.transform.childCount == 0) {
			return;
		}
		
		// Access the visual wheel transform
		Transform visualWheel = collider.transform.GetChild(0);
		
		Vector3 position;
		Quaternion rotation;
		
		// Get position and rotation of wheel collider
		collider.GetWorldPose(out position, out rotation);
		
		// Assign position to our wheel mesh
		visualWheel.transform.position = position;
		visualWheel.transform.rotation = rotation;
	}
	
	private void ApplyRollStabilizer(AxleInfo axleInfo)
	{
		// How much the spring has traveled
		float travelL = 1.0f;
		float travelR = 1.0f;
			
		WheelHit hit;
		
		bool groundedL, groundedR;
		
		// Check if left wheel is grounded
		groundedL = axleInfo.leftWheel.GetGroundHit(out hit);
		
		
		if (groundedL)
		{
			// Find how far the spring has traveled
			travelL = (- axleInfo.leftWheel.transform.InverseTransformPoint(hit.point).y - axleInfo.leftWheel.radius) / axleInfo.leftWheel.suspensionDistance;
		}
		else
		{
			// If it's not grounded then the spring will be fully extended
			travelL = 1.0f;
		}
		
		groundedR = axleInfo.rightWheel.GetGroundHit(out hit);
		
		
		if (groundedR)
		{
			// Find how far the spring has traveled
			travelR = (- axleInfo.rightWheel.transform.InverseTransformPoint(hit.point).y - axleInfo.rightWheel.radius) / axleInfo.rightWheel.suspensionDistance;
		}
		else
		{
			// If it's not grounded then the spring will be fully extended
			travelR = 1.0f;
		}
		
		// Calculate the anti-roll force needed
		float antiRollForce = (travelL - travelR) * antiRoll;
		
		// Apply the anti roll force
		if (groundedL)
			rb.AddForceAtPosition(transform.up * -antiRollForce, axleInfo.leftWheel.transform.position);
		if (groundedR)
			rb.AddForceAtPosition(transform.up * antiRollForce, axleInfo.rightWheel.transform.position);
		
	}
	
	private void CalculateSpeed()
	{
		speed = rb.velocity.magnitude;
	}
	
	private void Drive (float vertical, float horizontal)
	{
		foreach (AxleInfo axleInfo in axleInfos) {
		
			// Steering
			if (axleInfo.steering) {
				axleInfo.leftWheel.steerAngle = horizontal;
				axleInfo.rightWheel.steerAngle = horizontal;
			}
			
			// Acceleration
			if (axleInfo.motor) {
				axleInfo.leftWheel.motorTorque = vertical;
				axleInfo.rightWheel.motorTorque = vertical;
			}
			
			// Stabilize the body roll of the car
			ApplyRollStabilizer(axleInfo);
			
			// Make visual wheels rotate the same as their colliders
			ApplyLocalPositionToVisuals(axleInfo.leftWheel);
			ApplyLocalPositionToVisuals(axleInfo.rightWheel);
			
			if (axleInfo.leftWheel.isGrounded || axleInfo.rightWheel.isGrounded)
			{
				grounded = true;
				flipped = false;
			}
			else
			{
				grounded = false;
			}
				
		}
	}
	
	private void AerialMovement (bool jumpButton, float vertical, float horizontal)
	{
		// Handles space press
		if(jumpButton)
		{			
			if (grounded)
			{
				// Add our jump force
				rb.AddForce(transform.up * jumpForce);
			}
		}
	
		if (!grounded && !flipped)
		{
			// Flip the car over period of time
			rb.AddTorque(transform.right * vertical * Time.deltaTime * flipForce);
			// Spin the car over period of time
			rb.AddTorque(transform.up * horizontal * Time.deltaTime * spinForce);
		}

		if (flipped) 
		{
			rb.AddTorque(transform.forward * vertical * spinForce);
		}
	}
	
	private void Boost (bool boostButton)
	{
		if (boostButton)
		{
			rb.AddForce(transform.forward * boostForce);
		}
	}
	
	private void AddDownForce()
	{
		rb.AddForce(-transform.up * speed);
	}
	
	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.CompareTag ("Stadium") && Vector3.Dot (transform.up, Vector3.down) > 0) {
			flipped = true;
			Debug.Log ("Flipped");
		}
		else 
		{
			flipped = false;
		}
	}

	public void FixedUpdate()
	{
		// Get player input
		float vertical = maxMotorTorque * Input.GetAxis("Vertical");
		float horizontal = maxSteeringAngle * Input.GetAxis("Horizontal");
		bool jumpButton = Input.GetButton("Jump");
		bool boostButton = Input.GetButton("Fire1");
		
	
		Drive(vertical,horizontal);
		CalculateSpeed();
		AerialMovement(jumpButton, vertical, horizontal);
		Boost(boostButton);
		AddDownForce();
	}
}

using UnityEngine;
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

	[SerializeField] List<AxleInfo> axleInfos;		// Holds all the axles for our car
	[SerializeField] float maxMotorTorque = 400f;	// How much torque can the motor apply
	[SerializeField] float maxSteeringAngle = 30f;	// How quickly the car will turn
	[SerializeField] float antiRoll = 35000f;		// Reduces amount of body roll should be equal to the spring force
	[SerializeField] float jumpForce = 100000f;		// How hard do we want to jump our car
	
	[SerializeField] Transform centerOfMass;		// Use to readjust the center of mass of the car
	
	private Rigidbody rb;							// Our car's rigidbody
	private bool jumping;							// Is the car currently jumping
	private float speed = 0.0f;
	Vector3 lastPosition = Vector3.zero;
	
	public void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.centerOfMass = centerOfMass.position;
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
	
	void OnGUI() {
		GUI.Label(new Rect(10, 10, 100, 20), speed.ToString());
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
	
	public void FixedUpdate()
	{
		// Get player input
		float motor = maxMotorTorque * Input.GetAxis("Vertical");
		float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
		
		foreach (AxleInfo axleInfo in axleInfos) {
		
			// Steering
			if (axleInfo.steering) {
				axleInfo.leftWheel.steerAngle = steering;
				axleInfo.rightWheel.steerAngle = steering;
			}
			
			// Acceleration
			if (axleInfo.motor) {
				axleInfo.leftWheel.motorTorque = motor;
				axleInfo.rightWheel.motorTorque = motor;
			}
			
			ApplyRollStabilizer(axleInfo);
			
			// Make visual wheels rotate the same as their colliders
			ApplyLocalPositionToVisuals(axleInfo.leftWheel);
			ApplyLocalPositionToVisuals(axleInfo.rightWheel);
			
			if (axleInfo.leftWheel.isGrounded)
				jumping = false;
		}
		
		if(Input.GetKey("space") && jumping == false)
		{
			rb.AddForce(Vector3.up * jumpForce);
			jumping = true;
		}
		
		speed = (((transform.position - lastPosition).magnitude) / Time.deltaTime) ;
		lastPosition = transform.position;
	}
}

using UnityEngine;
using System.Collections;

public class BoxCarController : MonoBehaviour {

	public float speed = 0.0f;						// Car's current speed
	
	[SerializeField] float maxMotorTorque = 400f;	// How much torque can the motor apply
	[SerializeField] float maxSteeringAngle = 30f;	// How quickly the car will turn
	[SerializeField] float jumpForce = 350000f;		// How hard do we want to jump our car
	[SerializeField] float boostForce = 20000f;		// How much force we need to boost into air
	[SerializeField] Transform centerOfMass;		// Use to readjust the center of mass of the car
	
	private Rigidbody rb;							// Our car's rigidbody
	private bool grounded;							// Is the car currently jumping
	
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
	private void ApplyLocalPositionToVisuals()
	{	

	}
	
	Vector3 lastPosition = Vector3.zero;
	private void CalculateSpeed()
	{
		speed = (((transform.position - lastPosition).magnitude) / Time.deltaTime) ;
		lastPosition = transform.position;	
	}
	
	private void Drive (float vertical, float horizontal)
	{
		rb.AddForce(transform.forward * vertical);
		rb.AddTorque(transform.up * horizontal * speed);
	}
	
	private void AerialMovement (bool jumpButton, float vertical, float horizontal)
	{
		// Handles space press
		if(jumpButton)
		{			
			if (grounded)
			{
				// Add our jump force
				rb.AddForce(Vector3.up * jumpForce);
			}
		}
		
		if (!grounded)
		{
			rb.AddTorque(transform.right * vertical * Time.deltaTime * 500);
			rb.AddTorque(transform.up * horizontal * Time.deltaTime * 5000);
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
		rb.AddForce(Vector3.down * speed);
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
		//AerialMovement(jumpButton, vertical, horizontal);
		//Boost(boostButton);
		//AddDownForce();
	}
}

using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	[SerializeField] Transform player;
	[SerializeField] float cameraFollowDepth = 3.0f;
	[SerializeField] bool orbitY;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (player != null)
		{
			//transform.LookAt(player);
			
			//if (orbitY)
			//	transform.RotateAround(player.position, Vector3.up, Time.deltaTime * 15);
				
			transform.position = new Vector3 (player.position.x, player.position.y + 0.8f, player.position.z - cameraFollowDepth);
		}
	}
}

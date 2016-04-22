using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	[SerializeField] Transform player;
	[SerializeField] float cameraFollowDepth = 3.0f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3 (player.localPosition.x, player.localPosition.y + .8f, player.localPosition.z - cameraFollowDepth);
	}
}

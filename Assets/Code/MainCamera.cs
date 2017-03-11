using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour {

    public float height = 2.0f;
    public float followDistance = 5.0f;
    public float cameraRotateSpeed = 90;
    public float lookAtHeight = 1.0f;

    public GameObject hero = null;
    
	// Use this for initialization
	void Start () {
        Vector3 pos = transform.position;
        pos.y = height;
        transform.position = pos;
    }
	
    void Update()
    {
        
    }

	// Update is called once per frame
	void LateUpdate() {

        // look at hero
        GameController heroController = GameObject.Find("GameController").GetComponent<GameController>();
        transform.rotation = Quaternion.Slerp(transform.rotation, hero.transform.rotation,
            cameraRotateSpeed * Mathf.Deg2Rad * Time.deltaTime * heroController.GetSpeedProjection() / heroController.heroMaxSpeed);

        Vector3 heroToCamera = Vector3.ProjectOnPlane(-transform.forward, Vector3.up);
        heroToCamera.Normalize();
        heroToCamera = heroToCamera * followDistance;

        Vector3 cameraPosition = hero.transform.position + heroToCamera;
        cameraPosition.y = height;
        transform.position = cameraPosition;
        Transform targetTransform = hero.transform;
        Vector3 lookAtTarget = targetTransform.position;
        lookAtTarget.y = lookAtHeight;
        transform.LookAt(lookAtTarget, Vector3.up);
    }
}

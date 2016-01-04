using UnityEngine;
using System.Collections;

public class TP_Motor : MonoBehaviour {
	/* (Basically moves the character around)
	* Process motion data from TP_Controller into World Space motion
	* Character rotation (camera relative)
	* Camera's loo direction defines character orientation
	*/

	public static TP_Motor _instance;
	public float MoveSpeed = 10f;
	
	public Vector3 MoveVector { get; set; }


	void Awake() 
	{
		_instance = this;
	}

	// Motor will update b/c TP_Controller tells it to
	public void UpdateMotor () 
	{ 
		SnapAlignCharacterWithCamera();
		ProcessMotion();
	}
	
	void ProcessMotion() 
	{
		// Transform MoveVector to World Space (relative to camera orientation, Also depends on SnapAlignCharacterWithCamera() for ref of rotation)
		MoveVector = transform.TransformDirection (MoveVector);
		//MoveVector = Camera.main.transform.TransformDirection(MoveVector);
		
		// Normalize MoveVector if Magnitude > 1
		// Normalize fixes diagonal movement (Diagonal movement is faster than normal. So we use this to normalize diagonal speed)
		if (MoveVector.magnitude > 1) 
		{
			MoveVector = Vector3.Normalize(MoveVector);
		}
		
		// Multiply MoveVector by MoveSpeed
		MoveVector *= MoveSpeed;

		// Multiply MoveVector by DeltaTime (meters/frame to meters/sec)
		MoveVector *= Time.deltaTime;

		// Move the character
		TP_Controller._characterController.Move(MoveVector);
	}

	// checks if we are moving
	void SnapAlignCharacterWithCamera() 
	{ 
		// If moving
		if (MoveVector.x != 0 || MoveVector.z != 0) 
		{
			// Rotate the character to match direction camera is facing
			transform.rotation = Quaternion.Euler(
				transform.eulerAngles.x,
				Camera.main.transform.eulerAngles.y,
				transform.eulerAngles.z
				);
		}
	}

}

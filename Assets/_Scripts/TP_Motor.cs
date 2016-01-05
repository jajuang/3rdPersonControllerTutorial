using UnityEngine;
using System.Collections;

public class TP_Motor : MonoBehaviour {
	/* (Basically moves the character around)
	* Process motion data from TP_Controller into World Space motion
	* Character rotation (camera relative)
	* Camera's loo direction defines character orientation
	*/

	public static TP_Motor _instance;
	public float forwardSpeed = 10f;
	public float backwardsSpeed = 10f;
	public float strafingSpeed = 10f;
	public float slideSpeed = 10f;
	public float jumpSpeed = 12f;
	public float gravity = 21f;
	public float terminalVelocity = 20f;
	public float slideThreshold = 0.7f;
	public float maxControllableSlideMagnitude = 0.4f; // the max angle you can move up on (without jumping)

	private Vector3 slideDirection;
	public Vector3 localMoveDirection { get; set; } // The direction the charcter faces while moving (in relation to camera view and user's input)
	public Vector3 stationaryDirection { get; set; }  // The direction the character faces when stationary (in relation to camera view and user's input)

	public Vector3 cameraBehind { get; set; }

	public Vector3 MoveVector { get; set; }
	public float VerticalVelocity { get; set; }


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
		
		// For diagonal movement. Normalize MoveVector if Magnitude > 1 (Normalize fixes diagonal movement. Diagonal movement is faster than normal. So we use this to normalize diagonal speed)
		if (MoveVector.magnitude > 1) 
		{
			MoveVector = Vector3.Normalize(MoveVector);
		}

		// Apply sliding if applicable
		ApplySlide();
		
		// Multiply MoveVector by MoveSpeed
		MoveVector *= MoveSpeed();

		// Reapply VerticalVelocity to MoveVector.y
		MoveVector = new Vector3(MoveVector.x, VerticalVelocity, MoveVector.z);

		// Apply gravity
		ApplyGravity();

		// Move the character
		TP_Controller._characterController.Move(MoveVector * Time.deltaTime);


		
		float directionOut = 0f;
		float speedOut = 0f;
		float angleOut = 0f;
		StickToWorldspace (this.transform, Camera.main.transform, ref directionOut, ref speedOut, ref angleOut, false);
		// Face charcter to direction it is moving relative to the camera
		if (localMoveDirection != Vector3.zero) {
			transform.localRotation = Quaternion.LookRotation (localMoveDirection);
		}
	}


	//https://www.youtube.com/watch?v=lnguV1v38z4&index=8&list=PLKFvhfT4QOqlEReJ2lSZJk_APVq5sxZ-x
	public void StickToWorldspace(Transform root, Transform camera, ref float directionOut, ref float speedOut, ref float angleOut, bool isPivoting)
	{
		var inputX = Input.GetAxis("Horizontal");
		var inputY = Input.GetAxis("Vertical");	
		
		stationaryDirection = root.forward;

		// The direction of the user's input (in relation to world view)
		Vector3 stickDirection = new Vector3(inputX, 0, inputY);
		
		speedOut = stickDirection.sqrMagnitude;		
		
		// Get camera rotation
		Vector3 CameraDirection = camera.forward;
		CameraDirection.y = 0.0f; // kill Y
		Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(CameraDirection));
		
		// Convert joystick input in Worldspace coordinates
		// The direction of the user's input (in relation to camera view)
		localMoveDirection = referentialShift * stickDirection; 		// Offset of camera and where user input is pointing
		cameraBehind =  referentialShift * stickDirection; 		// Offset of camera and where user input is pointing

		Vector3 axisSign = Vector3.Cross(localMoveDirection, stationaryDirection);
		
		Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2f, root.position.z), localMoveDirection, Color.green);
		Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2f, root.position.z), stationaryDirection, Color.magenta);
		Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2f, root.position.z), stickDirection, Color.blue);
		Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2.5f, root.position.z), axisSign, Color.red); // positive=facing up and negative=facing down


		float angleRootToMove = Vector3.Angle(stationaryDirection, localMoveDirection) * (axisSign.y >= 0 ? -1f : 1f);
		if (!isPivoting)
		{
			angleOut = angleRootToMove;
		}
		angleRootToMove /= 180f;
		
		directionOut = angleRootToMove; // * directionSpeed;
	}	




	void ApplyGravity() 
	{
		if (MoveVector.y > -terminalVelocity) {
			MoveVector = new Vector3(MoveVector.x, (MoveVector.y - gravity * Time.deltaTime), MoveVector.z);
		}
		if (TP_Controller._characterController.isGrounded && MoveVector.y < -1) {
			MoveVector = new Vector3(MoveVector.x, -1, MoveVector.z);
		}
	}


	void ApplySlide()
	{	
		// Are we even grounded?
		if (!TP_Controller._characterController.isGrounded) {
			return;
		}

		// Zero out slide direction (to reset the calcuation)
		slideDirection = Vector3.zero;

		// Cast a ray straight down and store the normal of what we are standing on 
		RaycastHit hitInfo;

		// Our feet hit a thing
		if ( Physics.Raycast((transform.position), Vector3.down, out hitInfo) ) 
		{
			// Check Y of the normal and see if we can slide (Normal = a line perpendicular to a surface)
			if (hitInfo.normal.y < slideThreshold)
			{
				// Store slide direction
				slideDirection = new Vector3(hitInfo.normal.x, -hitInfo.normal.y, hitInfo.normal.z);
			}
		}

		// Angle too steep?
		if (slideDirection.magnitude < maxControllableSlideMagnitude)
		{
			MoveVector += slideDirection;
		}
		else
		{
			// Too steep, Can't go up angle
			MoveVector = slideDirection;
		}

	}

	public void Jump()
	{
		if (TP_Controller._characterController.isGrounded) {
			VerticalVelocity = jumpSpeed;
		}
	}


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

	// Changes the move speed based on direction
	float MoveSpeed()
	{
		float myMoveSpeed = 0f;

		switch (TP_Animator._instance.MoveDirection) 
		{
			case TP_Animator.Direction.Stationary:
				myMoveSpeed = 0;
				break;
			case TP_Animator.Direction.Forward:
				myMoveSpeed = forwardSpeed;
				break;
			case TP_Animator.Direction.Backward:
				myMoveSpeed = backwardsSpeed;
				break;
			case TP_Animator.Direction.Left:
				myMoveSpeed = strafingSpeed;
				break;
			case TP_Animator.Direction.Right:
				myMoveSpeed = strafingSpeed;
				break;
			case TP_Animator.Direction.LeftForward:
				myMoveSpeed = forwardSpeed;
				break;
			case TP_Animator.Direction.RightForward:
				myMoveSpeed = forwardSpeed;
				break;
			case TP_Animator.Direction.LeftBackward:
				myMoveSpeed = backwardsSpeed;
				break;
			case TP_Animator.Direction.RightBackward:
				myMoveSpeed = backwardsSpeed;
				break;
			default:			
				myMoveSpeed = 0;
				break;
		}

		if (slideDirection.magnitude > 0)
		{
			myMoveSpeed = slideSpeed;
		}

		return myMoveSpeed;
	}
}

using UnityEngine;
using System.Collections;

public class TP_Controller : MonoBehaviour {
	/* (Basically is the "brain" of the 3rd person contoller)
	* Processes player inputs
	* Motion inputs into 3D Vector (move vector)
	* Initial setup check (look for camera) --if camera is gone, then stop working
	*/

	public static CharacterController _characterController;
	public static TP_Controller _instance;

	public float deadZone = 0.1f; // holds dead space (AKA responsiveness to input)

	void Awake() 
	{
		_characterController = GetComponent<CharacterController>();
		_instance = this;

		// Use Exsiting Or Create New Main Camera
		TP_Camera.UseExisitingOrCreateNewMainCamera();
	}

	void Update() 
	{
		// Check if main camera exists
		if (Camera.main == null) {
			return;
		}

		GetLocomotionInput ();

		HandleActionInput ();

		// Tell TP_Motor to update
		TP_Motor._instance.UpdateMotor ();
	}


	// Gets user input for moving the character
	void GetLocomotionInput()
	{
		// Set verticalVelocity
		TP_Motor._instance.VerticalVelocity = TP_Motor._instance.MoveVector.y;

		// Zero out (reset) move vector
		TP_Motor._instance.MoveVector = Vector3.zero;

		// Verify that vertical motion is outside of dead space
		if (Input.GetAxis("Vertical") > deadZone || Input.GetAxis("Vertical") < deadZone)
		{
			// Apply vertical axis to the Z-Axis of MoveVector
			TP_Motor._instance.MoveVector += new Vector3(0, 0, Input.GetAxis("Vertical"));
		}
		
		// Horizontal input to X axis
		if (Input.GetAxis("Horizontal") > deadZone || Input.GetAxis("Horizontal") < deadZone)
		{
			TP_Motor._instance.MoveVector += new Vector3(Input.GetAxis("Horizontal"), 0, 0);

		}

		TP_Animator._instance.DetermineCurrentMoveDirection();


		// Rotate charcater based on direction user is inputting
		var characterRotation = Vector3.zero;
		switch (TP_Animator._instance.MoveDirection)
		{
			case TP_Animator.Direction.Forward:
				characterRotation += new Vector3(0,90,0);
				transform.rotation = Quaternion.Euler(characterRotation);
				break;
			case TP_Animator.Direction.Backward:
				break;
			case TP_Animator.Direction.Left:
				break;
			case TP_Animator.Direction.Right:
				break;
			default:
				break;
		}

	}
	
	// Gets user input for character actions
	void HandleActionInput()
	{
		if (Input.GetButton("Jump")) 
		{
			Jump();
		}

		if (Input.GetButton("Bumper Left"))
		{
			// TODO
			Debug.Log ("TODO: reset camera - see commented out link");
			// http://answers.unity3d.com/questions/405954/3rd-person-free-camera-based-in-3d-buzzs-tutorial.html
			TP_Camera._instance.PutCameraBehindCharacter();
		}
		if (Input.GetAxis("Trigger Left")!=0)
		{
			TP_Camera._instance.RotateCameraLeft();
		}
		if (Input.GetAxis("Trigger Right")!=0)
		{
			TP_Camera._instance.RotateCameraRight();
		}
	}

	void Jump()
	{
		// Basic action
		TP_Motor._instance.Jump();

		// TODO: Animations

		// TODO: Sound FXs

	}
}

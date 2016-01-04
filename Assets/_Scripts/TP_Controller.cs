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
	private float rotationDegreePerSecond = 120f;
	private float leftX =0f;

	void Awake() 
	{
		_characterController = GetComponent<CharacterController>();
		_instance = this;

		// Use Exsiting Or Create New Main Camera
		TP_Camera.UseExisitingOrCreateNewMainCamera();
	}

	void Update() 
	{
		leftX = Input.GetAxis ("Horizontal");
		// Check if main camera exists
		if (Camera.main == null) {
			return;
		}
		// Gets user input for moving the character
		GetLocomotionInput ();

		// Tell TP_Motor to update
		TP_Motor._instance.UpdateMotor ();
	}

	// Gets user input for moving the character
	void GetLocomotionInput()
	{
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

	}
}

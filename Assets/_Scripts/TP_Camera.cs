using UnityEngine;
using System.Collections;

public class TP_Camera : MonoBehaviour
{

	public static TP_Camera _instance;
	public Transform TargetLookAt;
	
	public float distance = 5f;
	public float distanceMin = 3f;
	public float distanceMax = 20f;
	public float SmoothDuration = 0.05f;
	public float distanceResumeSmooth = 1f; // smoothing factor for moving back to desired distance when occluded 
	public float X_RotationSensitivity = 3f;
	public float Y_RotationSensitivity = 2.5f;
	public float ZoomSensitivity = 20f;
	public float X_Smooth = 0.05f; 
	public float Y_Smooth = 0.05f; 
	public float Y_MinLimit = 5; // min angle
	public float Y_MaxLimit = 50; // max angle
	public float OcclusionDistanceStep = 0.5f;
	public int MaxOcclusionChecks = 10;

	public float deadZone = 0.01f;
	
	private float rotationX = 0f;
	private float rotationY = 0f;
	private float velX = 0f;
	private float velY = 0f;
	private float velZ = 0f;
	private float velDistance = 0f;
	private float startDistance = 0f;
	private Vector3 position = Vector3.zero;
	private Vector3 desiredPosition = Vector3.zero;
	private float desiredDistance = 0f;
	private float distanceSmooth = 0f;
	private float preOccludedDistance = 0f; // will hold distance value everytime we zoom

	private float analogTheshold = 0.6f; // so the analog stick doesn't keep goingggggggggg


	private Vector3 lookDir;


	void Awake()
	{
		_instance = this;
	}
	
	void Start()
	{
		distance = Mathf.Clamp(distance, distanceMin, distanceMax);
		startDistance = distance;
		Reset();

		lookDir = TargetLookAt.forward;
	}
	
	void LateUpdate()
	{
		if (TargetLookAt == null) {
			return;
		}

		HandlePlayerInput();

		var count = 0;
		do {
			CalculateDesiredPosition ();
			count++;
		} while (CheckIfOccluded(count));

		//CheckCameraPoints (TargetLookAt.position, desiredPosition);
		UpdatePosition();	
	}

	
	void HandlePlayerInput()
	{
		deadZone = 0.01f;

		// Auto rotation (Move rotation with character direction)
		rotationX += Input.GetAxis("Horizontal") * X_RotationSensitivity;


		// Manual rotation (right analog sticks)
		if (Input.GetAxis ("RightAnalogX") > analogTheshold || Input.GetAxis ("RightAnalogX") < -analogTheshold || Input.GetAxis("RightAnalogY") > analogTheshold || Input.GetAxis("RightAnalogY") < -analogTheshold ){
			rotationX += Input.GetAxis ("RightAnalogX") * X_RotationSensitivity;
			rotationY -= Input.GetAxis("RightAnalogY") * Y_RotationSensitivity;
		}
			
		// Make sure our rotation is in the max/min limits
		rotationY = Helper.ClampAngle(rotationY, Y_MinLimit, Y_MaxLimit);
			
		// Zoom (Mouse scrollwheel)		
		// Make sure we're outside of dead zone (dead zone = sensitivity of user pressing inupt)
		if (Input.GetAxis("Mouse ScrollWheel") < -deadZone || Input.GetAxis("Mouse ScrollWheel") > deadZone)
		{
			// Apply mousewheel_sensitivity and subtract value from current distance 
			desiredDistance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * ZoomSensitivity,
			                              distanceMin,
			                              distanceMax);
			preOccludedDistance = desiredDistance; // user zoomed, so reset desiredDistance to user's preference (so camera doesn't move backwards when un-occluded)
			distanceSmooth = SmoothDuration;
		}
	}
	
	void CalculateDesiredPosition()
	{		
		// Evaluate distance
		ResetDesiredDistance();
		distance = Mathf.SmoothDamp(distance, desiredDistance, ref velDistance, distanceSmooth);
		
		// Calculate desired position
		desiredPosition = CalculatePosition(rotationY, rotationX, distance); // Note: rotationY and rotationX are reversed on purpose. 
	}
	
	Vector3 CalculatePosition(float myRotationX, float myRotationY, float myDistance)
	{
		Vector3 myDirection = new Vector3(0, 0, -myDistance); // negative distance is so we point behind character
		Quaternion myRotation = Quaternion.Euler(myRotationX, myRotationY, 0);
		return TargetLookAt.position + myRotation * myDirection;
	}

	#region Occluded

	bool CheckIfOccluded(int count)
	{		
		bool isOccluded = false;
		float nearestDistance = CheckCameraPoints(TargetLookAt.position, desiredPosition); 

		// Are we occluded? (We are occuluded 1 or more of our 5 points is occudued)
		if (nearestDistance != -1) { // If nothing is hit, then it returns -1
			if (count < MaxOcclusionChecks){
				isOccluded = true;
				// Move camera forward
				distance -= OcclusionDistanceStep;

				// This limit makes sure that the distance is more than 0
				// We should probs take this out when making a 1st person view
				if (distance < 0.25f) {
					distance = 0.25f;
				}
			}
			else 
			{ // brute force jump			
				// if increments > limit, force camera to safe position 
				// this keeps us from performing toomany checks in between frames
				distance = nearestDistance - Camera.main.nearClipPlane; // minus buffer so camera isn't in geometry  // just move camera there, no smoothing
			}
			desiredDistance = distance;
			distance = distanceResumeSmooth;
		}

		return isOccluded;
	}

	// If nothing is hit, then it returns -1
	float CheckCameraPoints(Vector3 from, Vector3 to) 
	{
		float nearestDistance = -1f; 
		RaycastHit hitInfo;

		// Calculate points for clipPlanePoints
		Helper.ClipPlanePoints clipPlanePoints = Helper.ClipPlaneAtNear(to);

		// Debugging
		// Draw lines in the editor to make it easier to visualize
		Debug.DrawLine(from, to + transform.forward * -GetComponent<Camera>().nearClipPlane, Color.red);
		Debug.DrawLine(from, clipPlanePoints.UpperLeft);
		Debug.DrawLine(from, clipPlanePoints.LowerLeft);
		Debug.DrawLine(from, clipPlanePoints.UpperRight);
		Debug.DrawLine(from, clipPlanePoints.LowerRight);
		
		Debug.DrawLine(clipPlanePoints.UpperLeft, clipPlanePoints.UpperRight);
		Debug.DrawLine(clipPlanePoints.UpperRight, clipPlanePoints.LowerRight);
		Debug.DrawLine(clipPlanePoints.LowerRight, clipPlanePoints.LowerLeft);
		Debug.DrawLine(clipPlanePoints.LowerLeft, clipPlanePoints.UpperLeft);


		// Get the nearest Distance for when one of the linecast points hits something between the parent and camera
		if (Physics.Linecast (from, clipPlanePoints.UpperLeft, out hitInfo) && !IsCameraIgnoreOcclusion(hitInfo)) {
			nearestDistance = hitInfo.distance;
		}
		if (Physics.Linecast (from, clipPlanePoints.LowerLeft, out hitInfo) && !IsCameraIgnoreOcclusion(hitInfo)) {
			if (hitInfo.distance < nearestDistance || nearestDistance == -1){
				nearestDistance = hitInfo.distance;
			}
		}
		if (Physics.Linecast (from, clipPlanePoints.UpperRight, out hitInfo) && !IsCameraIgnoreOcclusion(hitInfo)) {
			if (hitInfo.distance < nearestDistance || nearestDistance == -1) {
				nearestDistance = hitInfo.distance;
			}
		}		
		if (Physics.Linecast (from, clipPlanePoints.LowerRight, out hitInfo) && !IsCameraIgnoreOcclusion(hitInfo)) {
			if (hitInfo.distance < nearestDistance || nearestDistance == -1){
				nearestDistance = hitInfo.distance;
			}
		}		
		if (Physics.Linecast (from, to + transform.forward * -GetComponent<Camera> ().nearClipPlane, out hitInfo) && !IsCameraIgnoreOcclusion(hitInfo)) {
			if (hitInfo.distance < nearestDistance || nearestDistance == -1) {
				nearestDistance = hitInfo.distance;
			}
		}

		return nearestDistance;
	}


	bool IsCameraIgnoreOcclusion (RaycastHit hitInfo){
		bool isIgnored = false;
		if (hitInfo.collider.tag == "Player" 
		    || hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ignore Camera Occlusion")) 
		{
			isIgnored = true;
		}
		return isIgnored;
	}


	#endregion

	// Reset desired distance fro when occluded
	void ResetDesiredDistance()
	{
		// Did the user zoom at all when occluded?
		if (desiredDistance < preOccludedDistance) 
		{
			// User didn't zoom so don't go back to the distance before the occlusion
			var pos = CalculatePosition(rotationY, rotationX, preOccludedDistance);

			float nearestDistance = CheckCameraPoints(TargetLookAt.position, pos);

			// Check for collision
			if (nearestDistance == -1 || nearestDistance > preOccludedDistance)
			{
				// Set desiredDistance back to location before occlusion
				desiredDistance = preOccludedDistance;
			}
		}
	}

	void UpdatePosition()
	{
		var positionX = Mathf.SmoothDamp(position.x, desiredPosition.x, ref velX, X_Smooth); // S-curve from position to desiredPosition. While it moves in the S-curve, it continously aaves point in S-curve in ref vel.
		var positionY = Mathf.SmoothDamp(position.y, desiredPosition.y, ref velY, Y_Smooth);
		var positionZ = Mathf.SmoothDamp(position.z, desiredPosition.z, ref velZ, X_Smooth);
		position = new Vector3(positionX, positionY, positionZ);
		
		transform.position = position;
		
		transform.LookAt(TargetLookAt);
	}
	
	public void Reset()
	{
		rotationX = 0;
		rotationY = 25;
		distance = startDistance;
		desiredDistance = distance;
		preOccludedDistance = distance;
	}
	
	public void PutCameraBehindCharacter()
	{	
		// TODO: Make this actually work asdgfhj
		var rotX = 0f;

		// Is the character moving?
		if (TP_Animator._instance.IsMoving()) 
		{
			// take the localMoveDirection 
			Debug.Log ("Moving direction");
		} 
		else
		{
			// take the rootDirection
			Debug.Log ("Stationary direction");
			rotX = Vector3.Angle(Camera.main.transform.position, TP_Motor._instance.stationaryDirection);
			Debug.Log (Vector3.Angle(Camera.main.transform.position, TP_Motor._instance.stationaryDirection));
		}

		//Camera.main.transform.localRotation = myLocRot;

		rotationX = 0;
		rotationY = 25;
		distance = startDistance;
		desiredDistance = distance;
		preOccludedDistance = distance;
	}

	public void RotateCameraLeft(){		
		rotationX -= X_RotationSensitivity;
	}

	public void RotateCameraRight() {
		rotationX += X_RotationSensitivity;
	}

	public static void UseExisitingOrCreateNewMainCamera()
	{
		GameObject tempCamera;
		GameObject targetLookAt;
		TP_Camera myCamera;

		if (Camera.main != null)
		{
			// Use existing main camera
			tempCamera = Camera.main.gameObject;
		}
		else
		{
			// No existing main camera, so make one
			tempCamera = new GameObject("MainCamera");
			tempCamera.AddComponent<Camera>();
			tempCamera.tag = "MainCamera";
		}
		
		tempCamera.AddComponent<TP_Camera>();
		myCamera = tempCamera.GetComponent<TP_Camera>();

		// Find an existing camera target to look at
		targetLookAt = GameObject.Find("targetLookAt");

		// No existing camera target to look at, so make one where the character is
		if (targetLookAt == null)
		{
			targetLookAt = new GameObject("targetLookAt");
			targetLookAt.transform.position = Vector3.zero;
		}
		
		myCamera.TargetLookAt = targetLookAt.transform;
	}
}

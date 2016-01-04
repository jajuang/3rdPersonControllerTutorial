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
	public float X_RotationSensitivity = 2f;
	public float Y_RotationSensitivity = 2f;
	public float ZoomSensitivity = 20f;
	public float X_Smooth = 0.5f; //0.5f;
	public float Y_Smooth = 0.5f; //0.5f;
	public float Y_MinLimit = -40;
	public float Y_MaxLimit = 80;
	
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

	private float analogTheshold = 0.6f; // so the analog stick doesn't keep goingggggggggg
	
	void Awake()
	{
		_instance = this;
	}
	
	void Start()
	{
		distance = Mathf.Clamp(distance, distanceMin, distanceMax);
		startDistance = distance;
		Reset();
	}
	
	void LateUpdate()
	{
		if (TargetLookAt == null)
			return;
		
		HandlePlayerInput();

		CalculateDesiredPosition();
		
		UpdatePosition();
	}
	
	void HandlePlayerInput()
	{
		var deadZone = 0.01f;

		// Auto rotation (Move rotation with character direction)
		rotationX += Input.GetAxis("Horizontal") * X_RotationSensitivity;


		// Manual rotation (right analog sticks)
		if (Input.GetAxis ("RightAnalogX") > analogTheshold || Input.GetAxis ("RightAnalogX") < (-1*analogTheshold) || Input.GetAxis("RightAnalogY") > analogTheshold || Input.GetAxis("RightAnalogY") < (-1*analogTheshold) ){
			rotationX += Input.GetAxis ("RightAnalogX") * X_RotationSensitivity;
			rotationY -= Input.GetAxis("RightAnalogY") * Y_RotationSensitivity;
		}
				
		rotationY = Helper.ClampAngle(rotationY, Y_MinLimit, Y_MaxLimit);

		// Mouse scrollwheel 
		if (Input.GetAxis("Mouse ScrollWheel") < -deadZone || Input.GetAxis("Mouse ScrollWheel") > deadZone)
		{
			desiredDistance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * ZoomSensitivity,
			                              distanceMin,
			                              distanceMax);
		}
	}
	
	void CalculateDesiredPosition()
	{		
		// Evaluate distance
		distance = Mathf.SmoothDamp(distance, desiredDistance, ref velDistance, SmoothDuration);
		
		// Calculate desired position
		desiredPosition = CalculatePosition(rotationY, rotationX, distance); // Note: rotationY and rotationX are reversed on purpose. 
	}
	
	Vector3 CalculatePosition(float rotationX, float rotationY, float distance)
	{
		Vector3 direction = new Vector3(0, 0, -distance); // -distance to point behind character
		Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
		return TargetLookAt.position + rotation * direction;
	}

	
	void UpdatePosition()
	{
		var positionX = Mathf.SmoothDamp(position.x, desiredPosition.x, ref velX, X_Smooth);
		var positionY = Mathf.SmoothDamp(position.y, desiredPosition.y, ref velY, Y_Smooth);
		var positionZ = Mathf.SmoothDamp(position.z, desiredPosition.z, ref velZ, X_Smooth);
		position = new Vector3(positionX, positionY, positionZ);
		
		transform.position = position;
		
		transform.LookAt(TargetLookAt);
	}
	
	public void Reset()
	{
		rotationX = 0;
		rotationY = 10;
		distance = startDistance;
		desiredDistance = distance;
	}
	
	public static void UseExisitingOrCreateNewMainCamera()
	{
		GameObject tempCamera;
		GameObject targetLookAt;
		TP_Camera myCamera;
		
		if (Camera.main != null)
		{
			tempCamera = Camera.main.gameObject;
		}
		else
		{
			tempCamera = new GameObject("MainCamera");
			tempCamera.AddComponent<Camera>();
			tempCamera.tag = "MainCamera";
		}
		
		tempCamera.AddComponent<TP_Camera>();
		myCamera = tempCamera.GetComponent<TP_Camera>();
		
		targetLookAt = GameObject.Find("targetLookAt");
		
		if (targetLookAt == null)
		{
			targetLookAt = new GameObject("targetLookAt");
			targetLookAt.transform.position = Vector3.zero;
		}
		
		myCamera.TargetLookAt = targetLookAt.transform;
	}
}

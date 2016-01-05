using UnityEngine;

public static class Helper 
{

	// For camera occulusion
	public struct ClipPlanePoints
	{
		public Vector3 UpperLeft;
		public Vector3 UpperRight;
		public Vector3 LowerLeft;
		public Vector3 LowerRight;
	}


	// Converts rotation to 0-360 value (either + or -), then clamps and return
	public static float ClampAngle(float angle, float min, float max) 
	{
		do { // note: do..while's run at least once
			if (angle < -360) {
				angle += 360;
			}
			if (angle > 360) {
				angle -= 360;
			}
		} while (angle < -360 || angle > 360);

		return Mathf.Clamp(angle, min, max);
	}


	public static ClipPlanePoints ClipPlaneAtNear (Vector3 pos)
	{
		var myClipPlanePoints = new ClipPlanePoints();

		// Do we even have a main camera to do calcuations?
		if (Camera.main == null) {
			return myClipPlanePoints;
		}

		// See 3D Buzz video "Enhanced Character Sytstem" pt 12 (NearClipPlane theory) for explainations
		var cameraTransform = Camera.main.transform;
		float halfFOV = (Camera.main.fieldOfView / 2) * Mathf.Deg2Rad; // FOV divided by 2 to make a right triangle. Tan() gives you radians. We want degrees, so convert.
		var aspect = Camera.main.aspect; // aspect ratio
		float distance = Camera.main.nearClipPlane;
		float height = distance * Mathf.Tan (halfFOV); // Geometry 101!!! (formula of right angles to find height)
		float width = height * aspect; // gets width in respect to aspect ratio

		// Moves LowerRight point from position to the right by the width
		myClipPlanePoints.LowerRight = pos + cameraTransform.right * width; 
		// Move LowerRight point to bottom
		myClipPlanePoints.LowerRight -= cameraTransform.up * height;
		// Move LowerRight point away from the camera
		myClipPlanePoints.LowerRight += cameraTransform.forward * distance;

		
		// Moves LowerLeft point from position to the left by the width
		myClipPlanePoints.LowerLeft = pos - cameraTransform.right * width; 
		// Move LowerLeft point to bottom
		myClipPlanePoints.LowerLeft -= cameraTransform.up * height;
		// Move LowerLeft point away from the camera
		myClipPlanePoints.LowerLeft += cameraTransform.forward * distance;

		
		// Moves UpperRight point from position to the right by the width
		myClipPlanePoints.UpperRight = pos + cameraTransform.right * width; 
		// Move UpperRight point to top
		myClipPlanePoints.UpperRight += cameraTransform.up * height;
		// Move UpperRight point away from the camera
		myClipPlanePoints.UpperRight += cameraTransform.forward * distance;

		
		// Moves UpperLeft point from position to the left by the width
		myClipPlanePoints.UpperLeft = pos - cameraTransform.right * width; 
		// Move UpperLeft point to top
		myClipPlanePoints.UpperLeft += cameraTransform.up * height;
		// Move UpperLeft point away from the camera
		myClipPlanePoints.UpperLeft += cameraTransform.forward * distance;

		return myClipPlanePoints;
	}

}

using UnityEngine;

public static class Helper {

	// Mouse wheel
	// Make sure we're outside of dead zone
	// Apply mousewheel_sensitivity
	// Subtract value from current distance 
	
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

}

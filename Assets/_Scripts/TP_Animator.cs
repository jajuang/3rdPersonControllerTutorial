using UnityEngine;
using System.Collections;

public class TP_Animator : MonoBehaviour {

	public enum Direction
	{
		Stationary ,Forward ,Backward ,Left ,Right
		,LeftForward ,RightForward ,LeftBackward ,RightBackward
	}

	public static TP_Animator _instance;

	public Direction MoveDirection { get; set; }

	void Start () 
	{
		_instance = this;
	}

	void Update () 
	{
	
	}

	public void DetermineCurrentMoveDirection() 
	{
		bool forward = false;
		bool backwards = false;
		bool left = false;
		bool right = false;

		if (TP_Motor._instance.MoveVector.z > 0) {
			forward = true;
		}
		if (TP_Motor._instance.MoveVector.z < 0) {
			backwards = true;
		}
		if (TP_Motor._instance.MoveVector.x > 0) {
			right = true;
		}
		if (TP_Motor._instance.MoveVector.x < 0) {
			left = true;
		}

		// Set the direction
		if (forward) {
			if (left){
				MoveDirection = Direction.LeftForward;
			} else if(right) {
				MoveDirection = Direction.RightForward;
			} else {
				MoveDirection = Direction.Forward;
			}
		} 
		else if(backwards)
		{			
			if (left){
				MoveDirection = Direction.LeftBackward;
			} else if(right) {
				MoveDirection = Direction.RightBackward;
			} else {
				MoveDirection = Direction.Backward;
			}
		}
		else if (left) 
		{
			MoveDirection = Direction.Left;
		} else if (right) {
			MoveDirection = Direction.Right;
		} 
		else {	//not moving			
			MoveDirection = Direction.Stationary;
		}
	}
}

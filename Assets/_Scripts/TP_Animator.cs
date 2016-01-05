using UnityEngine;
using System.Collections;

public class TP_Animator : MonoBehaviour {

	public Animation anim;
	public enum Direction
	{
		Stationary ,Forward ,Backward ,Left ,Right
		,LeftForward ,RightForward ,LeftBackward ,RightBackward
	}
	
	public enum CharacterState 
	{
		Idle ,Running ,WalkBackwards ,StafingLeft ,StafingRight ,Jumping
		,Falling ,Landing ,Climbing ,Sliding ,Using ,Dead ,Action
	}

	public static TP_Animator _instance;

	public Direction MoveDirection { get; set; }
	public CharacterState State { get; set; }


	void Start () 
	{
		_instance = this;
		anim = GetComponent<Animation>();

		if (anim == null) {
			Debug.Log ("Error: you need to set an Animation on the character!");
		}
	}

	void Update () 
	{
		DetermineCurrentState();
		ProcessCurrentState();

		//Debug.Log ("Current Character State:" + State.ToString ());
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


	public bool IsMoving()
	{		
		if (MoveDirection == Direction.Stationary) {
			return false;
		} else {
			return true;
		}
	}


	void DetermineCurrentState() 
	{
		// Dead
		if (State == CharacterState.Dead) {
			return;
		}

		// Falling
		if (!TP_Controller._characterController.isGrounded)
		{
			if (State != CharacterState.Falling 
			    && State != CharacterState.Jumping
			    && State != CharacterState.Landing )
			{

			}
		}

		// All Motion States
		if (State != CharacterState.Falling     
		    && State != CharacterState.Jumping
		    && State != CharacterState.Landing
		    && State != CharacterState.Using
		    && State != CharacterState.Climbing 
		    && State != CharacterState.Sliding )
		{
			switch (MoveDirection)
			{
				case Direction.Stationary:
					State = CharacterState.Idle;
					break;
				case Direction.Forward:
					State = CharacterState.Running;
					break;
				case Direction.Backward:
					State = CharacterState.WalkBackwards;
					break;
				case Direction.Left:
					State = CharacterState.StafingLeft;
					break;
				case Direction.Right:
					State = CharacterState.StafingRight;
					break;
				case Direction.LeftForward:
					State = CharacterState.Running;
					break;
				case Direction.LeftBackward:
					State = CharacterState.WalkBackwards;
					break;
				case Direction.RightForward:
					State = CharacterState.Running;
					break;
				case Direction.RightBackward:
					State = CharacterState.WalkBackwards;
					break;
				default:
					State = CharacterState.Idle;
					break;
			}
		}
	}

	void ProcessCurrentState()
	{
		switch (State)
		{
			case CharacterState.Idle:
				Idle();
				break;
			case CharacterState.Running :
				Running();
				break;
			case CharacterState.WalkBackwards :
				break;
			case CharacterState.StafingLeft :
				break;
			case CharacterState.StafingRight :
				break;
			case CharacterState.Jumping :
				break;
			case CharacterState.Falling :
				break;
			case CharacterState.Landing :
				break;
			case CharacterState.Climbing :
				break;
			case CharacterState.Sliding :
				break;
			case CharacterState.Using :
				break;
			case CharacterState.Dead :
				break;
			case CharacterState.Action :
				break;
			default:
				break;
		}
	}

	#region Character State Method

	void Idle()
	{
		//anim.CrossFade ("Idle"); //This needs to be named what the animations are!
	}
	void Running()
	{
		//anim.CrossFade ("HumanoidRun");
	}

	#endregion

}

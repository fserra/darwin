using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour
{
	#region structs

	public struct BoxLimit
	{
		public float leftLimit;
		public float rightLimit;
		public float topLimit;
		public float bottomLimit;
	}

	#endregion


	#region class variables

	public static BoxLimit cameraLimits      = new BoxLimit();
	public static BoxLimit mouseScrollLimits = new BoxLimit();

	public float cameraMoveSpeed = 40f;
	public float mouseBoundary   = 50f;

	#endregion


	// Use this for initialization
	void Start ()
	{
		// camera limits
		cameraLimits.leftLimit   = 20f;
		cameraLimits.rightLimit  = 110f;
		cameraLimits.topLimit    = 78f;
		cameraLimits.bottomLimit = -2f;

		mouseScrollLimits.leftLimit   = mouseBoundary;
		mouseScrollLimits.rightLimit  = mouseBoundary;
		mouseScrollLimits.topLimit    = mouseBoundary;
		mouseScrollLimits.bottomLimit = mouseBoundary;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if( ShouldCameraMove() )
		{
			Vector3 cameraDesiredMove = GetDesiredTranslation();

			if( !IsDesiredPositionOverBoundaries(cameraDesiredMove) )
			{
				transform.Translate(cameraDesiredMove, Space.World);
			}
		}
	}

	void OnGUI()
	{
		GUI.Box(new Rect(Screen.width/2 - 140, 40, 280, 25), "Screen H,W = " + Screen.height + ", " + Screen.width);
		GUI.Box(new Rect(Screen.width/2 - 140, 5, 280, 25), "Mouse position = " + Input.mousePosition);
		GUI.Box(new Rect(Screen.width/2 - 70, Screen.height - 30, 140, 25), "Mouse X = " + Input.mousePosition.x);
		GUI.Box(new Rect(5, Screen.height/2 - 12, 140, 25), "Mouse Y = " + Input.mousePosition.y);
	}


	public bool IsDesiredPositionOverBoundaries(Vector3 desiredPosition) 
	{
		bool overBoundary = false;

		if(
		   transform.position.x + desiredPosition.x < cameraLimits.leftLimit || 
		   transform.position.x + desiredPosition.x > cameraLimits.rightLimit || 
		   transform.position.z + desiredPosition.z > cameraLimits.topLimit || 
		   transform.position.z + desiredPosition.z < cameraLimits.bottomLimit
			)
			overBoundary = true;

		return overBoundary;
	}


	public Vector3 GetDesiredTranslation()
	{
		float moveSpeed   = cameraMoveSpeed * Time.deltaTime;
		float desiredX = 0f;
		float desiredZ = 0f;

		// move right
		if( Input.mousePosition.x > Screen.width - mouseScrollLimits.rightLimit )
		{
			desiredX += moveSpeed;
		}
		
		// move left
		if( Input.mousePosition.x < mouseScrollLimits.leftLimit )
		{
			desiredX -= moveSpeed;
		}
		
		// move up
		if( Input.mousePosition.y > Screen.height - mouseScrollLimits.topLimit )
		{
			desiredZ += moveSpeed;
		}
		
		// move down
		if( Input.mousePosition.y < mouseScrollLimits.bottomLimit )
		{
			desiredZ -= moveSpeed;
		}

		return new Vector3(desiredX, 0f, desiredZ);
	}


	public bool ShouldCameraMove()
	{
		bool mouseMove = false;
		bool canMove;

		// check mouse position
		if( IsMousePositionInsideScreen() && IsMousePositionWithinBoundaries() )
			mouseMove = true;
		else
			mouseMove = false;

		// TODO: add keyboard?

		// check if should move
		if( mouseMove )
			canMove = true;
		else
			canMove = false;

		return canMove;
	}


	#region helpers

	public static bool IsMousePositionWithinBoundaries()
	{
		if(
			Input.mousePosition.x < mouseScrollLimits.leftLimit ||
			Input.mousePosition.x > (Screen.width - mouseScrollLimits.rightLimit) ||
			Input.mousePosition.y < mouseScrollLimits.bottomLimit ||
			Input.mousePosition.y > (Screen.height - mouseScrollLimits.topLimit)
		 )
			return true;
		else
			return false;
	}

	public static bool IsMousePositionInsideScreen()
	{
		if(
			Input.mousePosition.x > -5 && Input.mousePosition.x < (Screen.width  + 5) &&
			Input.mousePosition.y > -5 && Input.mousePosition.y < (Screen.height + 5)
		 )
			return true;
		else
			return false;
	}
	#endregion
}

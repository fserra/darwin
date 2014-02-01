using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//FIXME: when selecting by dragging it seems that 
//the units are selected twice each
public class Mouse : MonoBehaviour
{

	// what the mouse is pointing at
	RaycastHit hit;

	public static ArrayList unitsOnScreen = new ArrayList(); // shouldn't use arraylist
	public static List<GameObject> currentlySelectedUnits = new List<GameObject>();


	private static Vector2 mouseDragStart;
	private static Vector3 mouseDownPoint;
	// position where mouse is pointing
	private static Vector3 currentMousePoint;

	public static bool userIsDragging;
	private static float timeLimitBeforeDeclareDrag = 1f;
	private static float timeLeftBeforeDeclareDrag;

	private static float clickDragZone = 1.3f;

	// GUI
	private float boxWidth;
	private float boxHeight;
	private float boxLeft;
	private float boxTop;

	// box is define from top left to bottom right corner
	// boxStart = top left, boxFinish = bottom right
	private Vector2 boxStart;
	private Vector2 boxFinish;

	void Awake()
	{
		mouseDownPoint = Vector3.zero;
	}

	// Use this for initialization
	void Start ()
	{

	}

	// Update is called once per frame
	void Update ()
	{

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Debug.DrawRay(ray.origin, ray.direction * 1000);


		if( Physics.Raycast(ray, out hit, Mathf.Infinity) )
		{
			currentMousePoint = hit.point;

			// mouse drag
			if( Input.GetMouseButtonDown(0) )
			{
				Debug.Log("Hit = " + hit.collider.name);
				mouseDownPoint = hit.point;

				timeLeftBeforeDeclareDrag = timeLimitBeforeDeclareDrag;
				mouseDragStart = Input.mousePosition;
			}
			else if( Input.GetMouseButton(0) )
			{
				if( !userIsDragging )
				{
					timeLeftBeforeDeclareDrag -= Time.deltaTime;

					if( timeLeftBeforeDeclareDrag <= 0f || UserDraggingByPosition(mouseDragStart, Input.mousePosition) )
					{
						userIsDragging = true;
					}
				}
			}
			else if( Input.GetMouseButtonUp(0) )
			{
				// select units in drag box
				if( userIsDragging )
				{
					SelectGameobjectsInDragBox();
				}

				// mouse is no longer dragging
				userIsDragging = false;
				timeLeftBeforeDeclareDrag = 0f;
			}

			//clicking anywhere
			if( Input.GetMouseButtonUp(0) && DidUserClickLeftMouse(hit.point) )
			{
				// if we hit something we can select, select it
				if( hit.collider.transform.FindChild("Selected") )
				{
					Debug.Log("now hitting " + hit.collider.name);
					SelectSingleGameobject(hit.collider.gameObject);
				}
			}

			// issue action to selected unit
			if( Input.GetMouseButtonDown(1) )
			{
				if( currentlySelectedUnits.Count > 0 )
				{
					object[] args = new object[3];

					Debug.Log("going to send message about " + hit.collider.name);
					Debug.Log("There are " + currentlySelectedUnits.Count + " units selected");
					args[0] = "rc";
					args[1] = hit.collider.gameObject;
					args[2] = currentMousePoint;

					foreach( GameObject go in currentlySelectedUnits )
					{
						Debug.Log("going to send message to " + go.name);
						go.SendMessage("Action", args);
					}
				}
			}
		}

		if( userIsDragging )
		{
			// GUI variables
			boxWidth  = Camera.main.WorldToScreenPoint(mouseDownPoint).x - Camera.main.WorldToScreenPoint(currentMousePoint).x;
			boxHeight = Camera.main.WorldToScreenPoint(mouseDownPoint).y - Camera.main.WorldToScreenPoint(currentMousePoint).y;
			boxLeft   = Input.mousePosition.x;
			boxTop    = (Screen.height - Input.mousePosition.y) - boxHeight;

			if( boxWidth > 0f && boxHeight < 0f )
			{
				// mouse end position is top left
				boxStart  = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			}
			else if( boxWidth > 0f && boxHeight > 0f )
			{
				// mouse end position is bottom left
				boxStart  = new Vector2(Input.mousePosition.x, Input.mousePosition.y + boxHeight);
			}
			else if( boxWidth < 0f && boxHeight < 0f )
			{
				// mouse end position is top right
				boxStart  = new Vector2(Input.mousePosition.x + boxWidth, Input.mousePosition.y);
			}
			else
			{
				// mouse end position is bottom right
				boxStart  = new Vector2(Input.mousePosition.x + boxWidth, Input.mousePosition.y + boxHeight);
			}

			boxFinish = new Vector2(boxStart.x + Mathf.Abs(boxWidth), boxStart.y - Mathf.Abs(boxHeight));
		}
	}

	// OnGUI can be called multiple times on a frame
	void OnGUI()
	{
		if( userIsDragging )
		{

			GUI.Box(new Rect(boxLeft, boxTop, boxWidth, boxHeight), "");
		}
	}

#region Helper functions

	// user starts dragging because of mouse position?
	public bool UserDraggingByPosition(Vector2 dragStartPoint, Vector2 newPoint)
	{
		if(
				newPoint.x > dragStartPoint.x + clickDragZone ||
				newPoint.x < dragStartPoint.x - clickDragZone ||
				newPoint.y > dragStartPoint.y + clickDragZone ||
				newPoint.y < dragStartPoint.y - clickDragZone
		  )
			return true; 
		else 
			return false;
	}

	// did user perform a mouse click?
	public bool DidUserClickLeftMouse(Vector3 hitPoint)
	{
		if(
				(mouseDownPoint.x < hitPoint.x + clickDragZone && mouseDownPoint.x > hitPoint.x - clickDragZone) &&
				(mouseDownPoint.y < hitPoint.y + clickDragZone && mouseDownPoint.y > hitPoint.y - clickDragZone) &&
				(mouseDownPoint.z < hitPoint.z + clickDragZone && mouseDownPoint.z > hitPoint.z - clickDragZone)
		  )
			return true;
		else
			return false;
	}

	// select gameobject
	// if no GO is selected, select go.
	// if one is selected, check if it is go
	// if not, deselect and select go
	// else do nothing.
	// if more GO are selected, deselect all
	// the ones that are not go, clear the 
	// selected list and add go.
	public static void SelectSingleGameobject(GameObject go)
	{
		Debug.Log("Selecting " + go);
		if( currentlySelectedUnits.Count == 0 )
		{
			currentlySelectedUnits.Add(go);
			go.SendMessage("Select");
		}
		else if( currentlySelectedUnits.Count == 1 )
		{
			if( currentlySelectedUnits[0] != go )
			{
				DeselectGameobjectIfSelected();
				currentlySelectedUnits.Add(go);
				go.SendMessage("Select");
			}
		}
		else
		{
			foreach( GameObject goiter in currentlySelectedUnits )
			{
				if( goiter != go )
					go.SendMessage("Deselect");
			}
			currentlySelectedUnits.Clear();
			currentlySelectedUnits.Add(go);
			go.SendMessage("Select");
		}
	}

	// select gameobject in dragbox
	// when first unit found, we deselect previous selection
	// if any (here we are lying, hopefuly it doesn't show
	// in the game. We shouldn't deselct units that we are going
	// to select again
	void SelectGameobjectsInDragBox()
	{
		bool foundUnit;
		bool foundBuilding;

		Debug.Log("Top left " + this.boxStart + " Bottom right " + this.boxFinish);
		Debug.Log("Units: " + GameMaster.unitObjects + " n = " + GameMaster.unitObjects.Count);

		foundUnit = false;
		foundBuilding = false;
		foreach( GameObject goiter in GameMaster.unitObjects )
		{
			Debug.Log("Object " + goiter + " position " + Camera.main.WorldToScreenPoint(goiter.transform.position));
			if( IsObjectInsideDragBox(Camera.main.WorldToScreenPoint(goiter.transform.position)) )
			{
				Debug.Log("Object inside box");

				// first unit found
				if( !foundUnit )
				{
					DeselectGameobjectIfSelected();
					foundUnit = true;
				}

				currentlySelectedUnits.Add(goiter);
				//goiter.transform.FindChild("Selected").gameObject.SetActive(true);
				goiter.SendMessage("Select");
			}
		}

		if( !foundUnit )
		{
			foreach( GameObject goiter in GameMaster.buildingObjects )
			{
				Debug.Log("Object " + goiter + " position " + Camera.main.WorldToScreenPoint(goiter.transform.position));
				if( IsObjectInsideDragBox(Camera.main.WorldToScreenPoint(goiter.transform.position)) )
				{
					Debug.Log("Object inside box");

					// first building found
					if( !foundBuilding )
					{
						DeselectGameobjectIfSelected();
						foundBuilding = true;
					}

					currentlySelectedUnits.Add(goiter);
					goiter.SendMessage("Select");
				}
			}
		}
	}

	bool IsObjectInsideDragBox(Vector2 pos)
	{
		if( 
				boxStart.x <= pos.x && pos.x <= boxFinish.x &&
				boxStart.y >= pos.y && pos.y >= boxFinish.y
		  )	
			return true;
		else
			return false;
	}

	// deselect gameobject if selected
	public static void DeselectGameobjectIfSelected()
	{
		if( currentlySelectedUnits.Count > 0 )
		{
			foreach( GameObject go in currentlySelectedUnits )
			{
				go.SendMessage("Deselect");
			}

			currentlySelectedUnits.Clear();
		}
	}


	// check if a unit is within the screen space to deal with mouse drag selecting
	public static bool UnitWithinScreenSpace(Vector2 unitScreenPos)
	{
		if( 
				(unitScreenPos.x < Screen.width && unitScreenPos.y < Screen.height) &&
				(unitScreenPos.x > 0f && unitScreenPos.y > 0)
		  )
			return true;
		else
			return false;
	}


	// remove a unit from screen units unitsOnScreen arraylist
	public static void RemoveFromUnitsOnScreen(GameObject unit)
	{
		for( int i = 0; i < unitsOnScreen.Count; i++ )
		{
			GameObject unitObj = unitsOnScreen[i] as GameObject;

			if( unit == unitObj )
			{
				unitsOnScreen.RemoveAt(i);
				return;
			}
		}
	}


#endregion

}

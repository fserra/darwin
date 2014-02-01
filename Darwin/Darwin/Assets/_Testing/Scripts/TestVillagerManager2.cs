using UnityEngine;
using System.Collections;
using Pathfinding;

public class TestVillagerManager2 : MonoBehaviour, IVillagerManager
{
	public int currentHealth;
	Unit unit;
	
	// Movement
	private Seeker seeker;
	private CharacterController controller;
	public Path path;
	public float nextWaypointDistance = 3;
	private int currentWaypoint = 0;
	
	// Use this for initialization
	void Start ()
	{
		unit = GetComponent<Unit> ();
		seeker = GetComponent<Seeker>();
		controller = GetComponent<CharacterController>();
		currentHealth = unit.health;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (currentHealth <= 0) {
			GameMaster.resources[4]++;
			Destroy(gameObject);
		}
	}
	
	void FixedUpdate ()
	{
		Vector3 gravity = Vector3.zero;
		gravity.y = -20 * Time.fixedDeltaTime;
		controller.Move (gravity);

		if (path == null)
		{
			//We have no path to move after yet
			return;
		}
		
		if (currentWaypoint >= path.vectorPath.Count)
		{
			Debug.Log ("End Of Path Reached");
			return;
		}
		
		//Direction to the next waypoint
		Vector3 dir = (path.vectorPath[currentWaypoint]-transform.position).normalized;
		dir *= unit.speed * Time.fixedDeltaTime;
		controller.SimpleMove (dir);
		
		//Check if we are close enough to the next waypoint
		//If we are, proceed to follow the next waypoint
		if (Vector3.Distance (transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
		{
			currentWaypoint++;
			return;
		}
	}
	
	public void Action(object[] args)
	{
		Debug.Log ("not implemented yet");
		if( (string)args[0] == "rc" )
		{
			Debug.Log ("saying move to " + (Vector3)args[2]);
			this.Move((Vector3)(args[2]));
		}
	}
	
	public void Move(Vector3 position)
	{
		//Start a new path to the targetPosition, return the result to the OnPathComplete function
		seeker.StartPath (transform.position, position, OnPathComplete);
	}
	
	public void Gather(GameObject resource)
	{
		Debug.Log ("not implemented yet");
	}
	
	public void Drop()
	{
		Debug.Log ("not implemented yet");
	}
	
	public void OnPathComplete (Path p)
	{
		Debug.Log ("Yey, we got a path back. Did it have an error? "+p.error);
		if (!p.error) {
			path = p;
			//Reset the waypoint counter
			currentWaypoint = 0;
		}
	}
}

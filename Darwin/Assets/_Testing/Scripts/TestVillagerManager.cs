using UnityEngine;
using System.Collections;
using Pathfinding;

public class TestVillagerManager : MonoBehaviour, IVillagerManager
{
	public int currentHealth;
	Unit unit;

	// Movement
	private AIPath aiPath;
	
	// Use this for initialization
	void Start ()
	{
		unit = GetComponent<Unit> ();
		aiPath = GetComponent<AIPath>();
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
		GameObject target = new GameObject ();
		target.transform.position = position;
		aiPath.target = target.transform;
		aiPath.SearchPath ();
	}
	
	public void Gather(GameObject resource)
	{
		Debug.Log ("not implemented yet");
	}

	public void Drop()
	{
		Debug.Log ("not implemented yet");
	}

}

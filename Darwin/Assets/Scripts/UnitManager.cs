using UnityEngine;
using System.Collections;

public class UnitManager : MonoBehaviour
{
	public int currentHealth;
	Unit unit;

	void Awake()
	{
		// add it to unit list
		Debug.Log("Awaking " + this.gameObject);
		GameMaster.unitObjects.Add(this.gameObject);
	}

	// Use this for initialization
	void Start ()
	{
		unit = GetComponent<Unit> ();
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

	void OnGUI ()
	{
		if (GUI.Button (new Rect (Screen.width * 0.5f, Screen.height - 300, 100, 30), "Hit Units")) {
			currentHealth -= 10;
		}
	}
}

﻿using UnityEngine;
using System.Collections;

public class BuildingManager : MonoBehaviour
{
	Building building;
	Transform spawner;
	bool selected;

	void Awake()
	{
		// add it to building list
		GameMaster.buildingObjects.Add(this.gameObject);

		// update graph
		AstarPath.active.UpdateGraphs (transform.collider.bounds);

		selected = false;
	}

	// Use this for initialization
	void Start ()
	{
		building = GetComponent<Building> ();
		spawner = transform.Find ("Spawn");
	}
	
	// Update is called once per frame
	void Update ()
	{
		if( selected )
		{
			if (Input.GetKeyDown ("v"))
			{
				CreateUnit (0);
			}

			if (Input.GetKeyDown ("s"))
			{
				CreateUnit (1);
			}
		}
	}

	public void OnGUI ()
	{
		if( selected )
		{
			GUILayout.BeginArea (new Rect (Screen.width - 110, Screen.height - 300, 100, 300));
			GUILayout.BeginVertical ();

			foreach (int unitIndex in building.availableUnits)
			{
				if(GUILayout.Button (UnitsInfo.unitNames[unitIndex]))
				{
					CreateUnit(unitIndex);
				}
			}

			GUILayout.EndVertical ();
			GUILayout.EndArea ();
		}
	}

	public void CreateUnit (int unitIndex)
	{
		bool canAfford = true;

		// Check if all the resources are >= to the costs
		for (int i = 0; i <= 4; i++)
		{
			canAfford = canAfford && (GameMaster.resources[i] >= UnitsInfo.unitCosts[unitIndex, i]);
		}
		
		if (canAfford)
		{
			// Create and discount
			Instantiate(Resources.Load("Prefabs/Units/" + UnitsInfo.unitNames[unitIndex]), spawner.position, spawner.rotation);
			for (int i = 0; i <= 4; i++)
			{
				GameMaster.resources[i] -= UnitsInfo.unitCosts[unitIndex, i];
			}
		}
	}

	void Select()
	{
		selected = true;
		this.gameObject.transform.FindChild("Selected").gameObject.SetActive(true);
	}

	void Deselect()
	{
		selected = false;
		this.gameObject.transform.FindChild("Selected").gameObject.SetActive(false);
	}
}

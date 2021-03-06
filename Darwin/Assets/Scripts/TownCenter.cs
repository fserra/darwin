﻿using UnityEngine;
using System.Collections;

public class TownCenter : Building
{
	public void Awake ()
	{
		// add it to towncenter list
		GameMaster.towncenterObjects.Add(this.gameObject);

		availableUnits = new int[2] {0, 1};
		buildingName = "Town Center";
		attack = 10;
		range = 1.5f;
		health = 800;
		armor = 2;
		fov = 2.0f;
	}
}

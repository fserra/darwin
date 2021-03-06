﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// By using 'static' only one instance
// of the variable can exist per class.
// (all objects share the same 
// instance of the variable)
// This means that there is going to be 
// a unique wood (for every object using GameMaster)
// This way if different players use this script
// they will all have the same amount of wood food etc.
// http://answers.unity3d.com/questions/50466/get-variables-from-other-scripts.html
// static should be avoided
public class GameMaster : MonoBehaviour
{
	// TODO: we want to reference these without having an object, hence static
	// at least for now, maybe when we have multiple players we need
	// to create one object for each of them
	public static List<GameObject> unitObjects = new List<GameObject>();
	public static List<GameObject> buildingObjects = new List<GameObject>();
	public static List<GameObject> resourceObjects = new List<GameObject>();
	public static List<GameObject> towncenterObjects = new List<GameObject>(); 

	public static int wood = 100;
	public static int food = 200;
	public static int populationCap = 3;
	public static int[] resources;
	//public static Dictionary<string, int> resources;
	int population;

	int topHeight = 30;
	int topWidth = 300;
	

	// Use this for initialization
	void Start ()
	{
		// show current population but use the remaining population as a resource
		//resources.Add("Wood", wood);
		//resources.Add("Food", food);
		//resources.Add("PopulationCap", food);
		resources = new int[] {wood, food, 0, 0, populationCap};
	}

	void Update ()
	{
		//population = populationCap - resources["PopulationCap"];
		population = populationCap - resources[4];
	}
	
	// Update is called once per frame
	void OnGUI ()
	{
		GUI.Box (new Rect (5, 5, topWidth, topHeight), "Wood: " + resources[0].ToString() + " | Food: " + resources[1].ToString() + " | Population: " + population.ToString() + "/" + populationCap.ToString());
	}
}

using UnityEngine;
using System.Collections;

public class Villager : Unit
{
    public int[]   resourceMaxCarry;
    public float[] resourceMiningSpeed;

	void Awake ()
	{
		unitName = "Villager";
		health   = 50 + Random.Range (-5, 5);
		attack   = 3 + Random.Range (-1, 1);
		range    = 0f;
		armor    = 0;
		fov      = 1.5f;
		speed    = 200.0f;

		resourceMaxCarry = new int[]{10, 12}; // wood, food
		resourceMiningSpeed = new float[]{1f, 0.7f}; // wood, food
	}
}

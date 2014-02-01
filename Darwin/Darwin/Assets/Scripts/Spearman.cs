using UnityEngine;
using System.Collections;

public class Spearman : Unit
{
	// Use this for initialization
	void Awake ()
	{
		unitName = "Spearman";
		health = 120;
		attack = 5;
		range = 0.2f;
		armor = 1;
		fov = 1.0f;
		speed = 1.1f;
	}
}
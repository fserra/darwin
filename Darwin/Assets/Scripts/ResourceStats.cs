using UnityEngine;
using System.Collections;

public class ResourceStats : MonoBehaviour
{
	public int maxResources = 500;
	public int currentResources;
	public int resourceType = 0;


	// Use this for initialization
	void Start ()
	{
		currentResources = maxResources;
	}


	// Update is called once per frame
	void Update ()
	{
		if (currentResources <= 0)
		{
			gameObject.SetActive(false);
		}

		if (Input.GetKeyDown("r"))
		{
			currentResources -= 50;
			GameMaster.resources[resourceType] += 50;
		}
	}
}

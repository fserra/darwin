using UnityEngine;
using System.Collections;

public class TestMove : MonoBehaviour
{
	public VillagerManager Manager;
	public Transform target;

	// Use this for initialization
	void Start ()
	{
		Manager.Move (target.position);
	}
}

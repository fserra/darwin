using UnityEngine;
using System.Collections;

interface IVillagerManager
{
	void Action(object[] args);

	void Move(Vector3 position);

	void Gather(GameObject resource);

	void Drop();
}

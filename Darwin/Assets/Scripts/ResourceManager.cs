using UnityEngine;
using System.Collections;

public class ResourceManager : MonoBehaviour 
{

    Resource resource;
    public int currentResources;

	void Awake()
	{
        // add it to resource list
        GameMaster.resourceObjects.Add(this.gameObject);
	}

	// Use this for initialization
	void Start ()
    {
        resource = GetComponent<Resource>();
        currentResources = resource.resources;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if( currentResources <= 0 )
        {
            if( currentResources < 0 )
                Debug.LogError("Imposible! Resources should not be negative");

            Destroy(gameObject);
        }
	}

    void OnDestroy()
    {
        GameMaster.resourceObjects.Remove(this.gameObject);
    }

    public void Mine()
    {
        currentResources--;
	    if( currentResources <= 0 )
        {
            if( currentResources < 0 )
                Debug.LogError("Imposible! Resources should not be negative");

            Destroy(gameObject);
        }
    }
}

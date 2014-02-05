using UnityEngine;
using System.Collections;
using Pathfinding;

// villager has different status
// if action is right click on tree
// villager enters tree mining status
// It checks whether he can mine tree 
// (because of distance or because max carry)
// If he cannot because of distance, then moves
// if not because of carry, return to towncenter
public class VillagerManager : MonoBehaviour
{
    public enum Status
    {
        Idle,
            MovingToPoint,
            MineStartMining,
            MineStartMoveToMine,
            MineMovingToMine,
            MineStartMoveToStorage,
            MineMovingToStorage,
            MineMining,
            MineDeposit,
            MineSearchForMine
    }

    public int currentHealth;
    Villager unit;
    public Status status;

    // Mining
    ResourceManager resource;
    GameObject associatedResource;
    GameObject associatedStorage;
    Vector3 resourcePosition;
    public int  resourceType;
    public int  resourceCarry;
    public string  resourceName;
    public bool isMining;

    // Movement
    private Seeker seeker;
    private CharacterController controller;
    public Path path;
    public float nextWaypointDistance = 3;
    private int currentWaypoint = 0;
    bool destinationReached;

    //debug
    int ncoroutine;
    bool isequal;

    void Awake()
    {
        // add it to unit list
        //Debug.Log("Awaking " + this.gameObject);
        GameMaster.unitObjects.Add(this.gameObject);
        destinationReached = true;

        // debug
        ncoroutine = 0;
        isequal = true;

    }

    // Use this for initialization
    void Start ()
    {
        unit = GetComponent<Villager> ();
        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController>();
        currentHealth = unit.health;
    }

    // Update is called once per frame
    void Update ()
    {
        if( !isequal )
        {
            Debug.LogError("Not equal, now " + ncoroutine + " times");
            isequal = true;
        }
        if (currentHealth <= 0) {
            GameMaster.resources[4]++;
            Destroy(gameObject);
        }
    }

    void FixedUpdate ()
    {
        Vector3 gravity = Vector3.zero;
        gravity.y = -20 * Time.fixedDeltaTime;
        controller.Move (gravity);

        if (path == null)
        {
            //We have no path to move after yet
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            //Debug.Log ("End Of Path Reached");
            destinationReached = true;
            return;
        }

        //Direction to the next waypoint
        Vector3 dir = (path.vectorPath[currentWaypoint]-transform.position).normalized;
        dir *= unit.speed * Time.fixedDeltaTime;
        controller.SimpleMove (dir);

        //Check if we are close enough to the next waypoint
        //If we are, proceed to follow the next waypoint
        if (Vector3.Distance (transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
        {
            currentWaypoint++;
            return;
        }
    }

    //void OnGUI ()
    //{
    //if (GUI.Button (new Rect (Screen.width * 0.5f, Screen.height - 300, 100, 30), "Hit Units")) {
    //currentHealth -= 10;
    //}
    //}

    public void Action(object[] args)
    {
        Debug.Log ("not implemented yet");
        if( (string)args[0] == "rc" )
        {
            GameObject go = (GameObject)args[1];
            //Debug.Log ("saying move to " + (Vector3)args[2]);
            Debug.Log ("saying move towards " + go.name);

            if( go.name == "Terrain" )
            {
                StopAllCoroutines();
                Move((Vector3)(args[2]));
                status = Status.MovingToPoint;
                isMining = false;
            }
            // FIXME: maybe tag them resources and obtain the type
            // from the game object? Until now logic is the same
            else if( go.tag == "Wood" )
            {
                Debug.Log ("Start mining wood");
                if( !isMining || go != associatedResource )
                {
                    associatedResource = go;
                    resourceType = 0;
                    resourceName = go.tag;
                    resource = go.GetComponent<ResourceManager>();
                    resourcePosition = go.transform.position;
                    StopAllCoroutines();
                    StartCoroutine(StartMining());
                    Debug.Log ("Coroutine started");
                }
            }
            else if( go.tag == "Food" )
            {
                Debug.Log ("Start mining food");
                if( !isMining || go != associatedResource )
                {
                    associatedResource = go;
                    resourceType = 1;
                    resourceName = go.tag;
                    resource = go.GetComponent<ResourceManager>();
                    resourcePosition = go.transform.position;
                    StopAllCoroutines();
                    StartCoroutine(StartMining());
                    Debug.Log ("Coroutine started");
                }
            }

        }
        Debug.Log ("Leaving action");
    }


    // Mining coroutine
    // Moves to mine and mines until capacity
    // is reached. Then goes to storage facility
    // to drop the minerals
    // The mining process is defined as follows: the miner first checks whether
    // the resource still exists (another miner could have extract it all
    // while he was walking towards the resource). In case it doesn't exist,
    // the miner looks for a near mine of the same type. Then 
    // it extracts one mineral right away  and then takes unit.resourceMiningSpeed
    // seconds to process the taken mineral. This way we avoid the possibility 
    // of being waiting for a mineral, meanwhile another unit takes the last mineral.
    IEnumerator StartMining()
    {
        isMining = false;

        while( true )
        {
            // move to mine
            Move(resourcePosition, "To mine");

            // wait until close enough
            while( IsTooFarFromMine() )
            {
                Debug.Log("Moving to mine");
                yield return null;
            }
            Debug.Log("got to mine");

            // if mine doesn't exist, look for a new one and start again
            // if non is found, end routine
            if( !IsResourceAvailable() )
            {
                Debug.Log("resource not available");
                if( AssociateNewResouce() )
                    continue;
                else
                    yield break;
            }
            Debug.Log("resource is available");

            // mine until full
            while( resourceCarry < unit.resourceMaxCarry[resourceType] )
            {
                Debug.Log("mining");
                // check if resource still exists and is minable
                // NOTE: could it be that the resource
                // stops existing but this wasn't updated?
                if( !IsResourceAvailable() )
                {
                    if( !AssociateNewResouce() )
                        break;
                }

                isMining = true;
                yield return new WaitForSeconds(unit.resourceMiningSpeed[resourceType]);
                resource.Mine();
                isMining = false;
                resourceCarry += 1;

            }

            // drop, move to storage only if carrying resource
            if( resourceCarry > 0 )
            {
                associatedStorage = LocateNearestStorageBuilding();
                Move(associatedStorage.transform.position, "To storage");

                // wait until close enough
                while( IsTooFarFromStorage() )
                {
                    yield return null;
                }

                // drop logic
                GameMaster.resources[resourceType] += resourceCarry;
                resourceCarry = 0;
            }
            else
                yield break;
        }
    }


    // check if resource is null or it doesn't have more minerals
    bool IsResourceAvailable()
    {
        Debug.Log(associatedResource + " has " + resource.currentResources);
        return associatedResource != null & resource.currentResources > 0;
    }



    // Associates a new resource of the same type to the villager
    bool AssociateNewResouce()
    {
        bool foundNew = false;

        Collider[] hitColliders = Physics.OverlapSphere(resourcePosition, 10, 512);
        Debug.Log("Sphere hitted on " + hitColliders.Length);
        foreach( Collider col in hitColliders )
        {
            Debug.Log(col);
            if( col.tag == resourceName && col.gameObject != associatedResource )
            {
                associatedResource = col.gameObject;
                resource = col.gameObject.GetComponent<ResourceManager>();
                resourcePosition = col.gameObject.transform.position;
                foundNew = true;
                break;
            }
        }

        return foundNew;
    }


    GameObject LocateNearestStorageBuilding()
    {
        GameObject nearest;
        float minimumDistance;

        nearest = null;
        minimumDistance = Mathf.Infinity;
        foreach( GameObject tc in GameMaster.towncenterObjects )
        {
            if( minimumDistance > Vector3.Distance(unit.transform.position, tc.transform.position) )
            {
                minimumDistance = Vector3.Distance(unit.transform.position, tc.transform.position);
                nearest = tc;
            }
        }
        return nearest;
    }

    bool IsTooFarFromMine()
    {
        float distance;

        distance = Vector3.Distance(resourcePosition, unit.transform.position);
        //Debug.Log("Distance = " + distance);

        if( distance > 1.5 )
            return true;
        else
            return false;
    }

    bool IsTooFarFromStorage()
    {
        float distance;

        distance = Vector3.Distance(associatedStorage.transform.position, unit.transform.position);
        //Debug.Log("Distance = " + distance);

        if( distance > 4 )
            return true;
        else
            return false;
    }

    public void Move(Vector3 position, string where = "nowhere")
    {
        Debug.LogError("Being called from " + where);
        //Start a new path to the targetPosition, return the result to the OnPathComplete function
        seeker.StartPath (transform.position, position, OnPathComplete);
    }

    public void Gather(GameObject resource)
    {
        Debug.Log ("not implemented yet");
    }

    public void Drop()
    {
        Debug.Log ("not implemented yet");
    }

    public void OnPathComplete (Path p)
    {
        //Debug.Log ("Yey, we got a path back. Did it have an error? "+p.error);
        if (!p.error) {
            path = p;
            //Reset the waypoint counter
            currentWaypoint = 0;
            destinationReached = false;
        }
    }

    void Select()
    {
        this.gameObject.transform.FindChild("Selected").gameObject.SetActive(true);
    }

    void Deselect()
    {
        this.gameObject.transform.FindChild("Selected").gameObject.SetActive(false);
    }
}

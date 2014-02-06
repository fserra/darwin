using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

    // actions management
    public struct action
    {
        public string name;
        public object[] args;

        // constructur
        public action(string name)
        {
            this.name = name;
            this.args = null;
        }
    }
    LinkedList<action> actions = new LinkedList<action>();


    // stats
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
        if( (string)args[0] == "rc" )
        {
            GameObject go = (GameObject)args[1];
            //Debug.Log ("saying move to " + (Vector3)args[2]);
            Debug.Log ("saying move towards " + go.name);
            
            // process action
            if( go.name == "Terrain" )
            {
                actions.Clear();

                StopAllCoroutines();
                action move = new action();
                move.name = "Move";
                move.args = new object[1];
                move.args[0] = (Vector3)(args[2]);
                actions.AddFirst(move);
                actions.AddLast(new action("Idle"));
            }
            else if( go.tag == "Wood" )
            {
                Debug.Log ("Start mining wood");
                if( !isMining || go != associatedResource )
                {
                    actions.Clear();
                    StopAllCoroutines();

                    action moveMine = new action("Move");
                    moveMine.args = new object[1];
                    moveMine.args[0] = go.transform.position;

                    action mine = new action("Mine");
                    mine.args = new object[1];
                    mine.args[0] = go;

                    action findstorage = new action("FindStorage");

                    action moveStorage = new action("Move");
                    moveStorage.args = new object[1];

                    action drop = new action("Drop");

                    actions.AddFirst(moveMine);
                    actions.AddLast(mine);
                    actions.AddLast(findstorage);
                    actions.AddLast(moveStorage);
                    actions.AddLast(drop);
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
                    //StartCoroutine(StartMining());
                    Debug.Log ("Coroutine started");
                }
            }

            ExecAction(actions.First);
        }
        else
        {
            Debug.Log ("not implemented yet");
        }
        Debug.Log ("Leaving action");
    }

    // execute actions
    void ExecAction(LinkedListNode<action> actionNode)
    {
        if( actions.Count == 0 )
        {
            Debug.LogError("There should be at least one action");
            return;
        }

        action act = actionNode.Value;

        if( act.name == "Move" )
        {
            Debug.Log("Executing move");
            StartCoroutine(ActionMove(actionNode));
        }
        else if( act.name == "Idle" )
        {
            Debug.Log("Executing idle action");
            StopAllCoroutines();
            return;
        }
        else if( act.name == "Mine" )
        {
            Debug.Log("Executing mine");
            StartCoroutine(Mine(actionNode));
            return;
        }
        else if( act.name == "FindStorage" )
        {
            Debug.Log("Executing findstorage");
            FindStorage(actionNode);
            return;
        }
        else if( act.name == "Drop" )
        {
            Debug.Log("Executing drop");
            Drop(actionNode);
            return;
        }
        else
        {
            Debug.LogError("Unkown action " + act.name);
            return;
        }
    }

    // Moving action. Tells the objecto to move
    // and when it completes the movement, goes
    // to the next action
    // NOTE: might be slow?
    IEnumerator ActionMove(LinkedListNode<action> actionNode)
    {
        Debug.Log("actions " + actions.Count);
        action act = actionNode.Value;

        Move((Vector3)act.args[0]);

        yield return null;
        while( currentWaypoint < path.vectorPath.Count )
            yield return new WaitForSeconds(0.1f);

        Debug.Log("Finish moving. should go to next action");
        ExecAction(NextAction(actionNode));
    }

    // Circular behavior on LinkedList
    public LinkedListNode<action> NextAction(LinkedListNode<action> node)
    {
        return node.Next == null ? node.List.First : node.Next;
    }

    // mines until max capacity is reached or resources are over
    // In case current mining resource is over the miner looks 
    // for a near mine of the same type to continue mining.
    IEnumerator Mine(LinkedListNode<action> actionNode)
    {
        isMining = false;
        GameObject go = (GameObject)actionNode.Value.args[0];
        // There must be a better way of doing this
        resource = go.GetComponent<ResourceManager>();
        resourcePosition = go.transform.position;
        associatedResource = go;

        // should be close enough
        if( IsTooFarFromMine() )
        {
            Debug.LogError("Move failed, too far for mining");
            yield break;
        }

        // if mine doesn't exist, look for a new one and start again
        // if non is found, end routine
        if( !IsResourceAvailable() )
        {
            Debug.Log("resource not available");
            if( !AssociateNewResouce() )
            {
                Debug.Log("should get idle");
                yield break;
            }
        }
        Debug.Log("resource is available");

        // mine until full
        while( resourceCarry < unit.resourceMaxCarry[resourceType] )
        {
            Debug.Log("gathering");
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
            resourceCarry += 1;
            isMining = false;
        }

        ExecAction(NextAction(actionNode));
    }

    // look for closest storage
    public void FindStorage(LinkedListNode<action> actionNode)
    {
        associatedStorage = LocateNearestStorageBuilding();

        actionNode.Next.Value.args[0] = associatedStorage.transform.position;
        ExecAction(NextAction(actionNode));
    }

    // drop minerals on storage
    public void Drop(LinkedListNode<action> actionNode)
    {
        // should be close enough
        if( IsTooFarFromStorage() )
        {
            Debug.LogError("Move failed, too far to drop");
            return;
        }

        // drop logic
        GameMaster.resources[resourceType] += resourceCarry;
        resourceCarry = 0;

        ExecAction(NextAction(actionNode));
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
        Debug.Log("Distance from mine = " + distance);

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

    public void Move(Vector3 position)
    {
        //Start a new path to the targetPosition, return the result to the OnPathComplete function
        seeker.StartPath (transform.position, position, OnPathComplete);

    }

    public void Gather(GameObject resource)
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

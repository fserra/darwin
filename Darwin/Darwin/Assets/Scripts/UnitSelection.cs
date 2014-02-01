using UnityEngine;
using System.Collections;

/*
 * script should be attached to all controllable units in the game
 * whether they are walkable or not (buildings)
 */
public class UnitSelection : MonoBehaviour
{
    // for Mouse.cs
    public Vector2 screenPos;
    public bool onScreen;
    public bool selected = false;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        // if unit not selected, get screen space
        if( !selected )
        {
            // track screen position
            screenPos = Camera.main.WorldToScreenPoint(transform.position);

            // if within screen space
            if( Mouse.UnitWithinScreenSpace(screenPos) )
            {
                // and not added to unitsOnScreen, add it
                if( !onScreen )
                {
                    Mouse.unitsOnScreen.Add(gameObject);
                    onScreen = true;
                }
            }
            else
            {
                // unit not in screen space, remove if it was
                if( onScreen )
                {
                    Mouse.RemoveFromUnitsOnScreen(gameObject);
                }
            }
        }
	}
}

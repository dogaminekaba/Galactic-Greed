using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    private static bool alive = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // collision detection here

    public static bool isAlive()
    {
        if(alive)
            return true;
        else
            return false;
    }
}

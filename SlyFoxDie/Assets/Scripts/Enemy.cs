using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    
    //TODO: In game manager handle instantiation and destruction of enemy prefab instance

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //TODO:
    //Determine start + end locations
    //Build open + closed set of traversable cells
    //Determine heuristic
    //Once path has been found, retrace and return path

    //Can use Maze + MazeEdge to get neighbor information
    void PathFinding()
    {

    }
}

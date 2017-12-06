using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeWall : MazeCellEdge {

    public Transform wall;

    public override void Initialize(MazeCell cell, MazeCell otherCell, MazeDirection direction)
    {
        base.Initialize(cell, otherCell, direction);
        if(gameObject.tag != "Safe")
        {
            wall.GetComponent<Renderer>().material = cell.room.settings.wallMaterial;
        }
    }
}

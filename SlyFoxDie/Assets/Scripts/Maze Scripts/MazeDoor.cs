using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Like a MazePassage, with a door component attached
public class MazeDoor : MazePassage {

    public Transform hinge;

    public Transform switchTrans;

    public static Quaternion normalRotation = Quaternion.Euler(0f, -90f, 0f),
                             mirroredRotation = Quaternion.Euler(0f, 90f, 0f);

    private bool isMirrored;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private MazeDoor OtherSideOfDoor
    {
        get
        {
            return otherCell.GetEdge(direction.GetOpposite()) as MazeDoor;
        }
    }

    public override void Initialize(MazeCell primary, MazeCell other, MazeDirection direction)
    {
        base.Initialize(primary, other, direction);
        if(OtherSideOfDoor != null)
        {
            //TODO: Should not use negative scale (box colliders have issues)
            isMirrored = true;
            hinge.localScale = new Vector3(-1f, 1f, 1f);
            Vector3 p = hinge.localPosition;
            p.x = -p.x;
            hinge.localPosition = p;
        }
        for(int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild(i);
            if(child != hinge && child != switchTrans)
            {
                child.GetComponent<Renderer>().material = cell.room.settings.wallMaterial;
            }
        }
    }

    public override void OnPlayerEntered()
    {
        OtherSideOfDoor.hinge.localRotation = hinge.localRotation = isMirrored ? mirroredRotation : normalRotation;
    }

    public override void OnPlayerExited()
    {
        OtherSideOfDoor.hinge.localRotation = hinge.localRotation = Quaternion.identity;
    }
}

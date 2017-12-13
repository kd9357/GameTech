using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    private MazeCell currentCell;

    private MazeDirection currentDirection;

    private MazeCell origin;

    private bool hasTreasure = false;
    private bool canMove = true;
    private bool isMoving = false;

    public Text treasureText;
    public float Speed = 10f;

    public void SetLocation(MazeCell cell)
    {
        if(currentCell != null)
        {
            currentCell.OnPlayerExited();
        }
        currentCell = cell;
        StartCoroutine(Move(cell));
        currentCell.OnPlayerEntered();
        if (cell.canHide)
        {
            foreach(Transform trans in gameObject.GetComponentsInChildren<Transform>())
                trans.gameObject.layer = LayerMask.NameToLayer("Default");
        }
        else
        {
            foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>())
                trans.gameObject.layer = LayerMask.NameToLayer("Player");
        }
        if (cell == origin && hasTreasure)
        {
            GameManager.Instance.GameOver(true);
        }
        else if(!hasTreasure)
        {
            MazeCellEdge[] edges = currentCell.GetEdges();
            foreach (MazeCellEdge e in edges)
            {
                if (e.tag == "Safe")
                {
                    hasTreasure = true;
                    treasureText.text = "You have the treasure! Find your way back!";
                }
            }
        }
    }

    public IEnumerator Move(MazeCell cell)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = cell.transform.localPosition;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * Speed;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }
        isMoving = false;
        //currentCell = cell;
        yield return 0;
    }

    public void Activate(MazeCell cell)
    {
        SetLocation(cell);
        origin = cell;
    }

    public void disableMovement()
    {
        canMove = false;
    }

    public MazeCell GetLocation()
    {
        return currentCell;
    }

    private void Move(MazeDirection direction)
    {
        MazeCellEdge edge = currentCell.GetEdge(direction);
        if(edge is MazePassage)
        {
            SetLocation(edge.otherCell);
        }
    }

    private void Rotate(MazeDirection direction)
    {
        transform.localRotation = direction.ToRotation();
        currentDirection = direction;
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (canMove)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                Move(currentDirection);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                Move(currentDirection.GetNextClockwise());
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                Move(currentDirection.GetOpposite());
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Move(currentDirection.GetNextCounterclockwise());
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                Rotate(currentDirection.GetNextCounterclockwise());
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                Rotate(currentDirection.GetNextClockwise());
            }
        }
    }
}

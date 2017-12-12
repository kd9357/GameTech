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

    public Text treasureText;

    public void SetLocation(MazeCell cell)
    {
        if(currentCell != null)
        {
            currentCell.OnPlayerExited();
        }
        currentCell = cell;
        transform.localPosition = cell.transform.localPosition;
        currentCell.OnPlayerEntered();
        if (cell == origin && hasTreasure)
        {
            GameManager.Instance.GameOver(true);
        }
        else
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

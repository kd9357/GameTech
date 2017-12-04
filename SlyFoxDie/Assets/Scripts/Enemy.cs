using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float Speed = 3f;
    public Material pathMaterial;
    public GameObject player;

    //TODO: In game manager handle instantiation and destruction of enemy prefab instance
    private List<MazeCell> patrolPath;
    private List<MazeCell> investigationPath;
    private List<MazeCell> doors;

    private MazeCell currentCell;
    private int patrolIndex;
    private bool isMoving;

    private MazeRoom room;
    private bool isHidden;

	// Use this for initialization
	void Start () {
        patrolIndex = 1;
        isMoving = false;
        player = GameObject.FindWithTag("Player");
    }
	
	// Update is called once per frame
	void Update () {
        if (!isMoving)
        {
            if (patrolIndex >= patrolPath.Count - 1)
                patrolIndex = 0;
            StartCoroutine(Move());
            patrolIndex++;
        }
        if(isHidden)
        {
            Hide();
        }
        else
        {
            Show();
        }
	}

    public IEnumerator Move()
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = patrolPath[patrolIndex].transform.localPosition;
        float t = 0;
        while(t < 1f)
        {
            t += Time.deltaTime * Speed;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }
        isMoving = false;
        currentCell = patrolPath[patrolIndex];
        room = currentCell.room;
        if(room != player.GetComponent<Player>().GetLocation().room)
        {
            isHidden = true;
        }
        else
        {
            isHidden = false;
        }
        yield return 0;
    }

    public void Activate(MazeCell start, List<MazeCell> doors)
    {
        currentCell = start;
        room = start.room;
        transform.localPosition = currentCell.transform.localPosition + Vector3.up / 2;

        this.doors = doors;
        this.doors.Sort((a, b) => GameManager.Instance.GetManhattanDistance(start, a).CompareTo(GameManager.Instance.GetManhattanDistance(start, b)));

        PathToPatrol(currentCell);

    }

    public void SetStartLocation(MazeCell cell)
    {
        if (currentCell != null)
        {
            currentCell.OnPlayerExited();
        }
        currentCell = cell;
        transform.localPosition = cell.transform.localPosition + Vector3.up / 2;
        //currentCell.OnPlayerEntered();
    }

    public void Hide()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
        transform.GetChild(0).GetComponent<Renderer>().enabled = false;
    }

    public void Show()
    {
        gameObject.GetComponent<Renderer>().enabled = true;
        transform.GetChild(0).GetComponent<Renderer>().enabled = true;
    }

    #region Pathing
    // Creates a single long patrol path that connects each door and loops back to start
    void PathToPatrol(MazeCell initial)
    { 
        patrolPath = GameManager.Instance.PathFinding(initial, doors[0]);
        List<MazeCell> newPath;
        for (int i = 1; i < doors.Count; i++)
        {
            newPath = GameManager.Instance.PathFinding(doors[i - 1], doors[i]);
            for(int j = 1; j < newPath.Count; j++)
            {
                patrolPath.Add(newPath[j]);
            }
        }
        newPath = GameManager.Instance.PathFinding(doors[doors.Count - 1], initial);
        for(int i = 1; i < newPath.Count - 1; i++)
        {
            patrolPath.Add(newPath[i]);
        }
        SetPatrolPathColor(Color.red);
    }

    public void PathToInvestigate(MazeCell destination)
    {
        investigationPath = GameManager.Instance.PathFinding(currentCell, destination);
        SetInvestigationPathColor(Color.grey);
    }
    
    //Debugging methods
    void SetPatrolPathColor(Color c)
    {
        foreach(MazeCell cell in patrolPath)
        {
            cell.SetMaterialColor(c);
        }
        patrolPath[0].SetMaterialColor(Color.black);
        patrolPath[patrolPath.Count - 1].SetMaterialColor(Color.black);
    }

    void SetInvestigationPathColor(Color c)
    {
        foreach(MazeCell cell in investigationPath)
        {
            cell.SetMaterialColor(c);
        }
    }

    public void ClearPatrolPath()
    {
        foreach(MazeCell cell in patrolPath)
        {
            cell.ResetMaterialColor();
        }
        patrolPath.Clear();
    }

    public void ClearInvestigationPath()
    {
        if (investigationPath == null)
            return;
        foreach(MazeCell cell in investigationPath)
        {
            cell.ResetMaterialColor();
        }
        investigationPath.Clear();
    }

    #endregion
}

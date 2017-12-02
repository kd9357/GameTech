using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float Speed = 3f;
    public Material pathMaterial;

    //TODO: In game manager handle instantiation and destruction of enemy prefab instance
    private List<MazeCell> patrolPath;
    private List<MazeCell> investigationPath;
    private List<MazeCell> doors;

    private MazeCell currentCell;
    private int patrolIndex;
    private bool isMoving;

	// Use this for initialization
	void Start () {
        patrolIndex = 1;
        isMoving = false;
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
        yield return 0;
    }

    public void Activate(MazeCell start, List<MazeCell> doors)
    {
        currentCell = start;
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

    #region Pathing
    struct PathInfo
    {
        public MazeCell cell;
        public List<MazeCell> path;
        public int cost;

        public PathInfo(MazeCell cell, List<MazeCell> path, int cost)
        {
            this.cell = cell;
            this.path = path;
            this.cost = cost;
        }
    }

    private List<MazeCell> PathFinding(MazeCell initial, MazeCell destination)
    {
        List<PathInfo> fringe = new List<PathInfo>();
        Dictionary<MazeCell, int> visited = new Dictionary<MazeCell, int>();

        List<MazeCell> possiblePath = new List<MazeCell>();
        possiblePath.Add(initial);
        fringe.Add(new PathInfo(initial, possiblePath, 0));

        while(fringe.Count > 0)
        {
            //Pop first value off fringe
            PathInfo node = fringe[0];
            for(int i = 1; i < fringe.Count; i++)
            {
                if (fringe[i].cost < node.cost)
                    node = fringe[i];
            }
            fringe.Remove(node);

            //Check if we've reached our target
            if(node.cell == destination)
            {
                possiblePath = node.path;
                possiblePath.Add(destination);
                break;
            }

            //Iterate over cell neighbors
            foreach(MazeCellEdge edge in node.cell.GetEdges())
            {
                MazeCell successor = edge.otherCell;
                if (!(edge is MazePassage))
                    continue;
                int g = node.cost + successor.cost; //TODO: change to + successor.cost if we want to have enemy avoid areas
                int h = GameManager.Instance.GetManhattanDistance(successor, destination);
                int f = g + h;
                if(!visited.ContainsKey(successor) || visited[successor] > g)
                {
                    visited[successor] = g;
                    possiblePath = new List<MazeCell>(node.path);
                    possiblePath.Add(successor);
                    fringe.Add(new PathInfo(successor, possiblePath, f));
                }
            }
        }

        return possiblePath;
    }

    // Creates a single long patrol path that connects each door and loops back to start
    void PathToPatrol(MazeCell initial)
    { 
        patrolPath = PathFinding(initial, doors[0]);
        List<MazeCell> newPath;
        for (int i = 1; i < doors.Count; i++)
        {
            newPath = PathFinding(doors[i - 1], doors[i]);
            for(int j = 1; j < newPath.Count; j++)
            {
                patrolPath.Add(newPath[j]);
            }
        }
        newPath = PathFinding(doors[doors.Count - 1], initial);
        for(int i = 1; i < newPath.Count - 1; i++)
        {
            patrolPath.Add(newPath[i]);
        }
        SetPatrolPathColor(Color.red);
    }

    public void PathToInvestigate(MazeCell destination)
    {
        investigationPath = PathFinding(currentCell, destination);
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

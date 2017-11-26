using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public Material PathMaterial;
    public Material TargetMaterial;

    //TODO: In game manager handle instantiation and destruction of enemy prefab instance
    private MazeCell start;
    private MazeCell end;
    private List<MazeCell> path;

    private MazeCell currentCell;

	// Use this for initialization
	void Start () {
        PathFinding();
        SetPathColor();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //TODO:
    //Determine start + end locations
    //Build open + closed set of traversable cells
    //Determine heuristic
    //Once path has been found, retrace and return path

    public void SetStartLocation(MazeCell cell)
    {
        if (currentCell != null)
        {
            currentCell.OnPlayerExited();
        }
        start = cell;
        currentCell = cell;
        transform.localPosition = cell.transform.localPosition;
        //currentCell.OnPlayerEntered();
    }

    public void SetEndLocation(MazeCell cell)
    {
        end = cell;
    }

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

    ////Can use Maze + MazeEdge to get neighbor information
    //void PathFinding()
    //{
    //    //List<MazeCell> openSet = new List<MazeCell>();
    //    //HashSet<MazeCell> closedSet = new HashSet<MazeCell>();

    //    Dictionary<MazeCell, int> closedSet = new Dictionary<MazeCell, int>();
    //    List<PathInfo> openSet = new List<PathInfo>();

    //    //openSet.Add(start);
    //    openSet.Add(new PathInfo(start, new List<MazeCell>(), 0));

    //    while (openSet.Count > 0)
    //    {
    //        //TODO: Select the lowest cost in openset first
    //        //MazeCell cell = openSet[0];
    //        PathInfo node = openSet[0];
    //        openSet.Remove(node);
    //        MazeCell cell = node.cell;

    //        if (cell == end)
    //        {
    //            //Return node.path;
    //            return;
    //        }

    //        //openSet.Remove(cell);
    //        //closedSet.Add(cell);
    //        closedSet.Add(cell, node.cost);

    //        foreach (MazeCellEdge edge in cell.GetEdges())
    //        {
    //            MazeCell successor = edge.otherCell;
    //            if (!(edge is MazePassage) || closedSet.ContainsKey(successor))
    //                continue;

    //            //int newCostToSuccessor = cell.gCost + GetManhattanDistance(cell, successor);
    //            int newCostToSuccessor = node.cost + GetManhattanDistance(cell, successor);
    //            //if (newCostToSuccessor < successor.gCost || !openSet.Contains(successor))
    //            if (newCostToSuccessor < node.cost || !openSet.Contains(successor))
    //            {
    //                successor.gCost = newCostToSuccessor;
    //                successor.hCost = GetManhattanDistance(successor, end);
    //                successor.parent = cell; //tracks path
    //                if (!openSet.Contains(successor))
    //                    openSet.Add(successor);
    //            }
    //        }

    //    }
    //}

    void PathFinding()
    {
        List<PathInfo> fringe = new List<PathInfo>();
        Dictionary<MazeCell, int> visited = new Dictionary<MazeCell, int>();

        List<MazeCell> possiblePath = new List<MazeCell>();
        possiblePath.Add(start);
        fringe.Add(new PathInfo(start, possiblePath, 0));

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

            if(node.cell == end)
            {
                path = node.path;
                path.Add(end);
                return;
            }

            foreach(MazeCellEdge edge in node.cell.GetEdges())
            {
                MazeCell successor = edge.otherCell;
                if (!(edge is MazePassage))
                    continue;
                int g = node.cost; //TODO: change to + successor.cost if we want to have enemy avoid areas
                int h = GetManhattanDistance(successor, end);
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
    }

    void SetPathColor()
    {
        foreach(MazeCell cell in path)
        {
            cell.SetMaterial(PathMaterial);
        }
        start.SetMaterial(TargetMaterial);
        end.SetMaterial(TargetMaterial);
    }

    int GetManhattanDistance(MazeCell a, MazeCell b)
    {
        return Mathf.Abs(a.coordinates.x - b.coordinates.x) + Mathf.Abs(a.coordinates.z - b.coordinates.z);
    }
}

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

    public void SetStartLocation(MazeCell cell)
    {
        if (currentCell != null)
        {
            currentCell.OnPlayerExited();
        }
        start = cell;
        currentCell = cell;
        transform.localPosition = cell.transform.localPosition + Vector3.up / 2;
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

            //Check if we've reached our target
            if(node.cell == end)
            {
                path = node.path;
                path.Add(end);
                return;
            }

            //Iterate over cell neighbors
            foreach(MazeCellEdge edge in node.cell.GetEdges())
            {
                MazeCell successor = edge.otherCell;
                if (!(edge is MazePassage))
                    continue;
                int g = node.cost + successor.cost; //TODO: change to + successor.cost if we want to have enemy avoid areas
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

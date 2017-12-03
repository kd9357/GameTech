using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance = null;

    #region Public Parameters
    public Maze mazePrefab;
    public Player playerPrefab;

    public Enemy enemyPrefab;

    public bool gameStarted;
    #endregion

    #region Private parameters
    private Maze mazeInstance;
    private Player playerInstance;

    private Enemy enemyInstance;
    #endregion

    //Singleton instantiation
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        gameStarted = false;
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(BeginGame());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    private IEnumerator BeginGame()
    {
        //Instantiate Maze
        Camera.main.clearFlags = CameraClearFlags.Skybox;
        Camera.main.rect = new Rect(0f, 0f, 1f, 1f);
        mazeInstance = Instantiate(mazePrefab) as Maze;
        yield return StartCoroutine(mazeInstance.Generate());

        //Instantiate Enemy
        enemyInstance = Instantiate(enemyPrefab) as Enemy;
        List<MazeCell> doors = mazeInstance.GetDoorCells();
        MazeCell start = mazeInstance.GetCell(mazeInstance.RandomCoordinates);
        enemyInstance.Activate(start, doors);

        ////Instantiate Player
        //playerInstance = Instantiate(playerPrefab) as Player;
        //playerInstance.SetLocation(mazeInstance.GetCell(mazeInstance.RandomCoordinates));
        //Camera.main.clearFlags = CameraClearFlags.Depth;
        //Camera.main.rect = new Rect(0f, 0f, 0.5f, 0.5f);

        gameStarted = true;
    }

    private void RestartGame()
    {
        gameStarted = false;
        StopAllCoroutines();
        Destroy(mazeInstance.gameObject);
        if (playerInstance != null)
        {
            Destroy(playerInstance.gameObject);
        }
        if(enemyInstance != null)
        {
            Destroy(enemyInstance.gameObject);
        }
        StartCoroutine(BeginGame());
    }

    #region Pathing
    public class PathInfo : IHeapItem<PathInfo>
    {
        public MazeCell cell;
        public List<MazeCell> path;
        public int fCost;
        public int hCost;
        int heapIndex;

        public PathInfo(MazeCell cell, List<MazeCell> path, int fCost, int hCost)
        {
            this.cell = cell;
            this.path = path;
            this.fCost = fCost;
            this.hCost = hCost;
        }

        public int HeapIndex
        {
            get
            {
                return heapIndex;
            }
            set
            {
                heapIndex = value;
            }
        }

        public int CompareTo(PathInfo other)
        {
            int compare = fCost.CompareTo(other.fCost);
            if (compare == 0)
            {
                compare = hCost.CompareTo(other.hCost);
            }
            return -compare;
        }
    }

    public List<MazeCell> PathFinding(MazeCell initial, MazeCell destination)
    {
        Heap<PathInfo> fringe = new Heap<PathInfo>(mazeInstance.GetSize());
        Dictionary<MazeCell, int> visited = new Dictionary<MazeCell, int>();

        List<MazeCell> possiblePath = new List<MazeCell>
        {
            initial
        };
        fringe.Add(new PathInfo(initial, possiblePath, 0, GetManhattanDistance(initial, destination)));

        while (fringe.Count > 0)
        {
            PathInfo node = fringe.RemoveFirst();

            //Check if we've reached our target
            if (node.cell == destination)
            {
                possiblePath = node.path;
                possiblePath.Add(destination);
                break;
            }

            //Iterate over cell neighbors
            foreach (MazeCellEdge edge in node.cell.GetEdges())
            {
                MazeCell successor = edge.otherCell;
                if (!(edge is MazePassage))
                    continue;
                int g = node.fCost + successor.cost; //TODO: change to + successor.cost if we want to have enemy avoid areas
                int h = GetManhattanDistance(successor, destination);
                int f = g + h;
                if (!visited.ContainsKey(successor) || visited[successor] > g)
                {
                    visited[successor] = g;
                    possiblePath = new List<MazeCell>(node.path)
                    {
                        successor
                    };
                    fringe.Add(new PathInfo(successor, possiblePath, f, h));
                }
            }
        }

        return possiblePath;
    }
    public int GetManhattanDistance(MazeCell a, MazeCell b)
    {
        return Mathf.Abs(a.coordinates.x - b.coordinates.x) + Mathf.Abs(a.coordinates.z - b.coordinates.z);
    }

    //Tell player (for right now the enemy) what location the mouse is over for pathing
    public void SetDestination(MazeCell cell)
    {
        //playerInstance.SetDestination();
        if (enemyInstance != null)
        {
            enemyInstance.ClearInvestigationPath();
            enemyInstance.PathToInvestigate(cell);
        }
    }
    public void ClearDestination()
    {
        //if(enemyInstance != null)
        //enemyInstance.ClearInvestigationPath();
    }
    #endregion
}

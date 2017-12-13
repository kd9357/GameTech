using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public static GameManager Instance = null;

    #region Public Parameters
    public Maze mazePrefab;
    public Player playerPrefab;

    public Enemy enemyPrefab;

    public bool gameStarted;
    public bool gameOver;

    public Text stateText;
    public Text resetText;
    public Text treasureText;
    public Text rockText;
    #endregion

    #region Private parameters
    private Maze mazeInstance;
    private Player playerInstance;

    private Enemy enemyInstance;
    bool gameWon = false;

    private int rockCount;
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
        gameOver = false;
    }

    // Use this for initialization
    void Start()
    {
        stateText.text = "";
        resetText.text = "";
        treasureText.text = "";
        rockCount = 3;
        rockText.text = "";
        StartCoroutine(BeginGame());
    }

    // Update is called once per frame
    void Update()
    {
        if(gameOver)
        {
            playerInstance.disableMovement();
            enemyInstance.disableMovement();
            if (gameWon)
            {
                stateText.text = "You found the treasure!";
            }
            else
            {
                stateText.text = "You were caught!";
            }
            resetText.text = "Press Space to play again";

            if (Input.GetKeyDown(KeyCode.Space))
            {
                RestartGame();
            }
        }
    }

    public void GameOver(bool won)
    {
        gameOver = true;
        gameWon = won;
    }

    public MazeCell GetPlayerCell()
    {
        return playerInstance.GetLocation();
    }

    public MazeCell GetEnemyCell()
    {
        return enemyInstance.GetCurrentCell();
    }

    public bool ThrowRock()
    {
        if (rockCount > 0)
        {
            --rockCount;
            rockText.text = "Rocks: " + rockCount.ToString();
            return true;
        }
        return false;
    }

    private IEnumerator BeginGame()
    {
        //Instantiate Maze
        Camera.main.clearFlags = CameraClearFlags.Skybox;
        Camera.main.rect = new Rect(0f, 0f, 1f, 1f);
        mazeInstance = Instantiate(mazePrefab) as Maze;
        yield return StartCoroutine(mazeInstance.Generate());

        //Instantiate Player
        playerInstance = Instantiate(playerPrefab) as Player;
        IntVector2 coord = new IntVector2(0, 0);
        MazeCell currentCell = mazeInstance.GetCell(coord);
        MazeCell safeCell = mazeInstance.GetSafeCell();
        MazeCell playerCell = currentCell;
        int maxDistance = 0;
        int sizeX = mazeInstance.size.x;
        int sizeZ = mazeInstance.size.z;
        for(int i = 0; i < sizeX; ++i)
        {
            coord.x = i;
            for(int j = 0; j < sizeZ; ++j)
            {
                coord.z = j;
                currentCell = mazeInstance.GetCell(coord);
                int distance = GetManhattanDistance(currentCell, safeCell);
                if(distance > maxDistance)
                {
                    maxDistance = distance;
                    playerCell = currentCell;
                }
            }
        }
        playerInstance.Activate(playerCell);
        playerCell.SetMaterialColor(Color.blue);
        playerInstance.treasureText = treasureText;

        //Instantiate Enemy
        enemyInstance = Instantiate(enemyPrefab) as Enemy;
        List<MazeCell> doors = mazeInstance.GetDoorCells();
        coord = new IntVector2(0, 0);
        currentCell = mazeInstance.GetCell(coord);
        MazeCell enemyCell = currentCell;
        maxDistance = 0;
        sizeX = mazeInstance.size.x;
        sizeZ = mazeInstance.size.z;
        for (int i = 0; i < sizeX; ++i)
        {
            coord.x = i;
            for (int j = 0; j < sizeZ; ++j)
            {
                coord.z = j;
                currentCell = mazeInstance.GetCell(coord);
                int distance = GetManhattanDistance(currentCell, playerCell);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    enemyCell = currentCell;
                }
            }
        }
        enemyInstance.Activate(enemyCell, doors);

        rockText.text = "Rocks: " + rockCount.ToString();

        gameStarted = true;
    }

    private void RestartGame()
    {
        gameStarted = false;
        gameOver = false;
        rockCount = 3;
        stateText.text = "";
        resetText.text = "";
        treasureText.text = "";
        rockText.text = "";
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

    //From https://github.com/kd9357/cs343-search (my code from AI class)
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
                    //Update fringe if it's already in there!
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

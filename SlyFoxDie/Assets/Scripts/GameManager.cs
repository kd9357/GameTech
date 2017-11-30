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
        MazeCell start = doors[0];
        enemyInstance.SetStartLocation(start);
        MazeCell end = doors[doors.Count - 1];
        enemyInstance.SetEndLocation(end);

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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    #region Public Parameters
    public Maze mazePrefab;
    public Player playerPrefab;

    public Enemy enemyPrefab;
    #endregion

    #region Private parameters
    private Maze mazeInstance;
    private Player playerInstance;

    private Enemy enemyInstance;
    #endregion

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
        MazeCell start = mazeInstance.GetCell(mazeInstance.RandomCoordinates);
        enemyInstance.SetStartLocation(start);
        MazeCell end = mazeInstance.GetCell(mazeInstance.RandomCoordinates);
        //Gacky
        while(end == start)
        {
            end = mazeInstance.GetCell(mazeInstance.RandomCoordinates);
        }
        enemyInstance.SetEndLocation(end);

        ////Instantiate Player
        //playerInstance = Instantiate(playerPrefab) as Player;
        //playerInstance.SetLocation(mazeInstance.GetCell(mazeInstance.RandomCoordinates));
        //Camera.main.clearFlags = CameraClearFlags.Depth;
        //Camera.main.rect = new Rect(0f, 0f, 0.5f, 0.5f);
    }

    private void RestartGame()
    {
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
}

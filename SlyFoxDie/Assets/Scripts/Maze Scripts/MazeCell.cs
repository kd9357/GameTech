using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCell : MonoBehaviour {

    public IntVector2 coordinates;
    public MazeRoom room;
    [Tooltip("The cost it takes to move across this cell. Higher costs = greater avoidance")]
    public int cost = 1;
    public Color mouseOverColor;
    public bool canHide;

    private Renderer floorRenderer;
    private MazeCellEdge[] edges = new MazeCellEdge[MazeDirections.Count];
    private int initializedEdgeCount;
    private Color currentColor;

    public void Initialize(MazeRoom room)
    {
        room.Add(this);
        floorRenderer = transform.GetChild(0).GetComponent<Renderer>();
        floorRenderer.material = room.settings.floorMaterial;
        currentColor = room.settings.floorMaterial.color;
    }

    #region Getters
    public MazeCellEdge GetEdge (MazeDirection direction)
    {
        return edges[(int)direction];
    }

    public MazeCellEdge[] GetEdges()
    {
        return edges;
    }
    #endregion

    #region Setters
    public void SetEdge(MazeDirection direction, MazeCellEdge edge)
    {
        edges[(int)direction] = edge;
        initializedEdgeCount += 1;
    }

    public void SetMaterial(Material m)
    {
        floorRenderer.material = m;
    }

    public void SetMaterialColor(Color c)
    {
        //currentColor = c;
        floorRenderer.material.color = c;
    }
    #endregion

    public bool IsFullyInitialized
    {
        get
        {
            return initializedEdgeCount == MazeDirections.Count;
        }
    }

    public MazeDirection RandomUninitializedDirection
    {
        get
        {
            int skips = Random.Range(0, MazeDirections.Count - initializedEdgeCount) - 1;
            for (int i = 0; i < MazeDirections.Count; ++i)
            {
                if(edges[i] == null)
                {
                    if(skips <= 0)
                    {
                        return (MazeDirection)i;
                    }
                    skips -= 1;
                }
            }
            throw new System.InvalidOperationException("MazeCell has no uninitialized directions left.");
        }
    }

    public void OnPlayerEntered()
    {
        room.Show();
        for(int i = 0; i < edges.Length; ++i)
        {
            edges[i].OnPlayerEntered();
        }
    }

    public void OnPlayerExited()
    {
        room.Hide();
        for(int i = 0; i < edges.Length; ++i)
        {
            edges[i].OnPlayerExited();
        }
    }

    private void OnMouseEnter()
    {
        if (GameManager.Instance.gameStarted)
        {
            //GameManager.Instance.SetDestination(this);
            //TODO:Should probably set it to a different material, so it's easier to see on any color background
            floorRenderer.material.color = mouseOverColor;
        }
    }

    private void OnMouseDown()
    {
        if(GameManager.Instance.gameStarted)
        {
            bool rockThrow = false;
            if (GameManager.Instance.GetManhattanDistance(this, GameManager.Instance.GetPlayerCell()) <= 3)
            {
                floorRenderer.material.color = Color.black;
                rockThrow = GameManager.Instance.ThrowRock();
            }
            if(rockThrow && GameManager.Instance.GetManhattanDistance(this, GameManager.Instance.GetEnemyCell()) <= 5)
            {
                GameManager.Instance.SetDestination(this);
            }
        }
    }

    private void OnMouseExit()
    {
        if (GameManager.Instance.gameStarted)
        {
            //GameManager.Instance.ClearDestination();
            floorRenderer.material.color = currentColor;
        }
    }

    public void ResetMaterialColor()
    {
        floorRenderer.material.color = room.settings.floorMaterial.color;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

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
    public bool isMoving;

    private GameObject player;
    private FSMSystem fsm;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");    //can do this as long as instantiated after player
        isMoving = false;
    }

    //When ready, set location, generate patrol path, start FSM
    public void Activate(MazeCell start, List<MazeCell> doors)
    {
        currentCell = start;
        transform.localPosition = currentCell.transform.localPosition + Vector3.up / 2;

        this.doors = doors;
        this.doors.Sort((a, b) => GameManager.Instance.GetManhattanDistance(start, a).CompareTo(GameManager.Instance.GetManhattanDistance(start, b)));

        PathToPatrol(currentCell);

        MakeFSM();
    }

    private void FixedUpdate()
    {
        fsm.CurrentState.Reason(player, gameObject);
        fsm.CurrentState.Act(player, gameObject);
    }

    private void MakeFSM()
    {
        PatrolState patrol = new PatrolState(patrolPath);
        //patrol.AddTransition(Transition.Clue, StateID.Investigate);
        //Add transition to chase

        InvestigateState investigate = new InvestigateState();
        //add transition

        fsm = new FSMSystem();
        fsm.AddState(patrol);
        //fsm.AddState(investigate);
    }

    public void SetTransition(Transition t)
    {
        fsm.PerformTransition(t);
    }

    #region Movement and Orientation
    public void GoTo(MazeCell cell)
    {
        StartCoroutine(Move(cell));
    }

    public IEnumerator Move(MazeCell cell)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = cell.transform.localPosition;
        transform.LookAt(endPosition);  //TODO: should slowly rotate
        float t = 0;
        while(t < 1f)
        {
            t += Time.deltaTime * Speed;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }
        isMoving = false;
        currentCell = cell;
        yield return 0;
    }
    #endregion

    #region Pathing
    // Creates a single long patrol path that connects each door and loops back to start
    //TODO: May be better to build route in game rather than all at once to have more reasonable route
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

#region States
public class PatrolState : FSMState
{
    private List<MazeCell> patrolPath;
    private int currentIndex;
    
    public PatrolState(List<MazeCell> path)
    {
        patrolPath = path;
        currentIndex = 0;
        //stateID = StateID.Patrol;
    }

    public override void Reason(GameObject player, GameObject npc)
    {
        //throw new System.NotImplementedException();
    }

    public override void Act(GameObject player, GameObject npc)
    {
        // throw new System.NotImplementedException();
        Enemy enemyScript = npc.GetComponent<Enemy>();
        if (enemyScript == null)
            Debug.LogError("NPC has no enemy script!");

        if (!enemyScript.isMoving)
        {
            if (currentIndex >= patrolPath.Count)
                currentIndex = 0;
            enemyScript.GoTo(patrolPath[currentIndex]);
            currentIndex++;
        }
    }
}

public class InvestigateState : FSMState
{
    public override void Reason(GameObject player, GameObject npc)
    {
        throw new System.NotImplementedException();
    }

    public override void Act(GameObject player, GameObject npc)
    {
        throw new System.NotImplementedException();
    }
}

#endregion
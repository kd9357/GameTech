using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public float Speed = 3f;
    public Material pathMaterial;

    //[HideInInspector]
    //public List<MazeCell> investigationPath;

    private List<MazeCell> patrolPath;
    private List<MazeCell> doors;

    private MazeCell currentCell;
    private MazeCell investigateCell;
    private MazeCell cachedCell;

    public bool isMoving;
    public bool isInvestigating;
    public bool isPatrolling;
    public bool pathChanged;

    private GameObject player;
    private FSMSystem fsm;

    private MazeRoom room;
    private bool isHidden;
    private bool canMove = true;

    // Use this for initialization
    void Start()
    {
        isMoving = false;
        isPatrolling = true;
        isInvestigating = false;
        pathChanged = false;
        player = GameObject.FindWithTag("Player");//can do this as long as instantiated after player
    }

    // Update is called once per frame
    void Update()
    {
        if (isHidden)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    //When ready, set location, generate patrol path, start FSM
    public void Activate(MazeCell start, List<MazeCell> doors)
    {
        currentCell = start;
        transform.localPosition = currentCell.transform.localPosition;
        room = start.room;

        this.doors = doors;
        this.doors.Sort((a, b) => GameManager.Instance.GetManhattanDistance(start, a).CompareTo(GameManager.Instance.GetManhattanDistance(start, b)));

        PathToPatrol(currentCell);

        MakeFSM();
    }

    public void disableMovement()
    {
        canMove = false;
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            fsm.CurrentState.Reason(player, gameObject);
            fsm.CurrentState.Act(player, gameObject);
        }
    }

    private void MakeFSM()
    {
        PatrolState patrol = new PatrolState(patrolPath, this);
        patrol.AddTransition(Transition.Clue, StateID.Investigate);
        //Add transition to chase

        InvestigateState investigate = new InvestigateState(this);
        investigate.AddTransition(Transition.AllClear, StateID.ResumePatrol);
        //add transition to investigate again?
        //add transition to chase

        ResumePatrolState resume = new ResumePatrolState(this);
        resume.AddTransition(Transition.Return, StateID.Patrol);
        resume.AddTransition(Transition.Clue, StateID.Investigate);

        fsm = new FSMSystem();
        fsm.AddState(patrol);
        fsm.AddState(investigate);
        fsm.AddState(resume);
    }

    public void SetTransition(Transition t)
    {
        fsm.PerformTransition(t);
    }

    public MazeCell GetCurrentCell()
    {
        return currentCell;
    }

    public MazeCell GetInvestigateCell()
    {
        return investigateCell;
    }
    public MazeCell GetCachedCell()
    {
        return cachedCell;
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
        while (t < 1f)
        {
            t += Time.deltaTime * Speed;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }
        isMoving = false;
        currentCell = cell;
        room = currentCell.room;
        if (room != player.GetComponent<Player>().GetLocation().room)
        {
            isHidden = true;
        }
        else
        {
            isHidden = false;
        }
        yield return 0;
    }
    #endregion

    #region Rendering
    public void Hide()
    {
        foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }

    public void Show()
    {
        foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>())
            r.enabled = true;
    }
    #endregion

    #region Pathing
    // Creates a single long patrol path that connects each door and loops back to start
    //TODO: May be better to build route in game rather than all at once to have more reasonable route
    //TODO: May not need patrol route list on enemy, just let FSM handle it
    void PathToPatrol(MazeCell initial)
    {
        patrolPath = GameManager.Instance.PathFinding(initial, doors[0]);
        List<MazeCell> newPath;
        for (int i = 1; i < doors.Count; i++)
        {
            newPath = GameManager.Instance.PathFinding(doors[i - 1], doors[i]);
            for (int j = 1; j < newPath.Count; j++)
            {
                patrolPath.Add(newPath[j]);
            }
        }
        newPath = GameManager.Instance.PathFinding(doors[doors.Count - 1], initial);
        for (int i = 1; i < newPath.Count - 1; i++)
        {
            patrolPath.Add(newPath[i]);
        }
        SetPatrolPathColor(Color.red);
    }

    public void PathToInvestigate(MazeCell destination)
    {
        if (isInvestigating)
            pathChanged = true;
        if(isPatrolling)
        {
            isInvestigating = true;
            isPatrolling = false;
            cachedCell = currentCell;
        }
        investigateCell = destination;
    }

    //Debugging methods
    void SetPatrolPathColor(Color c)
    {
        foreach (MazeCell cell in patrolPath)
        {
            cell.SetMaterialColor(c);
        }
        patrolPath[0].SetMaterialColor(Color.black);
        patrolPath[patrolPath.Count - 1].SetMaterialColor(Color.black);
    }

    public void ClearPatrolPath()
    {
        foreach (MazeCell cell in patrolPath)
        {
            cell.ResetMaterialColor();
        }
        patrolPath.Clear();
    }

    #endregion
}

#region States

/*States to add:
 * 
 * Look at target state: when first investigate, pause for some seconds before going to investigate
 * Look around state: when reached cell to investigate, pause and look around for some seconds before going back to patrol
 * Chase: When player is seen, transition to this state (either game over or chase after player)
 * 
 */

//The enemy will walk towards the doors around its spawn point
public class PatrolState : FSMState
{
    private List<MazeCell> patrolPath;
    private int currentIndex;
    private Enemy enemy;

    public PatrolState(List<MazeCell> path, Enemy enemy)
    {
        patrolPath = path;
        currentIndex = 0;
        this.enemy = enemy;
        stateID = StateID.Patrol;
    }

    public override void Reason(GameObject player, GameObject npc)
    {
        if (!enemy.isMoving)
        {
            if (enemy.isInvestigating)
            {
                enemy.SetTransition(Transition.Clue);
            }
        }
    }

    public override void Act(GameObject player, GameObject npc)
    {
        if (!enemy.isMoving)
        {
            if (currentIndex >= patrolPath.Count)
                currentIndex = 0;
            enemy.GoTo(patrolPath[currentIndex]);
            currentIndex++;
        }
    }

    public override void DoBeforeEntering()
    {
        enemy.isInvestigating = false;
        enemy.isPatrolling = true;
    }
}

//The enemy will walk towards a specified cell
public class InvestigateState : FSMState
{
    private int currentIndex;
    private Enemy enemy;
    private List<MazeCell> path;

    public InvestigateState(Enemy enemy)
    {
        currentIndex = 0;
        this.enemy = enemy;
        stateID = StateID.Investigate;
    }

    public override void Reason(GameObject player, GameObject npc)
    {
        if (!enemy.isMoving)
        {
            if (enemy.pathChanged)
            {
                enemy.pathChanged = false;
                path = GameManager.Instance.PathFinding(enemy.GetCurrentCell(), enemy.GetInvestigateCell());
                currentIndex = 0;
            }
            else if(currentIndex >= path.Count)
            {
                enemy.SetTransition(Transition.AllClear);
            }
        }
    }

    public override void Act(GameObject player, GameObject npc)
    {
        if (!enemy.isMoving)
        {
            enemy.GoTo(path[currentIndex]);
            currentIndex++;
        }
    }

    public override void DoBeforeEntering()
    {
        enemy.isInvestigating = true;
        enemy.isPatrolling = false;
        currentIndex = 0;
        path = GameManager.Instance.PathFinding(enemy.GetCurrentCell(), enemy.GetInvestigateCell());
    }

}

//The enemy will return to its patrol route cell it left in investigate
public class ResumePatrolState : FSMState
{
    private int currentIndex;
    private Enemy enemy;
    private List<MazeCell> path;

    public ResumePatrolState(Enemy enemy)
    {
        this.enemy = enemy;
        currentIndex = 0;
        stateID = StateID.ResumePatrol;
    }

    public override void Reason(GameObject player, GameObject npc)
    {
        if (!enemy.isMoving)
        {
            if (enemy.pathChanged)
            {
                //Go to investigate
                enemy.SetTransition(Transition.Clue);
            }
            else if(currentIndex >= path.Count)
            {
                enemy.SetTransition(Transition.Return);
            }
        }
    }

    public override void Act(GameObject player, GameObject npc)
    {
        if (!enemy.isMoving)
        {
            enemy.GoTo(path[currentIndex]);
            currentIndex++;
        }
    }

    public override void DoBeforeEntering()
    {
        path = GameManager.Instance.PathFinding(enemy.GetCurrentCell(), enemy.GetCachedCell());
        currentIndex = 0;
    }

    public override void DoBeforeLeaving()
    {
        enemy.isInvestigating = false;
        enemy.isPatrolling = true;
    }
}

#endregion
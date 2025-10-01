//using System;
//using System.Drawing;
//using Unity.VisualScripting;
//using UnityEngine.UIElements;
//using static UnityEditor.PlayerSettings;
using System;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
//using static UnityEngine.Rendering.DebugUI;

public class PlayerController : MonoBehaviour
{
    public GameObject current;
    //public GameObject cam;
    public GameObject hover;
    public GameObject timeline;
    public GameObject player;
    public Unit activeUnit;
    public PlayerInput playerInput;

    [Range(0.1f, 100f)] // This adds a slider to the editor
    public float cameraSpeed;
    //public float cameraRotation;
    //public float cameraRotationSpeed;
    public Vector2 lookVector;
    public Vector2 moveVector;
    public Unit unit;
    public bool targeting = false;
    public bool inCombat = false;
    public GameObject combatArea;
    public Vector3 lastPosition;
    public GameObject[] enemyList;
    public GameObject[] playerUnits;
    public Transform[] playerPositions;
    public Transform[] enemyPositions;
    private Vector3 hitPoint;
    private Vector3 movement;
    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction selectAction;
    private InputAction interactAction;
    //private InputAction rotateAction;
    //private NavMeshPath path;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //canvas = (Canvas)FindFirstObjectByType(typeof(Canvas));
        hover = null;
        //cam = GameObject.Find("CameraTarget");
        player = GameObject.Find("Player");
        combatArea = GameObject.Find("CombatArea");
        playerPositions = GameObject.Find("PlayerSideSlots").GetComponentsInChildren<Transform>();
        enemyPositions = GameObject.Find("EnemySideSlots").GetComponentsInChildren<Transform>();
        timeline = GameObject.Find("Timeline");
        playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions.FindAction("Cursor");
        moveAction = playerInput.actions.FindAction("Move");
        //rotateAction = playerInput.actions.FindAction("Rotate");
        selectAction = playerInput.actions.FindAction("Attack");
        interactAction = playerInput.actions.FindAction("Interact");
        movement = Vector3.zero;

        //print("Not Walkable: " + NavMesh.GetAreaFromName("Not Walkable"));
        //print("Walkable: " + NavMesh.GetAreaFromName("Walkable"));
        //print(GameObject.Find("PauseButton").GetComponentAtIndex(3));
    }

    public GameObject FindClosestTagged(string tag, Vector3 position)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float distance = Mathf.Infinity;

        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

    // Update is called once per frame
    void Update()
    {
        lookVector = lookAction.ReadValue<Vector2>();
        moveVector = moveAction.ReadValue<Vector2>();
        //cameraRotation = rotateAction.ReadValue<float>();
        //cam.transform.RotateAround(cam.transform.position, Vector3.up, cameraRotation * cameraRotationSpeed * Time.fixedDeltaTime);
        movement = new Vector3(moveVector.x, moveVector.y, 0);
        
        if (inCombat == false)
        {
            player.transform.Translate(cameraSpeed * Time.deltaTime * movement);
        }
        else
        {
            EliminateInvalidEvents();
        }

        TimelineEvent[] events = timeline.GetComponents<TimelineEvent>();

        foreach (TimelineEvent e in events)
        {
            if (e.timelinePosition == 0)
            {
                activeUnit = e.owner;
            }
        }

        if (unit != null && activeUnit == unit && inCombat && unit.playerUI.activeSelf == false)
        {
            unit.playerUI.SetActive(true);
        }

        if (selectAction.WasPerformedThisFrame())// && !MouseOverUI())
        {
            if (hover != null)
            {
                print(hover.name);
                if (targeting == true && hover.GetComponent<Unit>().target.activeSelf == true)
                {
                    if (unit.currentAction.cleave == true)
                    {
                        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
                        Unit[] targets = new Unit[units.Length];
                        int i = 0;

                        foreach (GameObject unit in units)
                        {
                            Unit un = unit.GetComponent<Unit>();
                            if (un.target.activeSelf == true)
                            {
                                targets[i] = un;
                                i++;
                            }
                        }
                        unit.currentAction.Perform(targets);
                    }
                    else
                    {
                        Unit[] targets = new Unit[1];
                        targets[0] = hover.GetComponent<Unit>();
                        unit.currentAction.Perform(targets);
                    }
                    //unit.playerUI.SetActive(false);
                    CancelAction();
                    TimelineEventEnd();
                }
            }
            else if (activeUnit == unit)
            {
                //
            }
            else
            {
                //
            }
        }

        /*if (interactAction.WasPerformedThisFrame() && targeting == true)
        {
            CancelAction();
        }*/
    }

    void FixedUpdate()
    {
        //Create a ray from the Mouse click position
        Ray ray = Camera.main.ScreenPointToRay(lookVector);//Input.mousePosition);
        RaycastHit rayHit;
        LayerMask layerMask = LayerMask.GetMask("Default");

        if (Physics.Raycast(ray, out rayHit, Mathf.Infinity, layerMask))
        {
            hitPoint = rayHit.point;
            if (rayHit.collider.gameObject.TryGetComponent<Unit>(out Unit u))
            {
                hover = rayHit.collider.gameObject;
                if (unit != null && unit.currentAction != null)
                {
                    if (unit.currentAction.cleave == true)
                    {
                        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

                        foreach (GameObject uni in units)
                        {
                            Unit un = uni.GetComponent<Unit>();
                            if (un.target.activeSelf == true)
                            {
                                un.highlight.SetActive(true);
                            }
                        }
                    }
                }
                u.highlight.SetActive(true);
            }
            else
            {
                if (hover)
                {
                    GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

                    foreach (GameObject uni in units)
                    {
                        Unit un = uni.GetComponent<Unit>();
                        if (un.highlight.activeSelf == true)
                        {
                            un.highlight.SetActive(false);
                        }
                    }
                }
                hover = null;
                current.transform.position = hitPoint;
            }
        }
    }

    public void TimelineEventEnd()
    {
        if (unit != null)
        {
            unit.playerUI.SetActive(false);
        }

        if (timeline.TryGetComponent<TimelineEvent>(out TimelineEvent t))
        {
            TimelineEvent[] events = timeline.GetComponents<TimelineEvent>();
            TimelineEvent next = null;
            float time = Mathf.Infinity;

            foreach (TimelineEvent e in events)
            {
                float curTime = e.timelinePosition;
                if (curTime < time && (curTime != 0 || e.owner != activeUnit))
                {
                    next = e;
                    time = curTime;
                }
            }
            
            /*
            float[] values = new float[events.Length];
            for (int i = 0; i < events.Length; i++)
            {
                values[i] = events[i].timelinePosition;
            }
            Array.Sort(values);
            print("Values: " + values[0] + " - " + values[values.Length - 1]);
            
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] == values[i - 1])
                {
                    duplicateValue = values[i];
                }
            }
            */

            if (next != null)
            {
                if (next.owner.TryGetComponent<Unit>(out Unit u))
                {
                    if (u.faction == "Player")
                    {
                        if (u.downed)
                        {
                            Destroy(next);
                            unit = null;
                        }
                        else
                        {
                            unit = u;
                            unit.playerUI.SetActive(true);
                        }
                    }
                    else
                    {
                        unit = null;
                    }
                }
            }
            else
            {
                print("Next event timestamp: " + time);
                print("No next event");
                RoundStart();
                return;
            }

            foreach (TimelineEvent e in events)
            {
                if (e.timelinePosition == 0 && e.owner == activeUnit)
                {
                    Destroy(timeline.GetComponentAtIndex(e.GetComponentIndex()));
                }
            }

            foreach (TimelineEvent e in events)
            {
                if (e.timelinePosition == (int)time)
                {
                    activeUnit = e.owner;
                    print("Active unit: " + activeUnit + " of faction " + activeUnit.faction);
                }
                if ((int)time > 0)
                {
                    e.timelinePosition -= (int)time;
                }
            }
        }
        else
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

            int enemies = 0;

            foreach (GameObject unit in units)
            {
                Unit un = unit.GetComponent<Unit>();
                if (un.faction == "Enemy")
                {
                    enemies++;
                }
            }

            if (enemies > 0)
            {
                // Begin the next round
                RoundStart();
            }
            else
            {
                // Check for reinforcements / Next wave?
            }
        }
    }

    public void RoundStart()
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

        foreach (GameObject unit in units)
        {
            Unit un = unit.GetComponent<Unit>();
            if (!un.downed)
            {
                un.CreateTimelineEvent();
            }
        }

        FindDuplicateValues();
        TimelineEventEnd();
    }

    public void CancelAction()
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");

        foreach (GameObject unit in units)
        {
            Unit un = unit.GetComponent<Unit>();
            un.target.SetActive(false);
        }

        if (unit != null)
        {
            unit.currentAction = null;
        }
        
        targeting = false;
    }

    public void FindDuplicateValues()
    {
        TimelineEvent[] events = timeline.GetComponents<TimelineEvent>();
        foreach (TimelineEvent e in events)
        {
            foreach (TimelineEvent v in events)
            {
                if (e.timelinePosition == v.timelinePosition && e.GetComponentIndex() != v.GetComponentIndex())
                {
                    EliminateDuplicateValues(e.GetComponentIndex());
                    return;
                }
            }
        }
        print("No duplicate values found.");
    }

    public void EliminateDuplicateValues(int index)
    {
        TimelineEvent[] events = timeline.GetComponents<TimelineEvent>();
        foreach (TimelineEvent e in events)
        {
            if (e.GetComponentIndex() != index)
            {
                e.timelinePosition++;
            }
        }
        FindDuplicateValues();
    }

    public void EliminateInvalidEvents()
    {
        float time = Mathf.Infinity;
        if (timeline.TryGetComponent<TimelineEvent>(out TimelineEvent t))
        {
            TimelineEvent[] events = timeline.GetComponents<TimelineEvent>();

            foreach (TimelineEvent e in events)
            {
                float curTime = e.timelinePosition;
                if (curTime < time)
                {
                    time = curTime;
                }
            }
        }
        else
        {
            TimelineEventEnd();
        }
        if ((int)time != 0)
        {
            print((int)time);
            TimelineEventEnd();
        }
    }

    public GameObject[] CreateRandomEnemies()
    {
        int numberOfEnemies = 3;
        GameObject[] enemies = new GameObject[numberOfEnemies];

        for (int i = 0; i < numberOfEnemies; i++)
        {
            enemies[i] = enemyList[UnityEngine.Random.Range(0, enemyList.Length)];
        }
        return enemies;
    }

    public void BeginCombat(GameObject[] enemies)
    {
        int numberOfEnemies = enemies.Length;
        int numberOfPlayerUnits = playerUnits.Length;

        if (timeline.TryGetComponent<TimelineEvent>(out TimelineEvent t))
        {
            TimelineEvent[] events = timeline.GetComponents<TimelineEvent>();
            foreach (TimelineEvent e in events)
            {
                Destroy(timeline.GetComponentAtIndex(e.GetComponentIndex()));
            }
        }

        for (int i = 0; i < numberOfEnemies; i++)
        {
            GameObject enemy = Instantiate(enemies[i], enemyPositions[i + 1].position, Quaternion.identity);
        }

        for (int i = 0; i < numberOfPlayerUnits; i++)
        {
            GameObject playerUnit = Instantiate(playerUnits[i], playerPositions[i + 1].position, Quaternion.identity);
            playerUnit.GetComponent<Unit>().faction = "Player";
        }

        inCombat = true;
        lastPosition = player.transform.position;
        player.transform.position = combatArea.transform.position + Vector3.forward;

        RoundStart();
    }

    public void EndCombat()
    {
        player.transform.position = lastPosition;
        inCombat = false;
        unit = null;
        activeUnit = null;

        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        TimelineEvent[] events = timeline.GetComponents<TimelineEvent>();

        foreach (GameObject unit in units)
        {
            Destroy(unit);
        }

        foreach (TimelineEvent e in events)
        {
            Destroy(timeline.GetComponentAtIndex(e.GetComponentIndex()));
        }
    }

    public void RandomEncounter()
    {
        BeginCombat(CreateRandomEnemies());
    }

    private bool MouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
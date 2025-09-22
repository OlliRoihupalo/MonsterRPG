//using System;
//using System.Drawing;
//using Unity.VisualScripting;
//using UnityEngine.UIElements;
//using static UnityEditor.PlayerSettings;
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
        player = GameObject.FindWithTag("Player");
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
        
        if (player != null)
        {
            player.transform.Translate(cameraSpeed * Time.deltaTime * movement);
        }

        TimelineEvent[] events = timeline.GetComponents<TimelineEvent>();

        foreach (TimelineEvent e in events)
        {
            if (e.timelinePosition == 0)
            {
                activeUnit = e.owner;
            }
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

                    /*
                    hover.GetComponent<Unit>().TakeDamage(20);
                    */
                    CancelAction();
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
        //NavMeshHit hit;
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
                /*
                
                */
                u.highlight.SetActive(true);
            }
            else
            {
                if (hover)
                {
                    //hover.GetComponent<Unit>().highlight.SetActive(false);
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
            print(time);

            if (next != null)
            {
                if (next.owner.TryGetComponent<Unit>(out Unit u))
                {
                    if (u.faction == "Player")
                    {
                        unit = u;
                        unit.playerUI.SetActive(true);
                    }
                    else
                    {
                        unit = null;
                    }
                }
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
                    print("Active unit: " + activeUnit);
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

        unit.currentAction = null;
        targeting = false;
    }

    private bool MouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
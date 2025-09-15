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
    public float cameraSpeed;
    //public float cameraRotation;
    //public float cameraRotationSpeed;
    public Vector2 lookVector;
    public Vector2 moveVector;
    public Unit unit;
    private Vector3 hitPoint;
    private Vector3 movement;
    private InputAction lookAction;
    private InputAction moveAction;
    //private InputAction rotateAction;
    private InputAction selectAction;
    //private NavMeshPath path;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //canvas = (Canvas)FindFirstObjectByType(typeof(Canvas));
        hover = null;
        //cam = GameObject.Find("CameraTarget");
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        timeline = GameObject.Find("Timeline");
        playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions.FindAction("Cursor");
        moveAction = playerInput.actions.FindAction("Move");
        //rotateAction = playerInput.actions.FindAction("Rotate");
        selectAction = playerInput.actions.FindAction("Attack");
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
        
        player.transform.Translate(cameraSpeed * Time.deltaTime * movement);

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
                if (hover.GetComponent<Unit>().faction == "Player")
                {
                    unit = hover.GetComponent<Unit>();
                }
                else
                {
                    print(hover.name);
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
            hover = rayHit.collider.gameObject;
            current.transform.position = hitPoint;
            /*if (rayHit.collider.gameObject.TryGetComponent<Unit>(out Unit unit))
            {
                hover = rayHit.collider.gameObject;
            }
            else
            {
                hover = null;
                current.transform.position = hitPoint;
            }*/
        }
    }

    public void TimelineEventEnd()
    {
        TimelineEvent[] events = timeline.GetComponents<TimelineEvent>();
        TimelineEvent next = null;
        float time = Mathf.Infinity;

        foreach (TimelineEvent e in events)
        {
            float curTime = e.timelinePosition;
            if (curTime < time && curTime != 0)
            {
                next = e;
                time = curTime;
            }
        }

        if (next.owner.TryGetComponent<Unit>(out Unit u))
        {
            if (u.faction == "Player")
            {
                unit = u;
            }
        }

        foreach (TimelineEvent e in events)
        {
            if (e.timelinePosition == 0)
            {
                Destroy(timeline.GetComponentAtIndex(e.GetComponentIndex()));
                if (timeline.TryGetComponent<TimelineEvent>(out TimelineEvent t))
                {
                    //
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
                        foreach (GameObject unit in units)
                        {
                            Unit un = unit.GetComponent<Unit>();
                            un.CreateTimelineEvent();
                        }
                    }
                    else
                    {
                        // Check for reinforcements / Next wave?
                    }
                }
            }
            else
            {
                if (e.timelinePosition == (int)time)
                {
                    activeUnit = e.owner;
                }
                e.timelinePosition -= (int)time;
            }
        }
    }

    private bool MouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
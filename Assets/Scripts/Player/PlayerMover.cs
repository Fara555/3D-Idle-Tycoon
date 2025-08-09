using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMover : MonoBehaviour
{
    [Header("Ground Layer")]
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Movement")]
    [SerializeField] private float stoppingDistance = 0.2f;
    [SerializeField] private bool rotateTowardsVelocity = true;
    [SerializeField] private float rotateSpeed = 720f;

    [Header("Click Filtering")]
    [SerializeField] private float clickMaxMovePx = 8f;

    [Header("Animation")]
    [SerializeField] private Animator animator; 
    [SerializeField] private string speedParam = "Speed"; 

    private NavMeshAgent _agent;
    private Camera _cam;
    private Vector3 _downPos;
    private bool _pressed;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _cam = Camera.main;

        _agent.stoppingDistance = stoppingDistance;
        _agent.autoBraking = false;
        _agent.updateRotation = !rotateTowardsVelocity;
        _agent.updateUpAxis = true;

        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        HandleInput();
        HandleRotation();
        HandleAnimation();
        
        if (_agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
            _agent.ResetPath();
    }

    void HandleInput()
    {
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
        {
            _pressed = false;
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            _pressed = true;
            _downPos = Input.mousePosition;
        }

        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && _pressed)
        {
            _pressed = false;
            Vector3 upPos = Input.mousePosition;
            if ((upPos - _downPos).sqrMagnitude <= clickMaxMovePx * clickMaxMovePx)
            {
                TrySetDestinationByRay(upPos);
            }
        }
    }

    void TrySetDestinationByRay(Vector3 screenPos)
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        Ray ray = _cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                _agent.SetDestination(navHit.position);
            }
        }
    }

    void HandleRotation()
    {
        if (!rotateTowardsVelocity) return;

        Vector3 vel = _agent.steeringTarget - transform.position;
        vel.y = 0f;

        if (vel.sqrMagnitude > 0.0001f) 
        {
            Quaternion targetRot = Quaternion.LookRotation(vel);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }

    void HandleAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(speedParam))
        {
            float speed = _agent.velocity.magnitude;
            animator.SetFloat(speedParam, speed);
        }
    }

    public bool MoveTo(Vector3 worldPos, float projectRadius = 2f)
    {
        if (NavMesh.SamplePosition(worldPos, out NavMeshHit navHit, projectRadius, NavMesh.AllAreas))
        {
            _agent.SetDestination(navHit.position);
            return true;
        }
        return false;
    }
}

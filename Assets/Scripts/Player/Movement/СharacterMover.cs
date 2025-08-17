using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class СharacterMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float stoppingDistance = 0.2f;
    [SerializeField] private bool rotateTowardsVelocity = true;
    [SerializeField] private float rotateSpeed = 720f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParam = "Speed";

    private NavMeshAgent _agent;
    private Action<Vector3> OnMoveCommand;


    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.stoppingDistance = stoppingDistance;
        _agent.autoBraking = false;
        _agent.updateRotation = !rotateTowardsVelocity;
        _agent.updateUpAxis = true;

        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        HandleRotation();
        HandleAnimation();

        if (_agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
            _agent.ResetPath();
    }

    private void HandleRotation()
    {
        if (!rotateTowardsVelocity) return;
        Vector3 vel = _agent.steeringTarget - transform.position; vel.y = 0f;
        if (vel.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(vel);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }

    private void HandleAnimation()
    {
        if (!animator || string.IsNullOrEmpty(speedParam)) return;
        animator.SetFloat(speedParam, _agent.velocity.magnitude);
    }
    
    public bool MoveTo(Vector3 worldPos, float projectRadius = 2f)
    {
        if (NavMesh.SamplePosition(worldPos, out NavMeshHit navHit, projectRadius, NavMesh.AllAreas))
        {
            _agent.SetDestination(navHit.position);
            OnMoveCommand?.Invoke(navHit.position);
            _agent.isStopped = false; 
            return true;
        }
        return false;
    }
}

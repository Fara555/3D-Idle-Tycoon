using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class NavPathRenderer : MonoBehaviour
{
    [Header("Refresh")] 
    [SerializeField] private float refreshInterval = 0.06f;

    [Header("Ground")] 
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float offsetHeight = 0.03f;

    [Header("Arrows")]
    [SerializeField] private float lineWidth = 0.08f;

    [SerializeField] private NavMeshAgent _agent;
    private LineRenderer _lr;
    private float _timer;

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.useWorldSpace = true;
        _lr.textureMode = LineTextureMode.Tile;      
        _lr.alignment   = LineAlignment.TransformZ; 
        _lr.widthMultiplier = lineWidth;
        _lr.positionCount   = 0;

        _lr.textureScale = new Vector2(1f, 1f);

        if (_lr.material != null)
            _lr.material = new Material(_lr.material);
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= refreshInterval)
        {
            _timer = 0f;
            DrawFromAgentPath();
        }
    }

    void DrawFromAgentPath()
    {
        if (_agent == null || !_agent.enabled || _agent.pathPending || !_agent.hasPath)
        {
            Hide();
            return;
        }

        var corners = _agent.path.corners;
        if (corners == null || corners.Length < 2) { Hide(); return; }

        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = GroundProject(corners[i]);
            corners[i].y += offsetHeight;
        }

        _lr.positionCount = corners.Length;
        _lr.SetPositions(corners);

        if (!_lr.enabled) _lr.enabled = true;
    }

    Vector3 GroundProject(Vector3 origin)
    {
        if (Physics.Raycast(origin + Vector3.up * 100f, Vector3.down, out var hit, 1000f, groundLayers))
            return hit.point;
        return origin;
    }

    void Hide()
    {
        if (_lr.enabled) _lr.enabled = false;
        _lr.positionCount = 0;
    }
}

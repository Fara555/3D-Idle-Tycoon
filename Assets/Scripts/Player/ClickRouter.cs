using UnityEngine;
using UnityEngine.EventSystems;

public class ClickRouter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private СharacterMover сharacter;                 
    [SerializeField] private LayerMask interactableMask;         
    [SerializeField] private LayerMask groundMask;              
    [Header("Click Filter")]
    [SerializeField] private float clickMaxMovePx = 8f;

    private Vector3 _downPos;
    private bool _pressed;
    private IInteractable _hover;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        UpdateHover();

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;
            _pressed = true;
            _downPos = Input.mousePosition;
        }

        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && _pressed)
        {
            _pressed = false;
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

            Vector3 up = Input.mousePosition;
            if ((up - _downPos).sqrMagnitude > clickMaxMovePx * clickMaxMovePx) return; 

            if (TryRaycastInteract(out IInteractable interact))
            {
                if (Input.GetMouseButtonUp(1)) interact.OnRightClick();
                else                            interact.OnLeftClick();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (TryRaycastGround(up, out Vector3 hitPoint))
                {
                    сharacter.MoveTo(hitPoint);
                    VillagerAssignmentManager.Instance.CancelAssignment(); 
                }
            }
        }
    }

    void UpdateHover()
    {
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
        {
            SetHover(null);
            return;
        }

        if (TryRaycastInteract(out IInteractable interact))
            SetHover(interact);
        else
            SetHover(null);
    }

    void SetHover(IInteractable target)
    {
        if (_hover == target) return;
        if (_hover != null) _hover.OnHover(false);
        _hover = target;
        if (_hover != null) _hover.OnHover(true);
    }

    bool TryRaycastInteract(out IInteractable interact)
    {
        interact = null;
        if (!cam) cam = Camera.main;
        if (!cam) return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, interactableMask, QueryTriggerInteraction.Collide))
        {
            interact = hit.collider.GetComponentInParent<IInteractable>();
            return interact != null;
        }
        return false;
    }

    bool TryRaycastGround(Vector3 screenPos, out Vector3 point)
    {
        point = default;
        if (!cam) cam = Camera.main;
        if (!cam) return false;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask, QueryTriggerInteraction.Ignore))
        {
            point = hit.point;
            return true;
        }
        return false;
    }
}

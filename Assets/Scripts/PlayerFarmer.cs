using UnityEngine;
using TMPro;

public class PlayerFarmer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text promptText;

    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 2.0f;
    [SerializeField] private float feedPickupRadius = 1.0f;

    [Header("Mobile Input")]
    [SerializeField] private bool useMobileInput = true;

    private Inventory inventory;
    private IInteractable nearbyInteractable;
    private Vector2 mobileMoveInput;

    [HideInInspector]
    public bool isMobileChopping = false;

    public Inventory Inventory => inventory;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();

        if (inventory == null)
        {
            Debug.LogError("PlayerFarmer: Inventory component is missing on this GameObject.");
        }
    }

    private void Start()
    {
        HidePrompt();
    }

    private void Update()
    {
        SendMovementInputToStarterAssets();
        FindNearestInteractable();
        HandleInteractionPrompt();
        CheckFeedBagPickup();
    }

    private void SendMovementInputToStarterAssets()
    {
        Vector2 moveInput = mobileMoveInput;

        // Keyboard controls
        if (!useMobileInput || HasKeyboardMovementInput())
        {
            moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );
        }

        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }

        SendMessage("MoveInput", moveInput, SendMessageOptions.DontRequireReceiver);
    }

    private bool HasKeyboardMovementInput()
    {
        return Input.GetKey(KeyCode.W)
            || Input.GetKey(KeyCode.A)
            || Input.GetKey(KeyCode.S)
            || Input.GetKey(KeyCode.D)
            || Input.GetKey(KeyCode.UpArrow)
            || Input.GetKey(KeyCode.DownArrow)
            || Input.GetKey(KeyCode.LeftArrow)
            || Input.GetKey(KeyCode.RightArrow);
    }

    private void HandleInteractionPrompt()
    {
        if (nearbyInteractable == null)
        {
            HidePrompt();
            return;
        }

        ShowPrompt(nearbyInteractable.GetPromptText());

        // Desktop interaction key
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractWithNearbyObject();
        }
    }

    private void FindNearestInteractable()
    {
        Collider[] nearby = Physics.OverlapSphere(
            transform.position,
            interactionRadius,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide
        );

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider col in nearby)
        {
            if (col.transform.root == transform.root)
            {
                continue;
            }

            IInteractable interactable = col.GetComponentInParent<IInteractable>();

            if (interactable == null)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, col.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        nearbyInteractable = closest;
    }

    private void CheckFeedBagPickup()
    {
        if (inventory == null)
        {
            return;
        }

        Collider[] nearby = Physics.OverlapSphere(
            transform.position,
            feedPickupRadius
        );

        foreach (Collider col in nearby)
        {
            if (!col.CompareTag("FeedBag"))
            {
                continue;
            }

            inventory.AddFeedBag();
            Destroy(col.gameObject);
        }
    }

    private void InteractWithNearbyObject()
    {
        if (nearbyInteractable != null)
        {
            nearbyInteractable.Interact(this);
        }
    }

    // ===== MOBILE INPUT =====

    public void MobileMove(Vector2 input)
    {
        mobileMoveInput = input;
    }

    public void MobileMoveStop()
    {
        mobileMoveInput = Vector2.zero;
    }

    public void MobileInteract()
    {
        InteractWithNearbyObject();
    }

    public void MobileChop(bool pressed)
    {
        isMobileChopping = pressed;
    }

    public void MobileChopBegin()
    {
        isMobileChopping = true;
    }

    public void MobileChopEnd()
    {
        isMobileChopping = false;
    }

    // ===== UI =====

    private void ShowPrompt(string message)
    {
        if (promptText == null)
        {
            return;
        }

        promptText.gameObject.SetActive(true);
        promptText.text = message;
    }

    private void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    public void ClearNearbyInteractable()
    {
        nearbyInteractable = null;
        HidePrompt();
    }

    // ===== DEBUG =====

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, feedPickupRadius);
    }
}
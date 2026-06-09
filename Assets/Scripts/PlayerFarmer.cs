// PlayerFarmer.cs
// ─────────────────────────────────────────────────────────────────────────────
// Attached to the PlayerArmature. Handles interaction input and
// keeps track of which IInteractable the player is currently near.
// Movement is handled by the Starter Assets controller — we only
// add the farming interaction layer here.
// ─────────────────────────────────────────────────────────────────────────────

using UnityEngine;
using TMPro;

public class PlayerFarmer : MonoBehaviour
{
    // ── Inspector References ─────────────────────────────────────────────
    // promptText is the single TMP_Text you created in the Canvas.
    // There is no separate "panel" — we show/hide the text object directly.
    [SerializeField] private TMP_Text promptText;

    // interactionRadius controls how close the player must be to trigger a prompt.
    // Tune this in the Inspector — 2.0 is a good starting value.
    [SerializeField] private float interactionRadius = 2.0f;

    // ── Cached Components ────────────────────────────────────────────────
    private Inventory inventory;

    // ── State ────────────────────────────────────────────────────────────
    private IInteractable nearbyInteractable;

    // Mobile chop hold state — read by Tree.cs each frame
    [HideInInspector] public bool isMobileChopping = false;

    // ── Property ─────────────────────────────────────────────────────────
    public Inventory Inventory => inventory;

    // ────────────────────────────────────────────────────────────────────
    // Awake: cache the Inventory component on this same GameObject
    // ────────────────────────────────────────────────────────────────────
    void Awake()
    {
        inventory = GetComponent<Inventory>();
        if (inventory == null)
            Debug.LogError("PlayerFarmer: Inventory component missing from this GameObject!");
    }

    // ────────────────────────────────────────────────────────────────────
    // Start: hide the prompt text at game start
    // ────────────────────────────────────────────────────────────────────
    void Start()
    {
        HidePrompt();
    }

    // ────────────────────────────────────────────────────────────────────
    // Update: every frame, use Physics.OverlapSphere to find the closest
    // interactable object within interactionRadius.
    //
    // Why OverlapSphere instead of OnTriggerEnter?
    // The Starter Assets PlayerArmature uses a CharacterController, which
    // has unreliable trigger behaviour. OverlapSphere is a direct physics
    // query that works correctly regardless of how the player is set up.
    // No special collider setup is needed on the player.
    // ────────────────────────────────────────────────────────────────────
    void Update()
    {
        FindNearestInteractable();

        if (nearbyInteractable != null)
        {
            ShowPrompt(nearbyInteractable.GetPromptText());

            // Keyboard: press E once to interact
            if (Input.GetKeyDown(KeyCode.E))
                nearbyInteractable.Interact(this);
        }
        else
        {
            HidePrompt();
        }

        // FeedBag pickup: check separately each frame
        CheckFeedBagPickup();
    }

    // ────────────────────────────────────────────────────────────────────
    // FindNearestInteractable: scan all colliders within interactionRadius
    // and set nearbyInteractable to the closest IInteractable found.
    // ────────────────────────────────────────────────────────────────────
    private void FindNearestInteractable()
    {
        // QueryTriggerInteraction.Collide means we detect both trigger
        // and non-trigger colliders — works with any Asset Store prefab
        Collider[] nearby = Physics.OverlapSphere(
            transform.position,
            interactionRadius,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide
        );

        IInteractable closest  = null;
        float         closestD = float.MaxValue;

        foreach (Collider col in nearby)
        {
            // Skip the player's own colliders
            if (col.transform.root == transform.root) continue;

            IInteractable interactable = col.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < closestD)
            {
                closestD = dist;
                closest  = interactable;
            }
        }

        nearbyInteractable = closest;
    }

    // ────────────────────────────────────────────────────────────────────
    // CheckFeedBagPickup: auto-collect any FeedBag the player walks over
    // ────────────────────────────────────────────────────────────────────
    private void CheckFeedBagPickup()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, 1.0f);
        foreach (Collider col in nearby)
        {
            if (col.CompareTag("FeedBag"))
            {
                inventory.AddFeedBag();
                Destroy(col.gameObject);
            }
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // Mobile button methods — called by on-screen UI buttons
    // ────────────────────────────────────────────────────────────────────
    public void MobileInteract()
    {
        if (nearbyInteractable != null)
            nearbyInteractable.Interact(this);
    }

    public void MobileChopBegin() { isMobileChopping = true;  }
    public void MobileChopEnd()   { isMobileChopping = false; }

    // ────────────────────────────────────────────────────────────────────
    // Prompt helpers
    // ────────────────────────────────────────────────────────────────────
    private void ShowPrompt(string message)
    {
        if (promptText == null) return;
        promptText.gameObject.SetActive(true);
        promptText.text = message;
    }

    private void HidePrompt()
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    // Called by Tree.cs before it destroys itself
    public void ClearNearbyInteractable()
    {
        nearbyInteractable = null;
        HidePrompt();
    }
}
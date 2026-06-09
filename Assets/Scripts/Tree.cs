// Tree.cs
// ─────────────────────────────────────────────────────────────────────────────
// The player holds E to chop a tree. A UI Slider fills as chopping progresses.
// When the progress reaches 1.0, the tree falls and is destroyed.
//
// Key concept: Time.deltaTime makes chopping speed frame-rate independent.
// Without it, the chop speed would vary wildly between fast and slow machines.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Tree : MonoBehaviour, IInteractable
{
    // ── Inspector Settings ───────────────────────────────────────────────
    [SerializeField] private float  chopDuration   = 4f;  // seconds to fully chop
    [SerializeField] private int    woodValue      = 20;  // score for chopping this tree
    [SerializeField] private float  fallDuration   = 1.2f;// seconds the tree takes to "fall"
    [SerializeField] private Slider chopProgressBar;    // the UI Slider — drag in from Canvas

    // ── State ────────────────────────────────────────────────────────────
    private float chopProgress    = 0f;    // 0.0 = untouched, 1.0 = fully chopped
    private bool  isBeingChopped  = false; // is the player currently holding E?
    private bool  isFalling       = false; // prevent double-chop once falling
    private PlayerFarmer currentPlayer;    // the player that started chopping

    // ────────────────────────────────────────────────────────────────────
    // Start: initialise the progress bar (it should be hidden at game start)
    // ────────────────────────────────────────────────────────────────────
    void Start()
    {
        if (chopProgressBar != null)
        {
            chopProgressBar.value = 0f;
            chopProgressBar.gameObject.SetActive(false);
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // Update: advance chop progress while E is held (keyboard) OR while
    // the mobile chop button is held (isMobileChopping on PlayerFarmer).
    //
    // chopProgress += Time.deltaTime / chopDuration
    //   → Time.deltaTime is the time since the last frame (e.g. 0.016s at 60fps)
    //   → Dividing by chopDuration normalises it: 1 second of holding = 1/chopDuration
    //   → After chopDuration seconds of holding, chopProgress reaches 1.0
    // ────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (isFalling) return; // already done — stop processing

        // Accept input from keyboard (PC) OR mobile button flag
        bool chopInputHeld = Input.GetKey(KeyCode.E)
                          || (currentPlayer != null && currentPlayer.isMobileChopping);

        if (isBeingChopped)
        {
            if (chopInputHeld)
            {
                // Advance progress (frame-rate independent)
                chopProgress += Time.deltaTime / chopDuration;
                chopProgress = Mathf.Clamp01(chopProgress); // keep between 0 and 1

                // Drive the UI Slider
                if (chopProgressBar != null)
                    chopProgressBar.value = chopProgress;

                // Trigger the fall when fully chopped
                if (chopProgress >= 1f)
                    StartCoroutine(FallAndDestroy());
            }
            else
            {
                // Player released — pause chopping but keep progress
                isBeingChopped = false;
                if (chopProgressBar != null)
                    chopProgressBar.gameObject.SetActive(false);
            }
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // IInteractable implementation
    // ────────────────────────────────────────────────────────────────────

    public string GetPromptText()
    {
        if (isFalling) return "Timber!";
        if (chopProgress > 0f)
            return $"Hold E to continue chopping ({(int)(chopProgress * 100)}%)";
        return "Hold E to Chop Tree";
    }

    public void Interact(PlayerFarmer player)
    {
        if (isFalling) return;

        // Pressing E starts (or resumes) chopping
        // Update() will then check GetKey(E) each frame
        isBeingChopped = true;
        currentPlayer  = player;

        if (chopProgressBar != null)
            chopProgressBar.gameObject.SetActive(true);

        Debug.Log($"{name}: Chopping started ({(int)(chopProgress * 100)}% complete)");
    }

    // ────────────────────────────────────────────────────────────────────
    // FallAndDestroy: tree "falls" (tilts), then is destroyed.
    // We use a coroutine to wait for the visual effect before Destroy().
    // ────────────────────────────────────────────────────────────────────
    private IEnumerator FallAndDestroy()
    {
        isFalling = true;
        isBeingChopped = false;

        // Hide the progress bar
        if (chopProgressBar != null)
            chopProgressBar.gameObject.SetActive(false);

        // Award the player their wood score
        if (currentPlayer != null)
            currentPlayer.Inventory.AddWood(woodValue);

        Debug.Log($"{name}: Timber! Falling over {fallDuration} seconds.");

        // Animate the tree tilting over using rotation over time
        float elapsed = 0f;
        Quaternion startRot = transform.rotation;
        Quaternion endRot   = Quaternion.Euler(0, 0, 90f); // falls sideways

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(startRot, endRot, elapsed / fallDuration);
            yield return null; // wait one frame, then continue the loop
        }

        // Clear the player's interaction target before destroying
        if (currentPlayer != null)
            currentPlayer.ClearNearbyInteractable();

        // Destroy the tree GameObject
        // Note: script execution stops after this call
        Destroy(gameObject);
    }
}
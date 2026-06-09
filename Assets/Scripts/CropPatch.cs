// CropPatch.cs
// ─────────────────────────────────────────────────────────────────────────────
// Manages the lifecycle of a single crop patch.
// Implements IInteractable so the player can plant and harvest
// by pressing E when nearby.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections;
using UnityEngine;

public class CropPatch : MonoBehaviour, IInteractable
{
    // ── State Machine ────────────────────────────────────────────────────
    // An enum cleanly names each stage — much better than using magic
    // integers (0, 1, 2, 3) which are hard to read and maintain.
    public enum CropState
    {
        Empty,     // bare soil, ready to plant
        Planted,   // seed in ground, starting to grow
        Growing,   // sprouting — yellow visual
        Ready      // fully grown — green, can be harvested
    }

    // ── Inspector Settings ───────────────────────────────────────────────
    [SerializeField] private float    plantedDuration = 10f; // seconds until Growing
    [SerializeField] private float    growingDuration = 8f;  // seconds until Ready
    [SerializeField] private int      cropValue       = 10;  // score awarded on harvest
    [SerializeField] private string   cropName        = "Wheat";

    // ── Materials for each visual state ──────────────────────────────────
    // Drag these in from the Project window in the Inspector.
    [SerializeField] private Material matEmpty;    // brown soil
    [SerializeField] private Material matPlanted;  // dark, moist soil
    [SerializeField] private Material matGrowing;  // yellow/green sprout
    [SerializeField] private Material matReady;    // bright green, ready

    // ── Cached Components ────────────────────────────────────────────────
    private Renderer  patchRenderer;  // the mesh renderer on this soil patch
    private Coroutine growCoroutine;  // stored so we can stop it if needed

    // ── Current State ────────────────────────────────────────────────────
    private CropState currentState = CropState.Empty;

    // ────────────────────────────────────────────────────────────────────
    // Awake: cache the Renderer component.
    // Use GetComponentInChildren — Asset Store prefabs almost always put
    // the mesh (and its Renderer) on a child object, not the root.
    // GetComponent would miss it and return null every time.
    // ────────────────────────────────────────────────────────────────────
    void Awake()
    {
        // Search this object AND all children for a Renderer
        patchRenderer = GetComponentInChildren<Renderer>();

        if (patchRenderer == null)
            Debug.LogError($"CropPatch on {name}: No Renderer found on this object or any child!");
    }

    // ────────────────────────────────────────────────────────────────────
    // Start: ensure the patch starts in the Empty visual state
    // ────────────────────────────────────────────────────────────────────
    void Start()
    {
        SetVisualState(CropState.Empty);
    }

    // ────────────────────────────────────────────────────────────────────
    // IInteractable implementation
    // ────────────────────────────────────────────────────────────────────

    public string GetPromptText()
    {
        // Return the right prompt depending on the current state
        return currentState switch
        {
            CropState.Empty   => "Press E to Plant",
            CropState.Planted => "Growing... please wait",
            CropState.Growing => "Almost ready...",
            CropState.Ready   => $"Press E to Harvest {cropName}",
            _                  => ""
        };
    }

    public void Interact(PlayerFarmer player)
    {
        switch (currentState)
        {
            case CropState.Empty:
                Plant();        // start the growing process
                break;

            case CropState.Ready:
                Harvest(player); // give crop to player, reset patch
                break;

            // Planted/Growing states: do nothing — just show the prompt
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // Plant: transition from Empty to Planted, start growing coroutine
    // ────────────────────────────────────────────────────────────────────
    private void Plant()
    {
        currentState = CropState.Planted;
        SetVisualState(CropState.Planted);
        Debug.Log($"{name}: Planted. Growing in {plantedDuration} seconds.");

        // Start the coroutine and store the reference so we could stop it
        growCoroutine = StartCoroutine(GrowSequence());
    }

    // ────────────────────────────────────────────────────────────────────
    // GrowSequence: the coroutine that advances the crop through stages.
    //
    // yield return new WaitForSeconds(t) pauses HERE and resumes after
    // t seconds — Update() and other coroutines keep running normally.
    // ────────────────────────────────────────────────────────────────────
    private IEnumerator GrowSequence()
    {
        // ── Stage 1: Planted → Growing ──────────────────────────────────
        yield return new WaitForSeconds(plantedDuration);

        currentState = CropState.Growing;
        SetVisualState(CropState.Growing);
        Debug.Log($"{name}: Now growing. Ready in {growingDuration} seconds.");

        // ── Stage 2: Growing → Ready ────────────────────────────────────
        yield return new WaitForSeconds(growingDuration);

        currentState = CropState.Ready;
        SetVisualState(CropState.Ready);
        Debug.Log($"{name}: {cropName} is ready to harvest!");
    }

    // ────────────────────────────────────────────────────────────────────
    // Harvest: add crop to inventory, reset patch to Empty
    // ────────────────────────────────────────────────────────────────────
    private void Harvest(PlayerFarmer player)
    {
        Debug.Log($"{name}: Harvested {cropName} for {cropValue} points!");
        player.Inventory.AddCrop(cropValue);

        // Reset the patch back to Empty so it can be planted again
        currentState = CropState.Empty;
        SetVisualState(CropState.Empty);
    }

    // ────────────────────────────────────────────────────────────────────
    // SetVisualState: swap the material to match the current state.
    // This provides instant, visible feedback without any animation.
    // ────────────────────────────────────────────────────────────────────
    private void SetVisualState(CropState state)
    {
        if (patchRenderer == null) return;

        patchRenderer.material = state switch
        {
            CropState.Empty   => matEmpty,
            CropState.Planted => matPlanted,
            CropState.Growing => matGrowing,
            CropState.Ready   => matReady,
            _                  => matEmpty
        };
    }
}
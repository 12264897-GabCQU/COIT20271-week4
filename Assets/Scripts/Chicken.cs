// Chicken.cs
// ─────────────────────────────────────────────────────────────────────────────
// A hungry chicken wanders randomly. The player feeds it by pressing E
// while holding a feed bag. After 30 seconds the chicken gets hungry again.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections;
using UnityEngine;

public class Chicken : MonoBehaviour, IInteractable
{
    // ── Inspector Settings ───────────────────────────────────────────────
    [SerializeField] private float hungerDuration    = 30f; // seconds before hungry again
    [SerializeField] private float wanderRadius      = 3f;  // how far the chicken strays
    [SerializeField] private float wanderInterval    = 3f;  // seconds between wander moves
    [SerializeField] private float wanderSpeed       = 1.5f;// movement speed while wandering
    [SerializeField] private int   feedScore         = 15;  // score for feeding this chicken

    // ── Cached Components ────────────────────────────────────────────────
    private Animator animator;

    // ── State ────────────────────────────────────────────────────────────
    private bool isHungry = true;
    private Vector3 homePosition; // where the chicken started
    private Vector3 wanderTarget;  // current wander destination

    // ────────────────────────────────────────────────────────────────────
    // Awake: cache the Animator component
    // ────────────────────────────────────────────────────────────────────
    void Awake()
    {
        animator = GetComponent<Animator>();
        homePosition = transform.position;
        wanderTarget = homePosition;
    }

    // ────────────────────────────────────────────────────────────────────
    // Start: begin the wander coroutine immediately
    // ────────────────────────────────────────────────────────────────────
    void Start()
    {
        StartCoroutine(WanderRoutine());
    }

    // ────────────────────────────────────────────────────────────────────
    // Update: move toward the current wander target each frame
    // ────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (!isHungry) return; // fed chickens stand still

        float distToTarget = Vector3.Distance(transform.position, wanderTarget);

        if (distToTarget > 0.1f)
        {
            // Move toward wander target using Time.deltaTime for frame-rate independence
            transform.position = Vector3.MoveTowards(
                transform.position,
                wanderTarget,
                wanderSpeed * Time.deltaTime
            );

            // Face the direction of movement
            Vector3 dir = (wanderTarget - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // WanderRoutine: every wanderInterval seconds, pick a new random
    // destination within wanderRadius of the home position.
    // ────────────────────────────────────────────────────────────────────
    private IEnumerator WanderRoutine()
    {
        while (true) // loop forever — chicken always wanders when hungry
        {
            yield return new WaitForSeconds(wanderInterval);

            if (isHungry)
            {
                // Pick a random point within the wander radius
                Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
                wanderTarget = homePosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            }
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // IInteractable implementation
    // ────────────────────────────────────────────────────────────────────

    public string GetPromptText()
    {
        return isHungry
            ? "Press E to Feed Chicken (needs feed bag)"
            : "Chicken is happy!";
    }

    public void Interact(PlayerFarmer player)
    {
        if (!isHungry)
        {
            Debug.Log("This chicken is already fed.");
            return;
        }

        // Attempt to spend a feed bag from the player's inventory
        bool hasFeed = player.Inventory.SpendFeedBag();

        if (!hasFeed)
        {
            Debug.Log("You need a feed bag to feed the chicken. Find one on the farm!");
            return;
        }

        // Feed successful!
        Feed(player);
    }

    // ────────────────────────────────────────────────────────────────────
    // Feed: mark as fed, play animation, start the re-hunger timer
    // ────────────────────────────────────────────────────────────────────
    private void Feed(PlayerFarmer player)
    {
        isHungry = false;

        // Trigger the Eat animation on the Animator Controller
        // The string "Eat" must match the Trigger parameter name exactly.
        if (animator != null)
            animator.SetTrigger("Eat");

        // Award the player score directly via GameManager
        GameManager.Instance.AddScore(feedScore);

        Debug.Log($"Chicken fed! Will get hungry again in {hungerDuration} seconds.");

        // Start the countdown to hunger again
        StartCoroutine(HungerTimer());
    }

    // ────────────────────────────────────────────────────────────────────
    // HungerTimer: after hungerDuration seconds, the chicken is hungry again
    // ────────────────────────────────────────────────────────────────────
    private IEnumerator HungerTimer()
    {
        yield return new WaitForSeconds(hungerDuration);

        isHungry = true;
        Debug.Log($"{name} is hungry again!");
    }
}
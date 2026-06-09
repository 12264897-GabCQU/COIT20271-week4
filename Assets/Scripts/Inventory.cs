// Inventory.cs
// ─────────────────────────────────────────────────────────────────────────────
// Tracks everything the player is carrying: feed bags, harvested crops,
// and chopped wood. Other scripts call the public methods to add or
// spend items rather than accessing the counts directly.
// ─────────────────────────────────────────────────────────────────────────────

using UnityEngine;

public class Inventory : MonoBehaviour
{
    // ── Starting quantities (tunable in the Inspector) ──────────────────
    [SerializeField] private int startingFeedBags = 0;

    // ── Private counts — other scripts use the public methods below ─────
    private int feedBags;
    private int cropsHarvested;
    private int woodChopped;

    // ── Properties — read-only access for other scripts ─────────────────
    public int FeedBags      => feedBags;
    public int CropsHarvested => cropsHarvested;
    public int WoodChopped   => woodChopped;

    // ────────────────────────────────────────────────────────────────────
    // Awake: initialise quantities from Inspector values
    // ────────────────────────────────────────────────────────────────────
    void Awake()
    {
        feedBags = startingFeedBags;
    }

    // ── Feed Bag methods ─────────────────────────────────────────────────

    public void AddFeedBag()
    {
        feedBags++;
        Debug.Log($"Picked up feed bag. Total: {feedBags}");
    }

    // Returns true if the player had a feed bag to spend, false if empty.
    public bool SpendFeedBag()
    {
        if (feedBags <= 0)
        {
            Debug.Log("No feed bags left!");
            return false;
        }
        feedBags--;
        Debug.Log($"Used feed bag. Remaining: {feedBags}");
        return true;
    }

    // ── Crop methods ─────────────────────────────────────────────────────

    public void AddCrop(int value)
    {
        cropsHarvested++;
        // Notify the GameManager to update score
        GameManager.Instance.AddScore(value);
        Debug.Log($"Harvested crop worth {value} points. Total crops: {cropsHarvested}");
    }

    // ── Wood methods ─────────────────────────────────────────────────────

    public void AddWood(int value)
    {
        woodChopped++;
        GameManager.Instance.AddScore(value);
        Debug.Log($"Chopped wood worth {value} points. Total wood: {woodChopped}");
    }
}
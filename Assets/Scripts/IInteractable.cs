// IInteractable.cs
// ─────────────────────────────────────────────────────────────────────────────
// This is a C# interface — not a MonoBehaviour.
// Any script that can be interacted with by the player must implement
// this interface. The player calls Interact() without caring what type
// of object it is talking to (CropPatch, Chicken, or Tree).
// ─────────────────────────────────────────────────────────────────────────────

public interface IInteractable
{
    // Called when the player presses E while standing near this object.
    // The PlayerFarmer reference is passed in so the object can access
    // the player's inventory or position if needed.
    void Interact(PlayerFarmer player);

    // Returns the prompt text shown on screen when the player is nearby.
    // Example: "Press E to Plant", "Press E to Feed Chicken"
    string GetPromptText();
}
// MobilePlatformUI.cs
// ─────────────────────────────────────────────────────────────────────────────
// Shows the on-screen buttons only when running on a touch device.
// Attach to the Canvas. Drag the mobile button panel into mobileControlsPanel.
// ─────────────────────────────────────────────────────────────────────────────

using UnityEngine;

public class MobilePlatformUI : MonoBehaviour
{
    [SerializeField] private GameObject mobileControlsPanel; // parent of E button, chop button
    [SerializeField] private GameObject keyboardPromptPanel; // "Press E to ..." text panel

    void Awake()
    {
        // Application.isMobilePlatform is true on Android and iOS at runtime.
        // In the Editor it is false — so keyboard controls stay active while testing.
        bool isMobile = Application.isMobilePlatform;

        if (mobileControlsPanel  != null) mobileControlsPanel.SetActive(isMobile);
        if (keyboardPromptPanel  != null) keyboardPromptPanel.SetActive(!isMobile);
    }
}
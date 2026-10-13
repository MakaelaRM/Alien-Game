using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private InputAction interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
    [SerializeField] private InputAction exitAction = new InputAction("ExitInteract", InputActionType.Button, "<Keyboard>/escape");
    [SerializeField] private float interactRange = 5f;
    [SerializeField] private FixedAngleCamera cameraController;

    private NPCInteractable currentNPC;      // NPC whose prompt is showing
    private NPCInteractable interactingNPC;  // NPC we're currently interacting with
    private bool dialogueShown;

    public bool IsInteracting => interactingNPC != null;

    void Awake()
    {
        if (cameraController == null && Camera.main != null)
            cameraController = Camera.main.GetComponent<FixedAngleCamera>();
    }

    void OnEnable()
    {
        interactAction.Enable();
        exitAction.Enable();
    }

    void OnDisable()
    {
        interactAction.Disable();
        exitAction.Disable();
    }

    void Update()
    {
        if (IsInteracting)
        {
            // Show the dialogue once the camera has finished zooming in
            if (!dialogueShown && (cameraController == null || cameraController.IsFocusComplete))
            {
                Transform viewer = cameraController != null ? cameraController.transform : Camera.main.transform;
                interactingNPC.ShowDialogue(exitAction.GetBindingDisplayString(), viewer);
                dialogueShown = true;
            }

            if (exitAction.WasPressedThisFrame())
                EndInteraction();
            return; // skip prompt logic so no name bubbles appear during the interaction
        }

        NPCInteractable closest = FindClosestNPC();

        if (closest != currentNPC)
        {
            if (currentNPC != null) currentNPC.HidePrompt();
            if (closest != null) closest.ShowPrompt(interactAction.GetBindingDisplayString());
            currentNPC = closest;
        }

        if (currentNPC != null && interactAction.WasPressedThisFrame())
        {
            StartInteraction(currentNPC);
        }
    }

    private void StartInteraction(NPCInteractable npc)
    {
        interactingNPC = npc;
        dialogueShown = false;
        npc.HidePrompt();
        npc.Interact();

        if (cameraController != null)
            cameraController.FocusOn(npc.transform, npc.FocusPoint);
    }

    private void EndInteraction()
    {
        interactingNPC.HideDialogue();
        interactingNPC = null;
        currentNPC = null; // forces the prompt to be re-checked next frame
        dialogueShown = false;

        if (cameraController != null)
            cameraController.StopFocus();
    }

    private NPCInteractable FindClosestNPC()
    {
        NPCInteractable closest = null;
        float closestDistance = Mathf.Infinity;

        Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);
        foreach (Collider collider in colliderArray)
        {
            if (collider.TryGetComponent(out NPCInteractable npc))
            {
                float distance = Vector3.Distance(transform.position, npc.transform.position);

                if (distance <= interactRange && distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = npc;
                }
            }
        }
        return closest;
    }
}
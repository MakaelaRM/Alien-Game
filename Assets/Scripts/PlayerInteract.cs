using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private InputAction interactAction;
    [SerializeField] private float interactRange = 5f;

    private NPCInteractable currentNPC;

    void OnEnable()  => interactAction.Enable();
    void OnDisable() => interactAction.Disable();

    void Update()
    {
        NPCInteractable closest = FindClosestNPC();

        if (closest != currentNPC)
        {
            if (currentNPC != null) currentNPC.HidePrompt();
            if (closest != null) closest.ShowPrompt(interactAction.GetBindingDisplayString());
            currentNPC = closest;
        }

        if (currentNPC != null && interactAction.WasPressedThisFrame())
        {
            currentNPC.Interact();
        }
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
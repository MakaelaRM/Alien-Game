using UnityEngine;
using TMPro;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private string npcName;
    [SerializeField] private GameObject chatBubblePrefab;
    [SerializeField] private float bubblePadding = 0.3f;

    private GameObject chatBubble;
    private TextMeshProUGUI bubbleText;

    void Reset()
    {
        #if UNITY_EDITOR
            chatBubblePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/UI/ChatBubble.prefab");
        #endif
    }
    void Awake()
    {
        if (string.IsNullOrEmpty(npcName))
            npcName = gameObject.name;

        float topOfHead = GetTopOfModel();

        Vector3 bubblePosition = new Vector3(
            transform.position.x,
            topOfHead + bubblePadding,
            transform.position.z);

        chatBubble = Instantiate(chatBubblePrefab, bubblePosition, Quaternion.identity, transform);

        bubbleText = chatBubble.GetComponentInChildren<TextMeshProUGUI>(true);
        chatBubble.SetActive(false);
    }

    private float GetTopOfModel()
    {
        float top = transform.position.y;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            top = Mathf.Max(top, r.bounds.max.y);
        return top;
    }

    public void ShowPrompt(string keyName)
    {
        bubbleText.text = npcName + "\nPress " + keyName + " to interact";
        chatBubble.SetActive(true);
    }

    public void HidePrompt() => chatBubble.SetActive(false);

    public void Interact()
    {
        Debug.Log("Interacting with: " + npcName);
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hovering;

    private ItemSO heldItem;
    private int itemAmount;

    private Image iconImage;
    private TextMeshProUGUI amountTxt;

    private void Awake()
    {
        iconImage = transform.GetChild(0).GetComponent<Image>();
        amountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public ItemSO GetItem()
    {
        return heldItem;
    }

    public int GetAmount()
    {
        return itemAmount;
    }

    public void SetItem(ItemSO item, int amount = 1)
    {
        heldItem = item;
        itemAmount = amount;

        UpdateSlot();
    }

    public void UpdateSlot()
    {
         if (heldItem != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = heldItem.icon;
            amountTxt.text = itemAmount.ToString();
            //amountTxt.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
            //iconImage.sprite = null;
            amountTxt.text = "";
            //amountTxt.enabled = false;
        }
    }

    public int AddAmount(int amount)
    {
        itemAmount += amount;
        UpdateSlot();
        return itemAmount;
    }

    public int RemoveAmount(int amount)
    {
        itemAmount -= amount;
        if (itemAmount <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateSlot();
        }
        return itemAmount;
    }

    public void ClearSlot()
    {
        heldItem = null;
        itemAmount = 0;
        UpdateSlot();
    }

    public bool HasItem()
    {
        return heldItem != null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    //public void IPointerEnterHandler(PointerEventData eventData)
    //{
    //    hovering = true;
    //}

    //public void IPointerExitHandler(PointerEventData eventData)
    //{
    //    hovering = false;
    //}

    //public void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    //{
    //    hovering = true;
    //}

    //public void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    //{
    //    hovering = false;
    //}
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    public ItemSO medicineItem;
    public ItemSO weaponItem;

    public GameObject hotbarObj;
    public GameObject inventorySlotParent;

    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

    private void Awake()
    {
        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObj.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);

        Debug.Log("Inventory slots: " + inventorySlots.Count);
        Debug.Log("Hotbar slots: " + hotbarSlots.Count);
        Debug.Log("Total slots: " + allSlots.Count);
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            AddItem(medicineItem, 3);
        }
        else if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            AddItem(weaponItem, 1);
        }
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.M))
    //    {
    //        AddItem(medicineItem, 3);
    //    }
    //    else if (Input.GetKeyDown(KeyCode.W))
    //    {
    //        AddItem(weaponItem, 1);
    //    }
    //}

    public void AddItem(ItemSO itemToAdd, int amount)
    {
        int remaining = amount;

        foreach(Slot slot in allSlots)
        {
            if(slot.HasItem() && slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStackSize;

                if(currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remaining);

                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);
                    remaining -= amountToAdd;

                    if (remaining <= 0)
                    {
                        return;
                    }
                  
                }
            }
        }

        foreach(Slot slot in allSlots)
        {
            if (!slot.HasItem())
            {
                int amountToPlace = Mathf.Min(itemToAdd.maxStackSize, remaining);
                slot.SetItem(itemToAdd, amountToPlace);
                remaining -= amountToPlace;

                if (remaining <= 0)
                {
                    return;
                }
            }
        }

        if(remaining > 0)
        {
            Debug.LogWarning("Not enough space in inventory to add all items. Remaining: " + remaining + " of " + itemToAdd.itemName);
        }
    }
}

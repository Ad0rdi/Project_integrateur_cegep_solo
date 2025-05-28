/* Original author Name: Donavan Sirois
 * Creation date: 2025/05/03
 * Goal: Program the inventory item class which is used for the inventory system
 * Modification listing:
 * 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] _inventorySlots;
    public GameObject _itemPrefab;
    int _selected = -1;
    void Start()
    {
        ChangeSelectedSlot(0);
    }

    private void Update()
    {
        if (Input.inputString != null)
        {
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if (isNumber && number > 0 && number < 9)
            {
                ChangeSelectedSlot(number - 1);
            }
        }
    }
    void ChangeSelectedSlot(int newSelection)
    {
        if (_selected >= 0)
        {
            _inventorySlots[_selected].Unselect();
        }
        _inventorySlots[newSelection].Select();
        _selected = newSelection;
    }
    public bool AddItem(Item item)
    {
        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            InventorySlot slot = _inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null)
            {
                SpawnItem(item, slot);
                return true;
            }
        }
        return false;
    }

    void SpawnItem(Item item, InventorySlot slot)
    {
        GameObject newItem = Instantiate(_itemPrefab, slot.transform);
    }

    public Item GetItem()
    {
        InventorySlot slot = _inventorySlots[_selected];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            return itemInSlot._item;
        }

        return null;
    }
}

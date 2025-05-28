/* Original author Name: Donavan Sirois
 * Creation date: 2025/05/03
 * Goal: Program the inventory item class which is used for the inventory system
 * Modification listing:
 * 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    [HideInInspector] public Item _item;
    public Image _image;

    private void Start()
    {
        InitialiseItem(_item);
    }
    public void InitialiseItem(Item newItem)
    {
        _item = newItem;
        _image.sprite = newItem._icon;
    }
}

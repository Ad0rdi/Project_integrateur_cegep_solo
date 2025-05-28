/* Original author Name: Donavan Sirois
 * Creation date: 2025/05/03
 * Goal: Program the inventory item class which is used for the inventory system
 * Modification listing:
 * 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    public Image image;
    public Color _selected, _unselected;

    private void Awake()
    {
        Unselect();
    }
    public void Select()
    {
        image.color = _selected;
    }
    
    public void Unselect()
    {
        image.color = _unselected;
    }
}

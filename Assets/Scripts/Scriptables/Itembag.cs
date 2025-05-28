/* Original author Name: Donavan Sirois
 * Creation date: 2025/05/06
 * Goal: Program the loot drops of creatures and chests
 * Modification listing:
 * 
*/

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Itembag : MonoBehaviour
{
    public GameObject _droppedItemPrefab;
    public List<Item> _itemList = new List<Item>();

    Item GetDroppedItem()
    {
        int rarity = Random.Range(1, 101);
        List<Item> possibleItems = new List<Item>();
        foreach (Item item in _itemList)
        {
            if(rarity <= item._dropChance)
            {
                possibleItems.Add(item);
            }
        }
        if (possibleItems.Count > 0)
        {
            Item droppedItem = possibleItems[Random.Range(0, possibleItems.Count)];
            return droppedItem;
        }
        return null;
    }

    public void InstantiateItem(Vector3 spawnPosition)
    {
        Item droppedItem = GetDroppedItem();
        if (droppedItem != null)
        {
            GameObject itemGameObject = Instantiate(_droppedItemPrefab, spawnPosition, Quaternion.identity);
            itemGameObject.GetComponent<SpriteRenderer>().sprite = droppedItem._icon;
        }
    }
}

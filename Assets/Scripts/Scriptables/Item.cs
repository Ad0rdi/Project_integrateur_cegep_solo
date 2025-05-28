/* Original author Name: Donavan Sirois
 * Creation date: 2025/05/03
 * Goal: Program the scriptable item object used for guns and tools
 * Modification listing:
 * 2025/05/06:
 *      Author Name: Donavan Sirois
 *      Goal: Modified for the loot drop that enemies will drop in the world
*/

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable object/Item")]
public class Item : ScriptableObject
{
    public Sprite _icon;
    public string _lootName;
    public int _dropChance;
    public ItemType _type;
    public ActionType _action;
}

public enum ItemType
{
    Gun,
    Tool,
    Hook
}

public enum ActionType
{
    Shoot,
    Mine,
    Grapple
}

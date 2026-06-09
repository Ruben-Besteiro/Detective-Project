using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Pickups/Pickup Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite image;
}

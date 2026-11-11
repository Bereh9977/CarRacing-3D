using UnityEngine;

[CreateAssetMenu(menuName = "Car Racing/Car Definition", fileName = "CarDefinition")]
public class CarDefinition : ScriptableObject
{
    public string id;
    public bool isFree;
    public int price = 3000;
    public GameObject prefab;
}

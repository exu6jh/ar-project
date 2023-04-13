using UnityEngine;

[CreateAssetMenu(fileName = "VectorMaterials", menuName = "ScriptableObjects/VectorMaterials", order = 0)]
public class VectorMaterials : ScriptableObject
{
    public Material[] materials;

    public Material Material(int index)
    {
        return materials[index];
    }
}
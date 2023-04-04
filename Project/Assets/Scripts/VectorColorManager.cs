using UnityEngine;

public class VectorColorManager : MonoBehaviour
{
    // public Material[] materials;
    public VectorMaterials vectorMaterials;
    
    private MeshRenderer cylinder;
    
    
    private void Start()
    {
        cylinder = GetComponent<VectorManager>().TransformCylinder.transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    public void Material(int index)
    {
        cylinder.material = vectorMaterials.Material(index);
    }
}
using UnityEngine;

public class VectorColorManager : MonoBehaviour
{
    // public Material[] materials;
    public VectorMaterials vectorMaterials;
    
    private MeshRenderer cylinder;

    public int answerNum;
    
    
    private void Awake()
    {
        cylinder = GetComponent<VectorManager>().TransformCylinder.transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    public void Material(int index)
    {
        cylinder.material = vectorMaterials.Material(index);
    }

    public void Answer(int answer)
    {
        Material(answerNum == answer ? 1 : 0);
    }
}
#if (UNITY_EDITOR)

using System.Numerics;
using UnityEditor;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[CustomEditor(typeof(BasisVectorManager))]
public class BasisVectorManagerEditor : Editor
{

    private Vector3 newPosition = Vector3.right;
    private float newLength = 1;
    private Vector3 newEuler = new Vector3(0, 0, -90);
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        BasisVectorManager basisVectorManager = (BasisVectorManager) target;

        if (GUILayout.Button("Set New Z Offset"))
        {
            Vector3 oldPosition = basisVectorManager.transform.localPosition;
            oldPosition.z = basisVectorManager.zOffset;
            basisVectorManager.transform.localPosition = oldPosition;
        }

        newPosition = EditorGUILayout.Vector3Field("New Position", newPosition);

        if (GUILayout.Button("Set New Position"))
        {
            basisVectorManager.SetNewPosition(newPosition);
            newLength = basisVectorManager.length;
            newEuler = basisVectorManager.euler;

            // // Length changes
            // newLength = newPosition.magnitude;
            //
            // Vector3 newLocalScale = basisVectorManager.TransformCylinder.transform.localScale;
            // newLocalScale.y = newLength;
            // basisVectorManager.TransformCylinder.transform.localScale = newLocalScale;
            //
            // Transform cone = basisVectorManager.TransformCone.transform.GetChild(0);
            // Vector3 newLocalPosition = cone.localPosition;
            // newLocalPosition.y = newLength;
            // cone.localPosition = newLocalPosition;
            //
            // // Rotation changes
            // basisVectorManager.transform.localRotation = Quaternion.FromToRotation(Vector3.up, newPosition);
            // newEuler = basisVectorManager.transform.localRotation.eulerAngles;
        }
        
        newLength = EditorGUILayout.FloatField("New Length", newLength);
        newEuler = EditorGUILayout.Vector3Field("New Euler", newEuler);
        
        if (GUILayout.Button("Set New Length and Euler"))
        {
            // Length changes
            Vector3 newLocalScale = basisVectorManager.TransformCylinder.transform.localScale;
            newLocalScale.y = newLength;
            basisVectorManager.TransformCylinder.transform.localScale = newLocalScale;
            
            Transform cone = basisVectorManager.TransformCone.transform.GetChild(0);
            Vector3 newLocalPosition = cone.localPosition;
            newLocalPosition.y = newLength;
            cone.localPosition = newLocalPosition;
            
            // Rotation changes
            basisVectorManager.transform.localRotation = Quaternion.Euler(newEuler);
            
            // Update newPosition
            newPosition = Quaternion.Euler(newEuler) * (Vector3.up * newLength);
        }
    }
}

#endif // (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionDetection : MonoBehaviour
{
    public string interactionType;
    public GameObject otherObject;
    public float distanceThreshold;
    public LessonReader lessonReader;

    // Update is called once per frame
    void Update()
    {
        if(interactionType.Equals("GETS-CLOSE") && Vector3.Distance(transform.position, otherObject.transform.position) <= distanceThreshold) {
            Debug.Log("Sufficiently close.");
            lessonReader.ExecuteFromSavePoint();
            Destroy(this);
        }
    }

    void OnCollisionEnter(Collision collision) {
        Debug.Log("Collision entered.");
        if(interactionType.Equals("COLLIDES") && collision.gameObject.Equals(otherObject)) {
            Debug.Log("It works!");
            lessonReader.ExecuteFromSavePoint();
            Destroy(this);
        }
    }
}

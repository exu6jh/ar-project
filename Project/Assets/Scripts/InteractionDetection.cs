using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Interaction detection: specifically for the WAIT-UNTIL tag
public class InteractionDetection : MonoBehaviour
{
    public string interactionType;
    public GameObject otherObject;
    public float distanceThreshold;
    public LessonReader lessonReader;

    // Update is called once per frame
    void Update()
    {
        // Proximity detection
        if(interactionType.Equals("GETS-CLOSE") && Vector3.Distance(transform.position, otherObject.transform.position) <= distanceThreshold) {
            Debug.Log("Sufficiently close.");
            lessonReader.ExecuteFromSavePoint();
            Destroy(this);
        }
    }

    // Collision detection; this is the predominant reason why interaction detection is its own class,
    // as detecting if two *other* objects have collided is surprisingly difficult
    void OnCollisionEnter(Collision collision) {
        Debug.Log("Collision entered.");
        if(interactionType.Equals("COLLIDES") && collision.gameObject.Equals(otherObject)) {
            lessonReader.ExecuteFromSavePoint();
            Destroy(this);
        }
    }
}

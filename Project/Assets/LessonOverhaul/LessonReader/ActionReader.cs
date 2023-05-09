using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class ActionReader : MonoBehaviour
{
    // for reading commands
    public ActionRepository repo;
    private List<ActionHolder> commands;
    
    // for generally keeping track of objects in the scene
    private Dictionary<string, bool> flags;
    private Dictionary<string, GameObject> gameObjects;
    private Dictionary<string, Matrix> matrices;

    // for DRAW
    [Header("Geometry Assets")]
    public GameObject grid;
    public GameObject point;
    public GameObject vector;

    // for keeping track of execution
    [HideInInspector]
    public bool executing = false;
    [HideInInspector]
    public float startTime = 0f;
    [HideInInspector]
    public float pauseTime = 0f;
    private int line = 0;

    // Start is called before the first frame update
    void Start()
    {
        commands = repo.GetActions();
        flags = new Dictionary<string, bool>();
        gameObjects = new Dictionary<string, GameObject>();
        matrices = new Dictionary<string, Matrix>();
    }

    void Update()
    {
        while(executing && line < commands.Count && Time.time - startTime >= commands[line].time) {
            Execute(line, 0f);
            line++;
        }
    }
    
    // For executing a command line.
    // Line execution is supported through a series of regexes.
    void Execute(int index, float progress) {
        Debug.Log(string.Format("Executing line {0}", index));
        // Handle out of bounds exceptions.
        if(index >= commands.Count) {
            Debug.Log("Script execution terminated at end of file.");
            return;
        }

        ActionHolder holder = commands[index];
        // CREATE-OBJECT command: creates an object from assets, with either default name or new name
        if(holder.command == CommandType.CreateObject) {
            Debug.Log("Creating game object.");
            try {
                // Instantiate corresponding asset from AssetDatabase.
                // Note that this is editor-only!
                string[] matchingAssets = AssetDatabase.FindAssets(holder.editorObjectName);
                string dataPath = AssetDatabase.GUIDToAssetPath(matchingAssets[0]);
                GameObject obj = (GameObject)AssetDatabase.LoadAssetAtPath(dataPath, typeof(GameObject));
                GameObject newObj = Instantiate(obj);

                // Check for optional custom name.
                if(!string.IsNullOrEmpty(holder.internalObjectName)) {
                    newObj.name = holder.internalObjectName;
                    gameObjects[holder.internalObjectName] = newObj;
                } else {
                    gameObjects[holder.editorObjectName] = newObj;
                }
            } catch {
                Debug.Log(string.Format("Asset \"{0}\" not found.", holder.editorObjectName));
            }
            return;
        // CREATE-MATRIX command: creates a matrix with given name and values
        } else if (holder.command == CommandType.CreateMatrix) {
            Debug.Log("Creating matrix.");
            // Obtain matrix elements
            matrices[holder.internalObjectName] = new Matrix(holder.matrixFields);
            return;
        // DELETE-OBJECT command: deletes the specified game object
        } else if(holder.command == CommandType.DeleteObject) {
            Debug.Log("Deleting game object.");
            if(gameObjects.ContainsKey(holder.internalObjectName)) {
                Destroy(gameObjects[holder.internalObjectName]);
            } else {
                Debug.Log(string.Format("No such object {0} detected.", holder.internalObjectName));
            }
            return;
        // ASSIGN-PROPERTY command: assigns a specific property to a given object, either instantaneously or over time
        } else if(holder.command == CommandType.AssignProperty) {
            Debug.Log("Assigning property.");
            GameObject obj;
            // Check for object to assign property to
            if(gameObjects.ContainsKey(holder.internalObjectName)) {
                obj = gameObjects[holder.internalObjectName];
            } else {
                Debug.Log(string.Format("No such object {0} detected.", holder.internalObjectName));
                return;
            }

            // If position is being changed...
            if(holder.property.Equals("POS")) {
                Debug.Log("Changing position.");
                try {
                    if(holder.matrixFields.GetLength(0) != 1 || holder.matrixFields.GetLength(1) != 3) {
                        Debug.Log("Error: position should be 1 row by 3 columns.");
                        throw new System.ArithmeticException();
                    }
                    float[,] pos = holder.matrixFields;
                    Vector3 targetPosition = new Vector3(pos[0,0], pos[0,1], pos[0,2]);
                    // Either directly change position, or change over time
                    if(holder.duration > 0f) {
                        Debug.Log("Movement time detected.");
                        IEnumerator posCoroutine = Pos(obj.transform, targetPosition, holder.duration >= progress ? 0f : holder.duration);
                        StartCoroutine(posCoroutine);
                    } else {
                        Debug.Log("No movement time detected.");
                        obj.transform.position = targetPosition;
                    }
                } catch {
                    Debug.Log("Error: inconsistent matrix size.");
                }
                return;
            // If rotation is being changed...
            } else if(holder.property.Equals("ROT")) {
                Debug.Log("Changing rotation.");
                try {
                    if(holder.matrixFields.GetLength(0) != 1 || holder.matrixFields.GetLength(1) != 3) {
                        Debug.Log("Error: rotation should be 1 row by 3 columns.");
                        throw new System.ArithmeticException();
                    }
                    float[,] rot = holder.matrixFields;
                    Vector3 targetRotation = new Vector3(rot[0,0], rot[0,1], rot[0,2]);
                    // Either directly change rotation, or change over time
                    if(holder.duration > 0f) {
                        Debug.Log("Rotation time detected.");
                        IEnumerator rotCoroutine = Rot(obj.transform, targetRotation, holder.duration >= progress ? 0f : holder.duration);
                        StartCoroutine(rotCoroutine);
                    } else {
                        Debug.Log("No movement time detected.");
                        obj.transform.eulerAngles = targetRotation;
                    }
                } catch {
                    Debug.Log("Error: inconsistent matrix size.");
                }
                return;
            //If scale is being changed...
            } else if(holder.property.Equals("SCALE")) {
                Debug.Log("Changing scale.");
                try {
                    if(holder.matrixFields.GetLength(0) != 1 || holder.matrixFields.GetLength(1) != 3) {
                        Debug.Log("Error: scale should be 1 row by 3 columns.");
                        throw new System.ArithmeticException();
                    }
                    float[,] scale = holder.matrixFields;
                    Vector3 targetScale = new Vector3(scale[0,0], scale[0,1], scale[0,2]);
                    // Change scale
                    if(holder.duration > 0f) {
                        Debug.Log("Scale time detected.");
                        IEnumerator scaleCoroutine = Scale(obj.transform, targetScale, holder.duration >= progress ? 0f : holder.duration);
                        StartCoroutine(scaleCoroutine);
                    } else {
                        Debug.Log("No movement time detected.");
                        obj.transform.localScale = targetScale;
                    }
                } catch {
                    Debug.Log("Error: inconsistent matrix size.");
                }
                return;
            // If no valid field is chosen, just skip command
            } else {
                Debug.Log(string.Format("Error: no valid property {0}", holder.property));
            }
            return;
        // PLAY SOUND command: plays a given sound asset
        } else if(holder.command == CommandType.PlaySound) {
            Debug.Log("Playing sound.");
            try {
                // Obtain sound from AssetDatabase
                // Like with CREATE-OBJECT, this is editor-only
                string[] matchingAssets = AssetDatabase.FindAssets(holder.editorObjectName);
                string dataPath = AssetDatabase.GUIDToAssetPath(matchingAssets[0]);
                AudioClip audio = (AudioClip)AssetDatabase.LoadAssetAtPath(dataPath, typeof(AudioClip));
                AudioSource source = GetComponent<AudioSource>();
                if(progress < audio.length) {
                    // Play audio
                    source.clip = audio;
                    source.time = progress;
                    source.Play();
                }
            } catch {
                Debug.Log(string.Format("Sound error, either clip \"{0}\" not found or no audio component attached.", holder.editorObjectName));
            }
            return;
        // DRAW GRID command: draws built-in grid
        } else if(holder.command == CommandType.DrawGrid) {
            Debug.Log("Drawing grid.");
            // Instantiate grid and put into gameObjects dictionary
            GameObject newGrid = Instantiate(grid);
            gameObjects[holder.internalObjectName] = newGrid;
            return;
        // DRAW POINT command: draws built-in point on GRID
        } else if(holder.command == CommandType.DrawPoint) {
            Debug.Log("Drawing point.");
            // Instantiate point and put into gameObjects dictionary
            GameObject newPoint = Instantiate(point);
            gameObjects[holder.internalObjectName] = newPoint;
            try {
                // Set default values related to grid, on point's managers
                GameObject managerObj = gameObjects[holder.affiliatedObjects[0]];
                newPoint.transform.parent = managerObj.transform;
                GridManager manager = managerObj.GetComponent<GridManager>();
                newPoint.GetComponent<PointManager>().gridManager = manager;
                newPoint.GetComponent<PointSnapConstraint>().origin = manager.origin;
                // Scale normalization
                newPoint.transform.localScale = new Vector3(1,1,1);
            } catch {
                Debug.Log(string.Format("Error: when drawing point, could not find grid corresponding to {0}.", holder.affiliatedObjects[0]));
            }
            return;
        // DRAW VECTOR command: draws built-in vector from one POINT to another on the same GRID
        } else if(holder.command == CommandType.DrawVector) {
            Debug.Log("Drawing vector.");
            // Instantiate vector and put into gameObjects dictionary
            GameObject newVector = Instantiate(vector);
            gameObjects[holder.internalObjectName] = newVector;
            try {
                // Attempt to get both endpoints
                GameObject endpoint1 = gameObjects[holder.affiliatedObjects[0]];
                GameObject endpoint2 = gameObjects[holder.affiliatedObjects[1]];
                // Check if they are on the same grid
                if(endpoint1.GetComponent<PointManager>().gridManager != endpoint2.GetComponent<PointManager>().gridManager) {
                    Debug.Log("You must select two endpoints that are on the same grid.");
                    throw new System.ArithmeticException();
                }
                // Set default values related to grid, on vector's managers
                VectorEndpointConstraint constraint = newVector.GetComponent<VectorEndpointConstraint>();
                GridManager manager = endpoint1.GetComponent<PointManager>().gridManager;
                constraint.from = endpoint1;
                constraint.to = endpoint2;
                newVector.GetComponent<VectorManager>().gridManager = manager;
                constraint.gridManager = manager;
                newVector.transform.parent = manager.transform;
                // Hardcoded for ideal aesthetics.
                newVector.transform.localScale = new Vector3(10,1,10);
            } catch {
                Debug.Log(string.Format("Error involving the supplied endpoints {0} and {1}", holder.affiliatedObjects[0], holder.affiliatedObjects[1]));
            }
            return;
        // APPLY-MATRIX command: applies a linear transformation as specified by a matrix to a given object
        } else if(holder.command == CommandType.ApplyMatrix) {
            Debug.Log("Applying matrix transformation.");
            if(matrices.ContainsKey(holder.internalObjectName) && gameObjects.ContainsKey(holder.affiliatedObjects[0])) {
                // Apply transformation to every mesh that is in the object
                GameObject parent = gameObjects[holder.affiliatedObjects[0]];
                foreach(MeshFilter filters in parent.GetComponentsInChildren<MeshFilter>()) {
                    Mesh mesh = filters.mesh;
                    Vector3[] vertices = mesh.vertices;
                    for(int i = 0; i < vertices.Length; i++) {
                        // Get the position (as a matrix) of the descendant vertex with respect to the base object
                        Vector3 wrtParent = filters.transform.TransformPoint(vertices[i]) - parent.transform.position;
                        float[,] vertexPosition = new float[3,1]{{wrtParent.x},{wrtParent.y},{wrtParent.z}};
                        Matrix vertexMatrix = new Matrix(vertexPosition);
                        // Transform the position to get the transformed position with respect to the base object
                        Matrix transformedMatrix = matrices[holder.internalObjectName] * vertexMatrix;
                        Vector3 transformedWrtParent = new Vector3(transformedMatrix.values[0,0], transformedMatrix.values[1,0], transformedMatrix.values[2,0]);
                        // Convert the transformed position with respect to the base object back into descendant coordinates
                        Vector3 transformedPosition = filters.transform.InverseTransformPoint(transformedWrtParent + parent.transform.position);
                        vertices[i] = transformedPosition;
                    }
                    mesh.vertices = vertices;
                    mesh.RecalculateNormals();
                }
            } else {
                Debug.Log(string.Format("Either no matrix {0} or no object {1} detected.", holder.internalObjectName, holder.affiliatedObjects[0]));
            }
            return;
        // Invalid command
        } else {
            Debug.Log(string.Format("No command, or invalid command, received at command line {0}.", index));
            Debug.Log(string.Format("If you have a command written down, there is likely a syntax error!"));
            return;
        }
    }

    // Position changing coroutine
    private IEnumerator Pos(Transform obj, Vector3 target, float time) {
        if(time > 0.1f) {
            Vector3 initPosition = obj.position;
            Debug.Log("Changing position.");
            for(int i = 0; i < Mathf.Ceil(time / Time.fixedDeltaTime); i++) {
                obj.position = Vector3.Lerp(initPosition, target, i / Mathf.Ceil(time / Time.fixedDeltaTime));
                yield return new WaitForSeconds(time / Mathf.Ceil(time / Time.fixedDeltaTime));
            }
            obj.position = target;
            Debug.Log("Change over.");
        } else {
            obj.position = target;
            Debug.Log("Time given is less than 0.1 seconds and is imperceptibly small; instantaneous change applied.");
        }
    }

    // Rotation changing coroutine
    private IEnumerator Rot(Transform obj, Vector3 target, float time)
    {
        bool useSlerp = true;
        if(time > 0.1f) {
            if (useSlerp)
            {
                Quaternion initRotation = obj.rotation;
                Quaternion targetRotation = Quaternion.Euler(target);
                Debug.Log("Changing rotation.");
                for (int i = 0; i < Mathf.Ceil(time / Time.fixedDeltaTime); i++)
                {
                    obj.rotation = Quaternion.Slerp(initRotation, targetRotation, i / Mathf.Ceil(time / Time.fixedDeltaTime));
                    yield return new WaitForSeconds(time / Mathf.Ceil(time / Time.fixedDeltaTime));
                }

                obj.eulerAngles = target;
                Debug.Log("Change over.");
            }
            else
            {
                Vector3 initRotation = obj.eulerAngles;
                Debug.Log("Changing rotation.");
                for (int i = 0; i < Mathf.Ceil(time / Time.fixedDeltaTime); i++)
                {
                    obj.eulerAngles = Vector3.Lerp(initRotation, target, i / Mathf.Ceil(time / Time.fixedDeltaTime));
                    yield return new WaitForSeconds(time / Mathf.Ceil(time / Time.fixedDeltaTime));
                }

                obj.eulerAngles = target;
                Debug.Log("Change over.");
            }
        } else {
            obj.eulerAngles = target;
            Debug.Log("Time given is less than 0.1 seconds and is imperceptibly small; instantaneous change applied.");
        }
    }

    private IEnumerator Scale(Transform obj, Vector3 target, float time) {
        if(time > 0.1f) {
            Vector3 initScale = obj.localScale;
            Debug.Log("Changing scale.");
            for(int i = 0; i < Mathf.Ceil(time / Time.fixedDeltaTime); i++) {
                obj.localScale = Vector3.Lerp(initScale, target, i / Mathf.Ceil(time / Time.fixedDeltaTime));
                yield return new WaitForSeconds(time / Mathf.Ceil(time / Time.fixedDeltaTime));
            }
            obj.localScale = target;
            Debug.Log("Change over.");
        } else {
            obj.localScale = target;
            Debug.Log("Time given is less than 0.1 seconds and is imperceptibly small; instantaneous change applied.");
        }
    }

    private void Reset() {
        flags.Clear();
        foreach(GameObject gameObject in gameObjects.Values) {
            Destroy(gameObject);
        }
        gameObjects.Clear();
        matrices.Clear();
    }

    public void Resume() {
        Reset();
        ExecuteAtTime(pauseTime);
    }

    public void Stop() {
        pauseTime = Time.time - startTime;
        executing = false;
        GetComponent<AudioSource>()?.Stop();
    }

    public void ChangeExecution() {
        if(!executing) {
            Resume();
        } else {
            Stop();
        }
    }

    public void ExecuteAtTime(float time) {
        line = 0;
        while(line < commands.Count && time >= commands[line].time) {
            Execute(line, time - commands[line].time);
            line++;
        }
        startTime = Time.time - pauseTime;
        executing = true;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

public class LessonReader : MonoBehaviour
{
    // for reading commands
    private string[] commands;
    
    // for generally keeping track of objects in the scene
    private Dictionary<string, bool> flags;
    private Dictionary<string, GameObject> gameObjects;
    private Dictionary<string, Matrix> matrices;

    // for WAIT-UNTIL
    private int savePoint;
    private string waitUntilType;
    private GameObject waitUntilObject1;
    private GameObject waitUntilObject2;
    private float waitUntilDistance;

    // for DRAW
    [Header("Geometry Assets")]
    public GameObject grid;
    public GameObject point;
    public GameObject vector;

    // Start is called before the first frame update
    void Start()
    {
        // Read the default lesson.
        StartNewLesson(System.IO.File.ReadAllLines("Assets/Lessons/" + Globals.lesson));
    }

    // For reading a lesson from a string.
    public void StartNewLesson(string userString)
    {
        List<string> lessonLines = new List<string>();
        using (System.IO.StringReader reader = new System.IO.StringReader(userString)) {
            lessonLines.Add(reader.ReadLine());
        }
        StartNewLesson(lessonLines.ToArray());
    }
    
    // For reading a lesson from a list of strings.
    public void StartNewLesson(string[] lessonLines)
    {
        commands = lessonLines;

        flags = new Dictionary<string, bool>();
        gameObjects = new Dictionary<string, GameObject>();
        matrices = new Dictionary<string, Matrix>();

        savePoint = 0;
        waitUntilType = null;
        waitUntilObject1 = null;
        waitUntilObject2 = null;
        waitUntilDistance = -1f;

        Execute(0);
    }

    // For executing a command line.
    // Line execution is supported through a series of regexes.
    void Execute(int index) {
        Debug.Log(string.Format("Executing line {0}", index));
        // Handle out of bounds exceptions.
        if(index >= commands.Length) {
            savePoint = -1;
            Debug.Log("Script execution terminated at end of file.");
            return;
        }

        savePoint = index;

        // Check for comment
        if(Regex.Match(commands[index], "^//[\\w-,;. ]+").Success) {
            Debug.Log("Comment: " + commands[index]);
            Execute(index + 1);
            return;
        // CREATE-OBJECT command: creates an object from assets, with either default name or new name
        } else if(Regex.Match(commands[index], "^CREATE-OBJECT \"[\\w\\-. ]+\"( AS \"[\\w\\-. ]+\")?$").Success) {
            Debug.Log("Creating game object.");
            string[] names = commands[index].Split("\"");

            try {
                // Instantiate corresponding asset from AssetDatabase.
                // Note that this is editor-only!
                string[] matchingAssets = AssetDatabase.FindAssets(names[1]);
                string dataPath = AssetDatabase.GUIDToAssetPath(matchingAssets[0]);
                GameObject obj = (GameObject)AssetDatabase.LoadAssetAtPath(dataPath, typeof(GameObject));
                GameObject newObj = Instantiate(obj);

                // Check for optional custom name.
                if(names.Length > 3) {
                    newObj.name = names[3];
                    gameObjects[names[3]] = newObj;
                } else {
                    gameObjects[names[1]] = newObj;
                }
            } catch {
                Debug.Log(string.Format("Asset \"{0}\" not found.", names[1]));
            }

            Execute(index + 1);
            return;
        // CREATE-MATRIX command: creates a matrix with given name and values
        } else if (Regex.Match(commands[index], "^CREATE-MATRIX \"[\\w\\- ]+\" \\[((-?[0-9]+(.[0-9]+)?)(,-?[0-9]+(.[0-9]+)?)*)(;(-?[0-9]+(.[0-9]+)?)(,-?[0-9]+(.[0-9]+)?)*)*\\]$").Success) {
            Debug.Log("Creating matrix.");
            // Obtain matrix elements
            string matrixElements = commands[index].Split(new char[] {'[', ']'})[1];
            string[] names = commands[index].Split("\"");
            try {
                // Attempt to create matrix
                matrices[names[1]] = new Matrix(matrixElements);
            } catch {
                Debug.Log("Error: inconsistent matrix size.");
            }

            Execute(index + 1);
            return;
        // DELETE-OBJECT command: deletes the specified game object
        } else if(Regex.Match(commands[index], "^DELETE-OBJECT \"[\\w\\-. ]+\"$").Success) {
            Debug.Log("Deleting game object.");
            string[] names = commands[index].Split("\"");
            // Check for object and destroy
            if(gameObjects.ContainsKey(names[1])) {
                Destroy(gameObjects[names[1]]);
            } else {
                Debug.Log(string.Format("No such object {0} detected.", names[1]));
            }

            Execute(index + 1);
            return;
        // WAIT command: waits for the given time
        } else if(Regex.Match(commands[index], "^WAIT [0-9]+(.[0-9]+)?$").Success) {
            Debug.Log("Waiting.");
            string[] scriptWords = commands[index].Split(" ");
            // Obtain time.
            float waitTime = float.Parse(scriptWords[1]);
            Debug.Log("Starting wait.");
            // Create new waiting coroutine, and wait.
            IEnumerator waitCoroutine = Wait(waitTime, index + 1);
            StartCoroutine(waitCoroutine);
            return;
        // WAIT-UNTIL command: stops control flow until an object either collides or gets within a certain distance to another object
        // Useful for things like checking interaction
        } else if(Regex.Match(commands[index], "^WAIT-UNTIL \"[\\w\\- ]+\" (COLLIDES|GETS-CLOSE [0-9]+(.[0-9]+)?) \"[\\w\\- ]+\"$").Success) {
            Debug.Log("Waiting until.");
            try {
                string[] names = commands[index].Split("\"");
                waitUntilObject1 = gameObjects[names[1]];
                waitUntilObject2 = gameObjects[names[3]];
                string[] keywords = names[2].Split(" ");
                waitUntilType = keywords[1];
                
                // Add interaction detection onto one of the two objects.
                InteractionDetection detect = waitUntilObject1.AddComponent<InteractionDetection>() as InteractionDetection;
                // Set interaction detection parameters.
                detect.interactionType = waitUntilType;
                detect.otherObject = waitUntilObject2;
                detect.lessonReader = this;
                if(waitUntilType.Equals("COLLIDES")) {
                    // Collision detection.
                    Debug.Log("Type: collision detection");
                } else if(waitUntilType.Equals("GETS-CLOSE")) {
                    // Proximity detection.
                    Debug.Log("Type: proximity detection");
                    detect.distanceThreshold = float.Parse(keywords[2]);;
                }

                savePoint = index + 1;
            } catch {
                Debug.Log(string.Format("Error in WAIT-UNTIL in command line {0}", index));
                Execute(index + 1);
            }
            return;
        // GOTO command: cedes control flow to a given line
        } else if(Regex.Match(commands[index], "^GOTO [0-9]+( IF \"[\\w-]+\" ELSE [0-9]+)?$").Success) {
            Debug.Log("Going to line.");
            string[] scriptWords = commands[index].Split(" ");
            try {
                // Get GOTO destination.
                int truePath = int.Parse(scriptWords[1]);
                if(scriptWords.Length > 2) {
                    string[] names = commands[index].Split("\"");
                    try {
                        // Check if flag is active.
                        bool value = flags[names[1]];
                        int falsePath = int.Parse(scriptWords[5]);
                        Execute(value ? truePath : falsePath);
                    } catch {
                        Debug.Log(string.Format("GOTO IF path detected, but invalid flag name or line provided in command line {0}", index));
                    }
                } else {
                    Execute(truePath);
                }
            } catch {
                Debug.Log(string.Format("Invalid goto line supplied in command line {0}", index));
                Debug.Log("All goto commands must be formatted like \"GOTO <int>\". For example, \"GOTO 10\".");
            }
            return;
        // ASSIGN-PROPERTY command: assigns a specific property to a given object, either instantaneously or over time
        } else if(Regex.Match(commands[index], "^ASSIGN-PROPERTY \"[\\w-. ]+\" \"[A-Z]+\" ((\\[(-?[0-9]+(.[0-9]+)?)((;|,)(-?[0-9]+(.[0-9]+)?))*\\])|(\"[\\w-.]+\"))( [0-9]+(.[0-9]+))?$").Success) {
            Debug.Log("Assigning property.");
            string[] names = commands[index].Split("\"");
            GameObject obj;
            // Check for object to assign property to
            if(gameObjects.ContainsKey(names[1])) {
                obj = gameObjects[names[1]];
            } else {
                Debug.Log(string.Format("No such object {0} detected.", names[1]));
                Execute(index + 1);
                return;
            }

            // If position is being changed...
            if(names[3].Equals("POS")) {
                Debug.Log("Changing position.");
                try {
                    string[] parsedString = commands[index].Split(new char[] {'[', ']'});
                    // Create new vector for position from command
                    string matrixElements = parsedString[1];
                    Matrix positionMatrix = new Matrix(matrixElements);
                    if(positionMatrix.getRows() != 1 || positionMatrix.getCols() != 3) {
                        Debug.Log("Error: position should be 1 row by 3 columns.");
                        throw new System.ArithmeticException();
                    }
                    float[,] pos = positionMatrix.values;
                    Vector3 targetPosition = new Vector3(pos[0,0], pos[0,1], pos[0,2]);
                    // Either directly change position, or change over time
                    if(!string.IsNullOrEmpty(parsedString[2])) {
                        Debug.Log("Movement time detected.");
                        IEnumerator posCoroutine = Pos(obj.transform, targetPosition, float.Parse(parsedString[2]), index + 1);
                        StartCoroutine(posCoroutine);
                    } else {
                        Debug.Log("No movement time detected.");
                        obj.transform.position = targetPosition;
                    }
                    Execute(index + 1);
                    return;
                } catch {
                    Debug.Log("Error: inconsistent matrix size.");
                }
            // If rotation is being changed...
            } else if(names[3].Equals("ROT")) {
                Debug.Log("Changing rotation.");
                try {
                    string[] parsedString = commands[index].Split(new char[] {'[', ']'});
                    // Create new vector for rotation from command
                    string matrixElements = parsedString[1];
                    Matrix rotationMatrix = new Matrix(matrixElements);
                    if(rotationMatrix.getRows() != 1 || rotationMatrix.getCols() != 3) {
                        Debug.Log("Error: rotation should be 1 row by 3 columns.");
                        throw new System.ArithmeticException();
                    }
                    float[,] rot = rotationMatrix.values;
                    Vector3 targetRotation = new Vector3(rot[0,0], rot[0,1], rot[0,2]);
                    // Either directly change rotation, or change over time
                    if(!string.IsNullOrEmpty(parsedString[2])) {
                        Debug.Log("Rotation time detected.");
                        IEnumerator rotCoroutine = Rot(obj.transform, targetRotation, float.Parse(parsedString[2]), index + 1);
                        StartCoroutine(rotCoroutine);
                    } else {
                        Debug.Log("No movement time detected.");
                        obj.transform.eulerAngles = targetRotation;
                    }
                    Execute(index + 1);
                    return;
                } catch {
                    Debug.Log("Error: inconsistent matrix size.");
                }
            //If scale is being changed...
            } else if(names[3].Equals("SCALE")) {
                Debug.Log("Changing scale.");
                try {
                    string matrixElements = commands[index].Split(new char[] {'[', ']'})[1];
                    // Create new vector for scale from command
                    Matrix scaleMatrix = new Matrix(matrixElements);
                    if(scaleMatrix.getRows() != 1 || scaleMatrix.getCols() != 3) {
                        Debug.Log("Error: scale should be 1 row by 3 columns.");
                        throw new System.ArithmeticException();
                    }
                    float[,] scale = scaleMatrix.values;
                    // Change scale
                    obj.transform.localScale = new Vector3(scale[0,0], scale[0,1], scale[0,2]);
                } catch {
                    Debug.Log("Error: inconsistent matrix size.");
                }
            // If no valid field is chosen, just skip command
            } else {
                Debug.Log(string.Format("Error: no valid property {0}", names[3]));
            }
            Execute(index + 1);
            return;
        // PLAY SOUND command: plays a given sound asset
        } else if(Regex.Match(commands[index], "^PLAY SOUND \"[\\w\\- ]+\"").Success) {
            Debug.Log("Playing sound.");
            string[] names = commands[index].Split("\"");
            try {
                // Obtain sound from AssetDatabase
                // Like with CREATE-OBJECT, this is editor-only
                string[] matchingAssets = AssetDatabase.FindAssets(names[1]);
                string dataPath = AssetDatabase.GUIDToAssetPath(matchingAssets[0]);
                AudioClip audio = (AudioClip)AssetDatabase.LoadAssetAtPath(dataPath, typeof(AudioClip));
                // Play audio
                GetComponent<AudioSource>().clip = audio;
                GetComponent<AudioSource>().Play();
            } catch {
                Debug.Log(string.Format("Sound clip \"{0}\" not found.", names[1]));
            }
            Execute(index + 1);
            return;
        // DRAW GRID command: draws built-in grid
        } else if(Regex.Match(commands[index], "^DRAW GRID \"[\\w\\- ]+\"").Success) {
            Debug.Log("Drawing grid.");
            string[] parsedString = commands[index].Split("\"");
            // Instantiate grid and put into gameObjects dictionary
            GameObject newGrid = Instantiate(grid);
            gameObjects[parsedString[1]] = newGrid;
            Execute(index + 1);
            return;
        // DRAW POINT command: draws built-in point on GRID
        } else if(Regex.Match(commands[index], "^DRAW POINT \"[\\w\\- ]+\" ON \"[\\w\\- ]+\"$").Success) {
            Debug.Log("Drawing point.");
            string[] parsedString = commands[index].Split("\"");
            // Instantiate point and put into gameObjects dictionary
            GameObject newPoint = Instantiate(point);
            gameObjects[parsedString[1]] = newPoint;
            try {
                // Set default values related to grid, on point's managers
                GameObject managerObj = gameObjects[parsedString[3]];
                newPoint.transform.parent = managerObj.transform;
                GridManager manager = managerObj.GetComponent<GridManager>();
                newPoint.GetComponent<PointManager>().gridManager = manager;
                newPoint.GetComponent<PointSnapConstraint>().origin = manager.origin;
                // Scale normalization
                newPoint.transform.localScale = new Vector3(1,1,1);
            } catch {
                Debug.Log("Error: could not find corresponding grid.");
            }
            Execute(index + 1);
            return;
        // DRAW VECTOR command: draws built-in vector from one POINT to another on the same GRID
        } else if(Regex.Match(commands[index], "^DRAW VECTOR \"[\\w\\- ]+\" FROM \"[\\w\\- ]+\" TO \"[\\w\\- ]+\"$").Success) {
            Debug.Log("Drawing vector.");
            string[] parsedString = commands[index].Split("\"");
            // Instantiate vector and put into gameObjects dictionary
            GameObject newVector = Instantiate(vector);
            gameObjects[parsedString[1]] = newVector;
            try {
                // Attempt to get both endpoints
                GameObject endpoint1 = gameObjects[parsedString[3]];
                GameObject endpoint2 = gameObjects[parsedString[5]];
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
                newVector.transform.localScale = new Vector3(10,1,10);
            } catch {
                Debug.Log("Error involving the supplied endpoints.");
            }
            Execute(index + 1);
            return;
        // APPLY-MATRIX command: applies a linear transformation as specified by a matrix to a given object
        } else if(Regex.Match(commands[index], "^APPLY-MATRIX \"[\\w\\- ]+\" TO \"[\\w\\-. ]+\"$").Success) {
            Debug.Log("Applying matrix transformation.");
            string[] names = commands[index].Split("\"");
            if(matrices.ContainsKey(names[1]) && gameObjects.ContainsKey(names[3])) {
                // Apply transformation to every mesh that is in the object
                foreach(MeshFilter filters in gameObjects[names[3]].GetComponentsInChildren<MeshFilter>()) {
                    Mesh mesh = filters.mesh;
                    Vector3[] vertices = mesh.vertices;
                    for(int i = 0; i < vertices.Length; i++) {
                        // Get the position (as a matrix) of the descendant vertex with respect to the base object
                        Vector3 wrtParent = filters.transform.TransformPoint(vertices[i]) - gameObjects[names[3]].transform.position;
                        float[,] vertexPosition = new float[3,1]{{wrtParent.x},{wrtParent.y},{wrtParent.z}};
                        Matrix vertexMatrix = new Matrix(vertexPosition);
                        // Transform the position to get the transformed position with respect to the base object
                        Matrix transformedMatrix = matrices[names[1]] * vertexMatrix;
                        Vector3 transformedWrtParent = new Vector3(transformedMatrix.values[0,0], transformedMatrix.values[1,0], transformedMatrix.values[2,0]);
                        // Convert the transformed position with respect to the base object back into descendant coordinates
                        Vector3 transformedPosition = filters.transform.InverseTransformPoint(transformedWrtParent + gameObjects[names[3]].transform.position);
                        vertices[i] = transformedPosition;
                    }
                    mesh.vertices = vertices;
                    mesh.RecalculateNormals();
                }
            } else {
                Debug.Log(string.Format("Either no matrix {0} or no object {1} detected.", names[1], names[3]));
            }
            Execute(index + 1);
        // Invalid command
        } else {
            Debug.Log(string.Format("No command, or invalid command, received at command line {0}.", index));
            Execute(index + 1);
        }
    }

    // For executing from a save point when control flow has paused (e.g. WAIT-UNTIL)
    public void ExecuteFromSavePoint() {
        waitUntilType = null;
        waitUntilObject1 = null;
        waitUntilObject2 = null;
        waitUntilDistance = -1;
        Execute(savePoint);
    }

    // Wait coroutine
    private IEnumerator Wait(float waitTime, int commandAfter) {
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Wait over.");
        Execute(commandAfter);
    }

    // Position changing coroutine
    private IEnumerator Pos(Transform obj, Vector3 target, float time, int commandAfter) {
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
    private IEnumerator Rot(Transform obj, Vector3 target, float time, int commandAfter)
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
}

// Custom matrix class
class Matrix
{
    public float[,] values;

    public int getRows() {
        return values.GetLength(0);
    }

    public int getCols() {
        return values.GetLength(1);
    }

    public Matrix(float[,] values) {
        if(values.Rank != 2) {
            Debug.Log("Invalid size of multidimensional array; array must be 2-dimensional.");
            throw new System.ArithmeticException();
        }
        this.values = values;
    }

    public Matrix(string input) {
        // Remove starting or trailing brackets
        if(input.IndexOf("[") == 0) {
            input = input.Substring(1);
        }
        if(input.IndexOf("]") == input.Length - 1) {
            input = input.Substring(0, input.Length - 1);
        }
        // Create empty 2D array
        string[] rows = input.Split(';');
        string[] referenceCols = rows[0].Split(",");
        float[,] values = new float[rows.Length, referenceCols.Length];
        // Parse each line, checking for inconsistent size
        for(int i = 0; i < rows.Length; i++) {
            string[] cols = rows[i].Split(",");
            if(cols.Length != referenceCols.Length) {
                Debug.Log("Matrix size mismatch.");
                throw new System.ArithmeticException();
            }
            for(int j = 0; j < cols.Length; j++) {
                values[i,j] = float.Parse(cols[j]);
            }
        }
        this.values = values;
    }

    // Get transpose of matrix
    public Matrix Transpose() {
        float[,] tValues = new float[getCols(), getRows()];
        for(int i = 0; i < getCols(); i++) {
            for(int j = 0; i < getRows(); j++) {
                tValues[i,j] = values[j,i];
            }
        }
        return new Matrix(tValues);
    }

    // Get the dot product of two one-dimensional matrices
    public static float Dot(Matrix a, Matrix b) {
        if(a.getCols() != 1 || b.getCols() != 1 || a.getRows() != b.getRows()) {
            Debug.Log("Vector size mismatch. Both vectors must be n rows by 1 column.");
            throw new System.ArithmeticException();
        }
        Matrix single = a.Transpose() * b;
        return single.values[0,0];
    }

    // Get the magnitude of a one-dimensional matrix
    public float Magnitude() {
        return Mathf.Sqrt(Dot(this, this));
    }

    // Get the projection of a one-dimensional matrix
    public static Matrix Proj(Matrix u, Matrix a) {
        return(Dot(u, a) / Dot(u, u)) * u;
    }

    // TO BE IMPLEMENTED: determinant through QR factorization

    // Standard unary matrix negation operator
    public static Matrix operator -(Matrix a) {
        float [,] result = new float[a.getRows(), a.getCols()];
        for(int i = 0; i < a.getRows(); i++) {
            for(int j = 0; j < a.getCols(); j++) {
                result[i,j] = -a.values[i,j];
            }
        }
        return new Matrix(result);
    }

    // Standard matrix addition operator
    public static Matrix operator +(Matrix a, Matrix b) {
        if(a.getRows() != b.getRows() || a.getCols() != b.getCols()) {
            Debug.Log("Invalid dimensions of added matrices.");
            throw new System.ArithmeticException();
        }

        float[,] result = new float[a.getRows(), a.getCols()];
        for(int i = 0; i < a.getRows(); i++) {
            for(int j = 0; j < b.getCols(); j++) {
                result[i,j] = a.values[i,j] + b.values[i,j];
            }
        }
        return new Matrix(result);
    }

    // Standard matrix subtraction operator
    public static Matrix operator -(Matrix a, Matrix b) {
        if(a.getRows() != b.getRows() || a.getCols() != b.getCols()) {
            Debug.Log("Invalid dimensions of added matrices.");
            throw new System.ArithmeticException();
        }

        float[,] result = new float[a.getRows(), a.getCols()];
        for(int i = 0; i < a.getRows(); i++) {
            for(int j = 0; j < b.getCols(); j++) {
                result[i,j] = a.values[i,j] - b.values[i,j];
            }
        }
        return new Matrix(result);
    }

    // Standard scalar-matrix multiplication operator
    public static Matrix operator *(float a, Matrix b) {
        float[,] result = new float[b.getRows(), b.getCols()];
        for(int i = 0; i < b.getRows(); i++) {
            for(int j = 0; j < b.getCols(); j++) {
                result[i,j] = a * b.values[i,j];
            }
        }
        return new Matrix(result);
    }

    // Standard matrix-scalar multiplication operator
    public static Matrix operator *(Matrix a, float b) {
        return b * a;
    }

    // Standard matrix-matrix multiplication operator
    public static Matrix operator *(Matrix a, Matrix b) {
        if(a.getCols() != b.getRows()) {
            Debug.Log("Invalid dimensions of multiplied matrices.");
            throw new System.ArithmeticException();
        }

        float[,] result = new float[a.getRows(),b.getCols()];
        for(int i = 0; i < a.getRows(); i++) {
            for(int j = 0; j < b.getCols(); j++) {
                float val = 0;
                for(int k = 0; k < a.getCols(); k++) {
                    val += a.values[i,k] * b.values[k,j];
                }
                result[i,j] = val;
            }
        }
        return new Matrix(result);
    }

    // Custom ToString command to better examine matrices
    public override string ToString() {
        string mainString = "[";
        for(int i = 0; i < this.getRows(); i++) {
            for(int j = 0; j < this.getCols(); j++) {
                mainString += values[i,j].ToString() + (j != this.getCols() - 1 ? "," : "");
            }
            mainString += (i != this.getRows() - 1 ? ";" : "");
        }
        mainString += "]";
        return mainString;
    }
}
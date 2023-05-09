using System;
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
  private Dictionary<string, List<Transformable>> gameObjects;
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
    TextAsset txt = (TextAsset)Resources.Load("lessons/lesson1", typeof(TextAsset));
    Debug.Log("Lesson loaded!!");
    // Read the default lesson.
    string[] lines = txt.text.Split(Environment.NewLine);
    StartNewLesson(lines);
  }

  // For reading a lesson from a string.
  public void StartNewLesson(string userString)
  {
    List<string> lessonLines = new List<string>();
    using (System.IO.StringReader reader = new System.IO.StringReader(userString))
    {
      lessonLines.Add(reader.ReadLine());
    }
    StartNewLesson(lessonLines.ToArray());
  }

  // For reading a lesson from a list of strings.
  public void StartNewLesson(string[] lessonLines)
  {
    commands = lessonLines;

    flags = new Dictionary<string, bool>();
    gameObjects = new Dictionary<string, List<Transformable>>();
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
  void Execute(int index)
  {
    Debug.Log(string.Format("Executing line {0}", index));
    // Handle out of bounds exceptions.
    if (index >= commands.Length)
    {
      savePoint = -1;
      Debug.Log("Script execution terminated at end of file.");
      return;
    }

    savePoint = index;

    // Check for comment
    if (Regex.Match(commands[index], "^//[\\w-,;. ]+").Success)
    {
      Debug.Log("Comment: " + commands[index]);
      Execute(index + 1);
      return;
      // CREATE-OBJECT command: creates an object from assets, with either default name or new name
    }
    else if (Regex.Match(commands[index], "^CREATE-OBJECT \"[\\w\\-. ]+\"( AS \"[\\w\\-. ]+\")?$").Success)
    {
      Debug.Log("Creating game object.");
      string[] names = commands[index].Split("\"");

      try
      {
        // Instantiate corresponding asset from AssetDatabase.
        // Note that this is editor-only!
#if Unity_Editor
        string[] matchingAssets = AssetDatabase.FindAssets(names[1]);

        string dataPath = AssetDatabase.GUIDToAssetPath(matchingAssets[0]);
        GameObject obj = (GameObject)AssetDatabase.LoadAssetAtPath(dataPath, typeof(GameObject));
        GameObject newObj = Instantiate(obj);

        // Check for optional custom name.
        if (names.Length > 3)
        {
          newObj.name = names[3];
          gameObjects[names[3]] = GridManager.createTransformable(newObj);
        }
        else
        {
          gameObjects[names[1]] = GridManager.createTransformable(newObj);
        }
#endif
      }
      catch
      {
        Debug.Log(string.Format("Asset \"{0}\" not found.", names[1]));
      }

      Execute(index + 1);
      return;
      // CREATE-MATRIX command: creates a matrix with given name and values
    }
    else if (Regex.Match(commands[index], "^CREATE-MATRIX \"[\\w\\- ]+\" \\[((-?[0-9]+(.[0-9]+)?)(,-?[0-9]+(.[0-9]+)?)*)(;(-?[0-9]+(.[0-9]+)?)(,-?[0-9]+(.[0-9]+)?)*)*\\]$").Success)
    {
      Debug.Log("Creating matrix.");
      // Obtain matrix elements
      string matrixElements = commands[index].Split(new char[] { '[', ']' })[1];
      string[] names = commands[index].Split("\"");
      try
      {
        // Attempt to create matrix
        matrices[names[1]] = new Matrix(matrixElements);
      }
      catch
      {
        Debug.Log("Error: inconsistent matrix size.");
      }

      Execute(index + 1);
      return;
      // DELETE-OBJECT command: deletes the specified game object
    }
    else if (Regex.Match(commands[index], "^DELETE-OBJECT \"[\\w\\-. ]+\"$").Success)
    {
      Debug.Log("Deleting game object.");
      string[] names = commands[index].Split("\"");
      // Check for object and destroy
      if (gameObjects.ContainsKey(names[1]))
      {
        Destroy(gameObjects[names[1]][0].gameObject);
      }
      else
      {
        Debug.Log(string.Format("No such object {0} detected.", names[1]));
      }

      Execute(index + 1);
      return;
      // WAIT command: waits for the given time
    }
    else if (Regex.Match(commands[index], "^WAIT [0-9]+(.[0-9]+)?$").Success)
    {
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
    }
    else if (Regex.Match(commands[index], "^WAIT-UNTIL \"[\\w\\- ]+\" (COLLIDES|GETS-CLOSE [0-9]+(.[0-9]+)?) \"[\\w\\- ]+\"$").Success)
    {
      Debug.Log("Waiting until.");
      try
      {
        string[] names = commands[index].Split("\"");
        waitUntilObject1 = gameObjects[names[1]][0].gameObject;
        waitUntilObject2 = gameObjects[names[3]][0].gameObject;
        string[] keywords = names[2].Split(" ");
        waitUntilType = keywords[1];

        // Add interaction detection onto one of the two objects.
        InteractionDetection detect = waitUntilObject1.AddComponent<InteractionDetection>() as InteractionDetection;
        // Set interaction detection parameters.
        detect.interactionType = waitUntilType;
        detect.otherObject = waitUntilObject2;
        detect.lessonReader = this;
        if (waitUntilType.Equals("COLLIDES"))
        {
          // Collision detection.
          Debug.Log("Type: collision detection");
        }
        else if (waitUntilType.Equals("GETS-CLOSE"))
        {
          // Proximity detection.
          Debug.Log("Type: proximity detection");
          detect.distanceThreshold = float.Parse(keywords[2]); ;
        }

        savePoint = index + 1;
      }
      catch
      {
        Debug.Log(string.Format("Error in WAIT-UNTIL in command line {0}", index));
        Execute(index + 1);
      }
      return;
      // GOTO command: cedes control flow to a given line
    }
    else if (Regex.Match(commands[index], "^GOTO [0-9]+( IF \"[\\w-]+\" ELSE [0-9]+)?$").Success)
    {
      Debug.Log("Going to line.");
      string[] scriptWords = commands[index].Split(" ");
      try
      {
        // Get GOTO destination.
        int truePath = int.Parse(scriptWords[1]);
        if (scriptWords.Length > 2)
        {
          string[] names = commands[index].Split("\"");
          try
          {
            // Check if flag is active.
            bool value = flags[names[1]];
            int falsePath = int.Parse(scriptWords[5]);
            Execute(value ? truePath : falsePath);
          }
          catch
          {
            Debug.Log(string.Format("GOTO IF path detected, but invalid flag name or line provided in command line {0}", index));
          }
        }
        else
        {
          Execute(truePath);
        }
      }
      catch
      {
        Debug.Log(string.Format("Invalid goto line supplied in command line {0}", index));
        Debug.Log("All goto commands must be formatted like \"GOTO <int>\". For example, \"GOTO 10\".");
      }
      return;
      // ASSIGN-PROPERTY command: assigns a specific property to a given object, either instantaneously or over time
    }
    else if (Regex.Match(commands[index], "^ASSIGN-PROPERTY \"[\\w-. ]+\" \"[A-Z]+\" ((\\[(-?[0-9]+(.[0-9]+)?)((;|,)(-?[0-9]+(.[0-9]+)?))*\\])|(\"[\\w-.]+\"))( [0-9]+(.[0-9]+))?$").Success)
    {
      Debug.Log("Assigning property.");
      string[] names = commands[index].Split("\"");
      List<Transformable> objTransformables;
      // Check for object to assign property to
      if (gameObjects.ContainsKey(names[1]))
      {
        objTransformables = gameObjects[names[1]];
      }
      else
      {
        Debug.Log(string.Format("No such object {0} detected.", names[1]));
        Execute(index + 1);
        return;
      }

      // If position is being changed...
      if (names[3].Equals("POS"))
      {
        Debug.Log("Changing position.");
        try
        {
          string[] parsedString = commands[index].Split(new char[] { '[', ']' });
          // Create new vector for position from command
          string matrixElements = parsedString[1];
          Matrix positionMatrix = new Matrix(matrixElements);
          if (positionMatrix.getRows() != 1 || positionMatrix.getCols() != 3)
          {
            Debug.Log("Error: position should be 1 row by 3 columns.");
            throw new System.ArithmeticException();
          }
          float[,] pos = positionMatrix.values;
          Vector3 targetPosition = new Vector3(pos[0, 0], pos[0, 1], pos[0, 2]);
          // Either directly change position, or change over time
          if (!string.IsNullOrEmpty(parsedString[2]))
          {
            Debug.Log("Movement time detected.");
            // Only support animating the first transformable, for simplicity
            IEnumerator posCoroutine = Pos(objTransformables[0], targetPosition, float.Parse(parsedString[2]), index + 1);
            StartCoroutine(posCoroutine);
          }
          else
          {
            Debug.Log("No movement time detected.");
            foreach (Transformable transformable in objTransformables)
            {
              transformable.SetStandardValue(targetPosition);
            }
          }
          Execute(index + 1);
          return;
        }
        catch (Exception e)
        {
          Debug.Log("Error: inconsistent matrix size.");
          Debug.Log($"Stack Trace: {e.StackTrace}");
          Debug.Log($"Message: {e.Message}");
          throw;
        }
        // If rotation is being changed...
      }
      else if (names[3].Equals("ROT"))
      {
        Debug.Log("Changing rotation.");
        try
        {
          string[] parsedString = commands[index].Split(new char[] { '[', ']' });
          // Create new vector for rotation from command
          string matrixElements = parsedString[1];
          Matrix rotationMatrix = new Matrix(matrixElements);
          if (rotationMatrix.getRows() != 1 || rotationMatrix.getCols() != 3)
          {
            Debug.Log("Error: rotation should be 1 row by 3 columns.");
            throw new System.ArithmeticException();
          }
          float[,] rot = rotationMatrix.values;
          Vector3 targetRotation = new Vector3(rot[0, 0], rot[0, 1], rot[0, 2]);
          // Either directly change rotation, or change over time
          if (!string.IsNullOrEmpty(parsedString[2]))
          {
            Debug.Log("Rotation time detected.");
            // Only support animating the first transformable, for simplicity
            // Also unknown effects, this should be avoided for transformable objects...
            IEnumerator rotCoroutine = Rot(objTransformables[0].gameObject.transform, targetRotation, float.Parse(parsedString[2]), index + 1);
            StartCoroutine(rotCoroutine);
          }
          else
          {
            Debug.Log("No movement time detected.");
            // Unknown effects, this should not be used for transformable objects...
            foreach (Transformable transformable in objTransformables)
            {
              transformable.gameObject.transform.eulerAngles = targetRotation;
            }
          }
          Execute(index + 1);
          return;
        }
        catch
        {
          Debug.Log("Error: inconsistent matrix size.");
        }
        //If scale is being changed...
      }
      else if (names[3].Equals("SCALE"))
      {
        Debug.Log("Changing scale.");
        try
        {
          string matrixElements = commands[index].Split(new char[] { '[', ']' })[1];
          // Create new vector for scale from command
          Matrix scaleMatrix = new Matrix(matrixElements);
          if (scaleMatrix.getRows() != 1 || scaleMatrix.getCols() != 3)
          {
            Debug.Log("Error: scale should be 1 row by 3 columns.");
            throw new System.ArithmeticException();
          }
          float[,] scale = scaleMatrix.values;
          // Change scale
          // Unknown effects, this should be avoided for transformable objects...
          Vector3 targetLocalScale = new Vector3(scale[0, 0], scale[0, 1], scale[0, 2]);
          foreach (Transformable transformable in objTransformables)
          {
            transformable.gameObject.transform.localScale = targetLocalScale;
          }
        }
        catch
        {
          Debug.Log("Error: inconsistent matrix size.");
        }
        // If no valid field is chosen, just skip command
      }
      else
      {
        Debug.Log(string.Format("Error: no valid property {0}", names[3]));
      }
      Execute(index + 1);
      return;
      // PLAY SOUND command: plays a given sound asset
    }
    else if (Regex.Match(commands[index], "^PLAY SOUND \"[\\w\\- ]+\"").Success)
    {
      Debug.Log("Playing sound.");
      string[] names = commands[index].Split("\"");
      try
      {
        // Obtain sound from AssetDatabase
        // Like with CREATE-OBJECT, this is editor-only
#if UNITY_EDITOR
        string[] matchingAssets = AssetDatabase.FindAssets(names[1]);
        string dataPath = AssetDatabase.GUIDToAssetPath(matchingAssets[0]);
        // Play audio
#endif
        AudioClip audio = (AudioClip)Resources.Load("lessons/" + names[1]);
        GetComponent<AudioSource>().clip = audio;
        GetComponent<AudioSource>().Play();
      }
      catch
      {
        Debug.Log(string.Format("Sound clip \"{0}\" not found.", names[1]));
      }
      Execute(index + 1);
      return;
      // DRAW GRID command: draws built-in grid
    }
    else if (Regex.Match(commands[index], "^DRAW GRID \"[\\w\\- ]+\"").Success)
    {
      Debug.Log("Drawing grid.");
      string[] parsedString = commands[index].Split("\"");
      // Instantiate grid and put into gameObjects dictionary
      GameObject newGrid = Instantiate(grid);
      gameObjects[parsedString[1]] = new List<Transformable>
            {
                new Transformable.MPartialGridManager(newGrid, newGrid.GetComponent<GridManager>())
            };
      Execute(index + 1);
      return;
      // DRAW POINT command: draws built-in point on GRID
    }
    else if (Regex.Match(commands[index], "^DRAW POINT \"[\\w\\- ]+\" ON \"[\\w\\- ]+\"$").Success)
    {
      Debug.Log("Drawing point.");
      string[] parsedString = commands[index].Split("\"");
      // Instantiate point and put into gameObjects dictionary
      GameObject newPoint = Instantiate(point);
      PointManager newPointManager = newPoint.GetComponent<PointManager>();
      PointSnapConstraint newPointSnapConstraint = newPoint.GetComponent<PointSnapConstraint>();
      gameObjects[parsedString[1]] = new List<Transformable>
            {
                new Transformable.MPointManager(newPointManager),
                new Transformable.MPointSnapConstraint(newPointSnapConstraint)
            };
      try
      {
        // Set default values related to grid, on point's managers
        GridManager manager = ((Transformable.MPartialGridManager)gameObjects[parsedString[3]][0]).GridManager;
        GameObject managerObj = manager.gameObject;
        newPoint.transform.SetParent(managerObj.transform.GetChild(0), worldPositionStays: false); // Make centimeter scaler the parent
        newPointManager.RefreshGridManager(manager);
        newPointSnapConstraint.origin = manager.origin;
        // Scale normalization
        // newPoint.transform.localScale = new Vector3(1,1,1);
        // newPoint.transform.localScale = new Vector3(10,10,10);
      }
      catch
      {
        Debug.Log("Error: could not find corresponding grid.");
      }
      Execute(index + 1);
      return;
      // DRAW VECTOR command: draws built-in vector from one POINT to another on the same GRID
    }
    else if (Regex.Match(commands[index], "^DRAW VECTOR \"[\\w\\- ]+\" FROM \"[\\w\\- ]+\" TO \"[\\w\\- ]+\"$").Success)
    {
      Debug.Log("Drawing vector.");
      string[] parsedString = commands[index].Split("\"");
      // Instantiate vector and put into gameObjects dictionary
      GameObject newVector = Instantiate(vector);
      VectorManager newVectorManager = newVector.GetComponent<VectorManager>();
      gameObjects[parsedString[1]] = new List<Transformable>
            {
                new Transformable.MVectorManager(newVectorManager)
            };
      try
      {
        // Attempt to get both endpoints
        GameObject endpoint1 = gameObjects[parsedString[3]][0].gameObject;
        GameObject endpoint2 = gameObjects[parsedString[5]][0].gameObject;
        // Check if they are on the same grid
        if (endpoint1.GetComponent<PointManager>().gridManager != endpoint2.GetComponent<PointManager>().gridManager)
        {
          Debug.Log("You must select two endpoints that are on the same grid.");
          throw new System.ArithmeticException();
        }
        // Set default values related to grid, on vector's managers
        VectorEndpointConstraint constraint = newVector.GetComponent<VectorEndpointConstraint>();
        GridManager manager = endpoint1.GetComponent<PointManager>().gridManager;
        constraint.from = endpoint1;
        constraint.to = endpoint2;
        newVectorManager.gridManager = manager;
        constraint.gridManager = manager;
        newVector.transform.SetParent(manager.transform.GetChild(0), worldPositionStays: false); // Make centimeter scaler the parent
                                                                                                 // Scale normalization
                                                                                                 // newVector.transform.localScale = new Vector3(10,1,10);
      }
      catch
      {
        Debug.Log("Error involving the supplied endpoints.");
      }
      Execute(index + 1);
      return;
      // APPLY-MATRIX command: applies a linear transformation as specified by a matrix to a given object
    }
    else if (Regex.Match(commands[index], "^APPLY-MATRIX \"[\\w\\- ]+\" TO \"[\\w\\-. ]+\"$").Success)
    {
      Debug.Log("Applying matrix transformation.");
      string[] names = commands[index].Split("\"");
      if (matrices.ContainsKey(names[1]) && gameObjects.ContainsKey(names[3]))
      {

        Matrix matrixToApply = matrices[names[1]];

        // Only support applying matrix to the first transformable, for simplicity
        Transformable transformable = gameObjects[names[3]][0];
        switch (transformable)
        {
          case Transformable.MPartialGridManager partialGridManager:
            GridManager gridManager = partialGridManager.GridManager;

            foreach (VectorManager vectorManager in gridManager.GetVectorManagers())
            {
              Matrix result = matrixToApply * new Matrix(vectorManager.standardValue);
              vectorManager.SetNewStandardValue(new Vector3(result.values[0, 0], result.values[1, 0], result.values[2, 0]));
            }

            break;
          case Transformable.NotTransformable notTransformable:
            // Apply transformation to every mesh that is in the object
            GameObject gameObject = notTransformable.GameObject;
            foreach (MeshFilter filters in gameObject.GetComponentsInChildren<MeshFilter>())
            {
              Mesh mesh = filters.mesh;
              Vector3[] vertices = mesh.vertices;
              for (int i = 0; i < vertices.Length; i++)
              {
                // Get the position (as a matrix) of the descendant vertex with respect to the base object
                Vector3 wrtParent = filters.transform.TransformPoint(vertices[i]) - gameObject.transform.position;
                float[,] vertexPosition = new float[3, 1] { { wrtParent.x }, { wrtParent.y }, { wrtParent.z } };
                Matrix vertexMatrix = new Matrix(vertexPosition);
                // Transform the position to get the transformed position with respect to the base object
                Matrix transformedMatrix = matrixToApply * vertexMatrix;
                Vector3 transformedWrtParent = new Vector3(transformedMatrix.values[0, 0], transformedMatrix.values[1, 0], transformedMatrix.values[2, 0]);
                // Convert the transformed position with respect to the base object back into descendant coordinates
                Vector3 transformedPosition = filters.transform.InverseTransformPoint(transformedWrtParent + gameObject.transform.position);
                vertices[i] = transformedPosition;
              }
              mesh.vertices = vertices;
              mesh.RecalculateNormals();
            }
            break;
          default:
            Debug.Log("Matrix not supported on this object");
            break;
        }
      }
      else
      {
        Debug.Log(string.Format("Either no matrix {0} or no object {1} detected.", names[1], names[3]));
      }
      Execute(index + 1);
      // Invalid command
    }
    else
    {
      Debug.Log(commands[index]);
      Debug.Log(string.Format("No command, or invalid command, received at command line {0}.", index));
      Execute(index + 1);
    }
  }

  // For executing from a save point when control flow has paused (e.g. WAIT-UNTIL)
  public void ExecuteFromSavePoint()
  {
    waitUntilType = null;
    waitUntilObject1 = null;
    waitUntilObject2 = null;
    waitUntilDistance = -1;
    Execute(savePoint);
  }

  // Wait coroutine
  private IEnumerator Wait(float waitTime, int commandAfter)
  {
    yield return new WaitForSeconds(waitTime);
    Debug.Log("Wait over.");
    Execute(commandAfter);
  }

  // Position changing coroutine
  private IEnumerator Pos(Transformable transformable, Vector3 target, float time, int commandAfter)
  {
    if (time > 0.1f)
    {
      Vector3 initPosition = transformable.GetStandardValue();
      Debug.Log("Changing position.");
      for (int i = 0; i < Mathf.Ceil(time / Time.fixedDeltaTime); i++)
      {
        transformable.SetStandardValue(Vector3.Lerp(initPosition, target, i / Mathf.Ceil(time / Time.fixedDeltaTime)));
        yield return new WaitForSeconds(time / Mathf.Ceil(time / Time.fixedDeltaTime));
      }
      transformable.SetStandardValue(target);
      Debug.Log("Change over.");
    }
    else
    {
      transformable.SetStandardValue(target);
      Debug.Log("Time given is less than 0.1 seconds and is imperceptibly small; instantaneous change applied.");
    }
  }

  // Rotation changing coroutine
  private IEnumerator Rot(Transform obj, Vector3 target, float time, int commandAfter)
  {
    bool useSlerp = true;
    if (time > 0.1f)
    {
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
    }
    else
    {
      obj.eulerAngles = target;
      Debug.Log("Time given is less than 0.1 seconds and is imperceptibly small; instantaneous change applied.");
    }
  }
}
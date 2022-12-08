using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public class LessonReader : MonoBehaviour
{
    public string[] commands;
    
    private Dictionary<string, bool> flags;
    private Dictionary<string, Object> objects;
    private int savePoint;

    // Start is called before the first frame update
    void Start()
    {
        flags = new Dictionary<string, bool>();
        Execute(0);
    }

    void Execute(int index) {
        // Handle out of bounds exceptions.
        if(index >= commands.Length) {
            Debug.Log("Script execution terminated at end of file.");
            return;
        }

        savePoint = index;
        string[] scriptWords = commands[index].Split(" ", StringSplitOptions.RemoveEmptyEntries/* | StringSplitOptions.TrimEntries */);

        // DEBUG: print out all parsed words in sentence.
        for(int i = 0; i < scriptWords.Length; i++) {
            Debug.Log(scriptWords[i]);
        }

        if(scriptWords[0].Equals("CREATE-OBJECT")) {
            Debug.Log("Creating.");
            
            string[] names = commands[index].Split("\"", StringSplitOptions.RemoveEmptyEntries/* | StringSplitOptions.TrimEntries */);
            for(int i = 0; i < names.Length; i++) {
                Debug.Log(names[i]);
            }

            string[] matchingAssets = AssetDatabase.FindAssets(names[1]);
            string dataPath = AssetDatabase.GUIDToAssetPath(matchingAssets[0]);
            GameObject obj = (GameObject)AssetDatabase.LoadAssetAtPath(dataPath, typeof(GameObject));
            GameObject newObj = Instantiate(obj);

            int j = 1;
            while(j < names.Length) {
                if(!string.IsNullOrWhiteSpace(names[j])) {
                    newObj.name = names[j];
                    break;
                }
                j++;
            }

            Execute(index + 1);
        } else if(scriptWords[0].Equals("DELETE-OBJECT")) {
            Debug.Log("Deleting.");

            string[] names = commands[index].Split("\"", StringSplitOptions.RemoveEmptyEntries/* | StringSplitOptions.TrimEntries */);
            for(int i = 0; i < names.Length; i++) {
                Debug.Log(names[i]);
            }
            GameObject obj = GameObject.Find(names[1]);
            if(obj) {
                Destroy(obj);
            }

            Execute(index + 1);
        } else if(scriptWords[0].Equals("WAIT")) {
            Debug.Log("Waiting.");
            try {
                float waitTime = float.Parse(scriptWords[1]);
                Debug.Log("Starting wait.");
                IEnumerator waitCoroutine = Wait(waitTime, index + 1);
                StartCoroutine(waitCoroutine);
            } catch {
                Debug.Log(string.Format("Invalid wait time supplied in command line {0}.", index));
                Debug.Log("All wait commands must be formatted like \"WAIT <float>\". For example, \"WAIT 24.1\".");
            }
        } else if(scriptWords[0].Equals("WAIT-UNTIL")) {
            savePoint = index + 1;
            Debug.Log("WAIT UNTIL");
        } else if(scriptWords[0].Equals("GOTO")) {
            Debug.Log("GOTO");
            try {
                int gotoIndex = int.Parse(scriptWords[1]);
                if(scriptWords.Length >= 5) {
                    try {
                        bool value = flags[scriptWords[2]];
                        int truePath = int.Parse(scriptWords[3]);
                        int falsePath = int.Parse(scriptWords[4]);
                        Execute(value ? truePath : falsePath);
                    } catch {
                        Debug.Log(string.Format("goto if path detected, but invalid command line choices provided in command line {0}", index));
                        Debug.Log("All goto if commands must be formatted like \"GOTO <int> <value> <path if true> <path if false>\"");
                    }
                } else {
                    Execute(gotoIndex);
                }
            } catch {
                Debug.Log(string.Format("Invalid goto line supplied in command line {0}", index));
                Debug.Log("All goto commands must be formatted like \"GOTO <int>\". For example, \"GOTO 10\".");
            }
        } else if (scriptWords[0].Equals("MATRIX")) {
            try {
                string[] terms = commands[index].Split("\"", StringSplitOptions.RemoveEmptyEntries/* | StringSplitOptions.TrimEntries */);
                string[] matrixElements = terms[2].Split(new char[] {'[', ']'}, StringSplitOptions.RemoveEmptyEntries/* | StringSplitOptions.TrimEntries */);
                
            } catch {
                Debug.Log(string.Format("Invalid 3 x 3 matrix supplied in command line {0}", index));
                Debug.Log("All such matrices must be formatted like [A, B, C; D, E, F; G, H, I]");
            }
        } else {
            Debug.Log(string.Format("No command, or invalid command, received at command line {0}.", index));
            Execute(index + 1);
        }
    }

    // Not quite sure what to use this for yet
    public void SetValueAndExecuteFromSavePoint(string key, bool value) {
        flags[key] = value;
        Execute(savePoint);
    }
    
    private IEnumerator Wait(float waitTime, int commandAfter) {
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Wait over.");
        Execute(commandAfter);
    }
}

class Matrix
{
    public float[,] values;
    public int rows;
    public int cols;

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
        rows = values.GetLength(0);
        cols = values.GetLength(1);
    }

    public static Matrix operator +(Matrix a, Matrix b) {
        if(a.rows != b.rows || a.cols != b.cols) {
            Debug.Log("Invalid dimensions of added matrices.");
            throw new System.ArithmeticException();
        }

        float[,] result = new float[a.rows, a.cols];
        for(int i = 0; i < a.rows; i++) {
            for(int j = 0; j < b.cols; j++) {
                result[i,j] = a.values[i,j] + b.values[i,j];
            }
        }

        return new Matrix(result);
    }

    public static Matrix operator *(Matrix a, Matrix b) {
        if(a.cols != b.rows) {
            Debug.Log("Invalid dimensions of multiplied matrices.");
            throw new System.ArithmeticException();
        }

        float[,] result = new float[a.rows,b.cols];
        for(int i = 0; i < a.rows; i++) {
            for(int j = 0; j < b.cols; j++) {
                float val = 0;
                for(int k = 0; k < a.cols; k++) {
                    val += a.values[i,k] * b.values[k,j];
                }
                result[i,j] = val;
            }
        }
        return new Matrix(result);
    }
}
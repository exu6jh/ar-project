using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

public class LessonReader : MonoBehaviour
{
    public string filename;
    private string[] commands;
    
    private Dictionary<string, bool> flags;
    private Dictionary<string, GameObject> gameObjects;
    private Dictionary<string, Matrix> matrices;

    private string savePhrase;
    private int savePoint;

    // Start is called before the first frame update
    void Start()
    {
        commands = System.IO.File.ReadAllLines(filename);

        flags = new Dictionary<string, bool>();
        gameObjects = new Dictionary<string, GameObject>();
        matrices = new Dictionary<string, Matrix>();
        Execute(0);
    }

    void Execute(int index) {
        // Handle out of bounds exceptions.
        if(index >= commands.Length) {
            savePoint = -1;
            Debug.Log("Script execution terminated at end of file.");
            return;
        }

        savePoint = index;

        if(Regex.Match(commands[index], "^CREATE-OBJECT \"[\\w\\-. ]+\"( \"[\\w\\-. ]+\")?$").Success) {
            Debug.Log("Creating.");
            string[] names = commands[index].Split("\"");

            try {
                string[] matchingAssets = AssetDatabase.FindAssets(names[1]);
                string dataPath = AssetDatabase.GUIDToAssetPath(matchingAssets[0]);
                GameObject obj = (GameObject)AssetDatabase.LoadAssetAtPath(dataPath, typeof(GameObject));
                GameObject newObj = Instantiate(obj);

                if(names.Length > 2) {
                    newObj.name = names[3];
                    gameObjects[names[3]] = newObj;
                } else {
                    gameObjects[names[1]] = newObj;
                }
            } catch {
                Debug.Log(string.Format("Asset \"{0}\" not found.", names[1]));
            }

            Execute(index + 1);
        } else if (Regex.Match(commands[index], "^CREATE-MATRIX \"[\\w\\- ]+\" \\[(([0-9]+(.[0-9]+)?)(,[0-9]+(.[0-9]+)?)*)(;([0-9]+(.[0-9]+)?)(,[0-9]+(.[0-9]+)?)*)*\\]$").Success) {
            try {
                string matrixElements = commands[index].Split(new char[] {'[', ']'})[1];
                string[] rows = matrixElements.Split(';');
                string[] referenceCols = rows[0].Split(",");
                float[,] values = new float[rows.Length, referenceCols.Length];
                for(int i = 0; i < rows.Length; i++) {
                    string[] cols = rows[i].Split(",");
                    if(cols.Length != referenceCols.Length) {
                        Debug.Log("Length mismatch.");
                        throw new System.ArithmeticException();
                    }
                    for(int j = 0; j < cols.Length; j++) {
                        values[i,j] = float.Parse(cols[j]);
                    }
                }
                string[] names = commands[index].Split("\"");
                matrices[names[1]] = new Matrix(values);
            } catch {
                Debug.Log(string.Format("Invalid 3 x 3 matrix supplied in command line {0}", index));
                Debug.Log("All such matrices must be formatted like [A, B, C; D, E, F; G, H, I]");
            }

            Execute(index + 1);
        } else if(Regex.Match(commands[index], "^DELETE-OBJECT \"[\\w\\-. ]+\"$").Success) {
            Debug.Log("Deleting.");
            string[] names = commands[index].Split("\"");
            if(gameObjects.ContainsKey(names[1])) {
                Destroy(gameObjects[names[1]]);
            } else {
                Debug.Log(string.Format("No such object {0} detected.", names[1]));
            }

            Execute(index + 1);
        } else if(Regex.Match(commands[index], "^WAIT [0-9]+(.[0-9]+)?$").Success) {
            Debug.Log("Waiting.");
            try {
                string[] scriptWords = commands[index].Split(" ");
                float waitTime = float.Parse(scriptWords[1]);
                Debug.Log("Starting wait.");
                IEnumerator waitCoroutine = Wait(waitTime, index + 1);
                StartCoroutine(waitCoroutine);
            } catch {
                Debug.Log(string.Format("Problem parsing wait time in command line {0}.", index));
            }
        } else if(Regex.Match(commands[index], "^WAIT-UNTIL \"[\\w\\- ]+\"$").Success) {
            Debug.Log("WAIT-UNTIL");
            string[] names = commands[index].Split("\"");
            savePhrase = names[1];
            savePoint = index + 1;
        } else if(Regex.Match(commands[index], "^GOTO [0-9]+( IF \"[\\w-]+\" ELSE [0-9]+)?$").Success) {
            Debug.Log("GOTO");
            string[] scriptWords = commands[index].Split(" ");
            try {
                int truePath = int.Parse(scriptWords[1]);
                if(scriptWords.Length > 2) {
                    string[] names = commands[index].Split("\"");
                    try {
                        bool value = flags[names[1]];
                        int falsePath = int.Parse(scriptWords[5]);
                        Execute(value ? truePath : falsePath);
                    } catch {
                        Debug.Log(string.Format("goto if path detected, but invalid command line choices provided in command line {0}", index));
                        Debug.Log("All goto if commands must be formatted like \"GOTO <int> <value> <path if true> <path if false>\"");
                    }
                } else {
                    Execute(truePath);
                }
            } catch {
                Debug.Log(string.Format("Invalid goto line supplied in command line {0}", index));
                Debug.Log("All goto commands must be formatted like \"GOTO <int>\". For example, \"GOTO 10\".");
            }
        } else if(Regex.Match(commands[index], "^ASSIGN-PROPERTY \"[\\w-. ]+\" [A-Z]+ ((\\[([0-9]+(.[0-9]+)?)((;|,)([0-9]+(.[0-9]+)?))*\\])|(\"[\\w-.]+\"))$").Success) {
            Debug.Log("TODO");
        } else {
            Debug.Log(string.Format("No command, or invalid command, received at command line {0}.", index));
            Execute(index + 1);
        }
    }

    // Not quite sure what to use this for yet
    public void ExecuteFromSavePoint(string password) {
        if(password.Equals(savePhrase)) {
            Debug.Log("Executing from save point.");
            Execute(savePoint);
        } else {
            Debug.Log("Invalid signal.");
        }
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

    public static Matrix operator -(Matrix a) {
        float [,] result = new float[a.getRows(), a.getCols()];
        for(int i = 0; i < a.getRows(); i++) {
            for(int j = 0; j < a.getCols(); j++) {
                result[i,j] = -a.values[i,j];
            }
        }
        return new Matrix(result);
    }

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
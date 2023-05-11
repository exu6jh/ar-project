using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ActionRepository : MonoBehaviour
{
    private List<ActionHolder> actions;

    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log(Globals.lessonCreate);
        readFromJsonFile(Globals.lessonCreate);
    }
    
    private void readFromPlaintextFile(string filename) {
        string[] actionList = System.IO.File.ReadAllLines(filename);
        actions = new List<ActionHolder>();
        for(int i = 0; i < actionList.Length; i++) {
            string[] sections = actionList[i].Split(new char[]{'[',']'});
            try {
                OldActionHolder newAction = new OldActionHolder(ParseTimestamp(sections[1]), sections[2].Trim(new char[]{' '}));
                actions.Add(ActionHolderFromOld(newAction));
            } catch {
                Debug.Log(string.Format("Error parsing line {0}", i));
            }
        }
    }

    private void readFromCommandFile(string filename) {
        string[] lines = System.IO.File.ReadAllLines(filename);
        string json = string.Join("", lines);
        OldActionHolder[] oldList = Newtonsoft.Json.JsonConvert.DeserializeObject<OldActionHolder[]>(json);
        actions = new List<ActionHolder>();
        for(int i = 0; i < oldList.Length; i++) {
            Debug.Log(oldList[i].command);
            actions.Add(ActionHolderFromOld(oldList[i]));
        }
    }

    private void readFromJsonFile(string filename) {
        try {
            Debug.Log("Reading from file " + filename + ".");
            string[] lines = System.IO.File.ReadAllLines(filename);
            string json = string.Join("", lines);
            actions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ActionHolder>>(json);
            if(actions == null) {
                actions = new List<ActionHolder>();
            }
        } catch {
            Debug.Log("No such file found. Creating file.");
            File.WriteAllText(filename, "");
            actions = new List<ActionHolder>();
        }
    }

    private ActionHolder ActionHolderFromOld(OldActionHolder old) {
        float time = old.time;
        string command = old.command;
        if(Regex.Match(command, "^CREATE-OBJECT \"[\\w\\-. ]+\"( AS \"[\\w\\-. ]+\")?$").Success) {
            string[] names = command.Split("\"");
            if(names.Length > 3) {
                return new ActionHolder(time, CommandType.CreateObject, names[3], null, names[1], null, null, 0f);
            } else {
                return new ActionHolder(time, CommandType.CreateObject, null, null, names[1], null, null, 0f);
            }
        } else if (Regex.Match(command, "^CREATE-MATRIX \"[\\w\\- ]+\" {((-?[0-9]+(.[0-9]+)?)(,-?[0-9]+(.[0-9]+)?)*)(;(-?[0-9]+(.[0-9]+)?)(,-?[0-9]+(.[0-9]+)?)*)*}$").Success) {
            // Obtain matrix elements
            string matrixElements = command.Split(new char[] {'{', '}'})[1];
            string[] names = command.Split("\"");
            try {
                // Attempt to create matrix
                Matrix mat = new Matrix(matrixElements);
                return new ActionHolder(time, CommandType.CreateMatrix, names[1], null, null, null, mat.values, 0f);
            } catch {
                Debug.Log("Error: inconsistent matrix size when loading from command JSON document.");
                return null;
            }
        } else if(Regex.Match(command, "^DELETE-OBJECT \"[\\w\\-. ]+\"$").Success) {
            string[] names = command.Split("\"");
            return new ActionHolder(time, CommandType.DeleteObject, names[1], null, null, null, null, 0f);
        } else if(Regex.Match(command, "^ASSIGN-PROPERTY \"[\\w-. ]+\" \"[A-Z]+\" ({(-?[0-9]+(.[0-9]+)?)((;|,)(-?[0-9]+(.[0-9]+)?))*})( [0-9]+(.[0-9]+))?$").Success) {
            string[] names = command.Split("\"");
            string[] parsedString = command.Split(new char[] {'{', '}'});
            float movementTime = 0f;
            if(!string.IsNullOrEmpty(parsedString[2])) {
                movementTime = float.Parse(parsedString[2]);
            }
            try {
                string matrixElements = parsedString[1];
                Matrix mat = new Matrix(matrixElements);
                return new ActionHolder(time, CommandType.AssignProperty, names[1], null, null, names[3], mat.values, movementTime);
            } catch {
                Debug.Log("Error: inconsistent matrix size when loading from command JSON document.");
                return null;
            }
        } else if(Regex.Match(command, "^PLAY SOUND \"[\\w\\- ]+\"").Success) {
            string[] names = command.Split("\"");
            return new ActionHolder(time, CommandType.PlaySound, null, null, names[1], null, null, 0f);
        } else if(Regex.Match(command, "^DRAW GRID \"[\\w\\- ]+\"").Success) {
            string[] parsedString = command.Split("\"");
            return new ActionHolder(time, CommandType.DrawGrid, parsedString[1], null, null, null, null, 0f);
        } else if(Regex.Match(command, "^DRAW POINT \"[\\w\\- ]+\" ON \"[\\w\\- ]+\"$").Success) {
            string[] parsedString = command.Split("\"");
            string[] grid = new string[]{parsedString[3]};
            return new ActionHolder(time, CommandType.DrawPoint, parsedString[1], grid, null, null, null, 0f);
        } else if(Regex.Match(command, "^DRAW VECTOR \"[\\w\\- ]+\" FROM \"[\\w\\- ]+\" TO \"[\\w\\- ]+\"$").Success) {
            string[] parsedString = command.Split("\"");
            string[] endpoints = new string[]{parsedString[3], parsedString[5]};
            return new ActionHolder(time, CommandType.DrawVector, parsedString[1], endpoints, null, null, null, 0f);
        } else if(Regex.Match(command, "^APPLY-MATRIX \"[\\w\\- ]+\" TO \"[\\w\\-. ]+\"$").Success) {
            string[] names = command.Split("\"");
            string[] appliedTo = new string[]{names[3]};
            return new ActionHolder(time, CommandType.ApplyMatrix, names[1], appliedTo, null, null, null, 0f);
        } else {
            Debug.Log(string.Format("No command, or invalid command, received from command JSON document."));
            return null;
        }
    }

    private OldActionHolder OldActionHolderFromNew(ActionHolder holder) {
        string commandString = "";
        if(holder.command == CommandType.CreateObject) {
            commandString = "CREATE-OBJECT \"" + holder.editorObjectName + "\"";
            commandString += (holder.internalObjectName == null ? " AS \"" + holder.internalObjectName + "\"" : "");
        } else if(holder.command == CommandType.CreateMatrix) {
            commandString = "CREATE-MATRIX \"" + holder.internalObjectName + "\" " + (new Matrix(holder.matrixFields)).ToString();
        } else if(holder.command == CommandType.DeleteObject) {
            commandString = "DELETE-OBJECT \"" + holder.internalObjectName + "\"";
        } else if(holder.command == CommandType.AssignProperty) {
            commandString = "ASSIGN-PROPERTY \"" + holder.internalObjectName + "\" \"" + holder.property + "\" " + (new Matrix(holder.matrixFields)).ToString();
            commandString += (holder.duration > 0.1f ? " " + holder.duration.ToString() : "");
        } else if(holder.command == CommandType.PlaySound) {
            commandString = "PLAY-SOUND \"" + holder.editorObjectName + "\"";
        } else if(holder.command == CommandType.DrawGrid) {
            commandString = "DRAW-GRID \"" + holder.internalObjectName + "\"";
        } else if(holder.command == CommandType.DrawPoint) {
            commandString = "DRAW-POINT \"" + holder.internalObjectName + "\" ON \"" + holder.affiliatedObjects[0] + "\"";
        } else if(holder.command == CommandType.DrawVector) {
            commandString = "DRAW-VECTOR \"" + holder.internalObjectName + "\" FROM \"" + holder.affiliatedObjects[0] + "\" TO \"" + holder.affiliatedObjects[1] + "\"";
        } else if(holder.command == CommandType.ApplyMatrix) {
            commandString = "APPLY-MATRIX \"" + holder.affiliatedObjects[0] + "\" TO \"" + holder.internalObjectName;
        } else {
            Debug.Log("Unrecognized holder command.");
            return null;
        }
        return new OldActionHolder(holder.time, commandString);
    }

    private float ParseTimestamp(string timestamp) {
        string[] timeSections = timestamp.Split(':');
        if(timeSections.Length > 3) {
            throw new System.ArrayTypeMismatchException();
        }
        float time = 0f;
        int multiplier = 1;
        for(int i = 1; i <= timeSections.Length; i++) {
            time += multiplier * float.Parse(timeSections[timeSections.Length - i]);
            multiplier *= 60;
        }
        return time;
    }

    public List<ActionHolder> GetActions() {
        return actions;
    }

    public int GetIndex(ActionHolder holder) {
        return actions.IndexOf(holder);
    }

    public int GetLength() {
        return actions.Count;
    }

    public void AddHolder(ActionHolder newHolder) {
        int i = 0;
        while(i < actions.Count && actions[i].time <= newHolder.time) {
            i++;
        }
        actions.Insert(i, newHolder);
    }

    public void DeleteHolder(ActionHolder holder) {
        actions.Remove(holder);
    }

    private static int CompareByTime(ActionHolder a, ActionHolder b) {
        return a.time.CompareTo(b.time);
    }

    public void Sort() {
        actions.Sort(CompareByTime);
    }

    public void SaveJson() {
        Debug.Log("Sorting.");
        Sort();
        Debug.Log("Converting to JSON.");
        string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(actions);
        Debug.Log("Writing to file.");
        System.IO.File.WriteAllText(Globals.lessonCreate, jsonString);
        Debug.Log("Writing complete.");
    }
}
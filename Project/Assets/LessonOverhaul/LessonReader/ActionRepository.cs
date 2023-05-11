using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        // if (Globals.isJSON)
        // {
        //     readFromJsonFile(Globals.lessonCreate);
        // }
        // else
        // {
        //     readFromOldFile(Globals.lessonCreate);
        // }
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
        Debug.Log("SaveJson entered");
        Debug.Log("Sorting.");
        Sort();
        Debug.Log("Converting to JSON.");
        string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(actions);
        Debug.Log("Writing to file.");
        System.IO.File.WriteAllText(Globals.lessonCreate, jsonString);
        Debug.Log("Writing complete.");
    }

    public void SaveOld()
    {
        Debug.Log("SaveOld entered");
        Debug.Log("Sorting.");
        Sort();
        Debug.Log("Converting to string.");
        string actionString = newActionsToOldString(actions);
        Debug.Log("Writing to file.");
        // StringBuilder filenameCandidate = new StringBuilder(System.DateTime.Now.ToString("s"));
        
        // foreach (var c in Path.GetInvalidFileNameChars()) 
        // { 
        //     filenameCandidate.Replace(c, '-'); 
        // }
        
        // System.IO.File.WriteAllText($"{filepathname}-{filenameCandidate}.txt", actionString);
        System.IO.File.WriteAllText($"{Globals.lessonCreate}_old.txt", actionString);
        Debug.Log("Writing complete.");
    }

    public string newActionsToOldString(List<ActionHolder> actions)
    {
        ResetOldOldDictionaries();
        
        IEnumerable<IGrouping<float, string>> timeGroupedActions =  
            from action in actions
            group OldOldActionHolderFromNew(action).command by action.time into timeGroupedOldAction
            select timeGroupedOldAction;

        List<string> outputLines = new List<string>();
        
        float lastTime = 0;
        foreach (var (curTime, commands) in timeGroupedActions.Select(x => (x.Key, x)))
        {
            outputLines.Add($"WAIT {curTime - lastTime :F1}");
            lastTime = curTime;

            foreach (var command in commands)
            {
                outputLines.Add(command);
            }
        }

        return String.Join("\n", outputLines);
    }

    private Dictionary<string, string> toParentName = new();
    private Dictionary<string, Dictionary<string, Vector3>> toProperties = new();

    private static Dictionary<string, Vector3> getInitialProperties()
    {
        return new() {{"POS", Vector3.zero}, {"ROT", Vector3.zero}, {"SCALE", Vector3.one}};
    }
    
    private static Dictionary<string, Vector3> getInitialGridProperties()
    {
        return new() {{"POS", Vector3.zero}, {"ROT", Vector3.zero}, {"SCALE", Vector3.one * 0.05f}};
    }

    private void ResetOldOldDictionaries()
    {
        toParentName.Clear();
        toProperties.Clear();
        
        toProperties.Add("mangle_origin", getInitialProperties());
    }
    
    

    private OldActionHolder OldOldActionHolderFromNew(ActionHolder holder) {
        string commandString = "";
        if(holder.command == CommandType.CreateObject) {
            commandString = "CREATE-OBJECT \"" + holder.editorObjectName + "\"";
            commandString += (holder.internalObjectName == null ? " AS \"" + holder.internalObjectName + "\"" : "");
            
            toParentName.Add(holder.internalObjectName ?? holder.editorObjectName, "mangle_origin");
            toProperties.Add(holder.internalObjectName ?? holder.editorObjectName, getInitialProperties());
            
        } else if(holder.command == CommandType.CreateMatrix) {
            commandString = "CREATE-MATRIX \"" + holder.internalObjectName + "\" " + (new Matrix(holder.matrixFields)).ToStringSquareDelim();
            
        } else if(holder.command == CommandType.DeleteObject) {
            commandString = "DELETE-OBJECT \"" + holder.internalObjectName + "\"";
            
        } else if(holder.command == CommandType.AssignProperty) {
            
            // Any program that can be converted back to oldold format must go to Vector3 
            Vector3 newWorldProperty = new Vector3(holder.matrixFields[0, 0], holder.matrixFields[0, 1], holder.matrixFields[0, 2]);
            
            toProperties[holder.internalObjectName][holder.property] = newWorldProperty;

            // This is bad code (only works for POS when 0 ROT), but eh
            Vector3 parentWorldProperty = toProperties[toParentName[holder.internalObjectName]][holder.property];
            
            Vector3 parentWorldScale = toProperties[toParentName[holder.internalObjectName]]["SCALE"];
            
            Vector3 newLocalProperty = (newWorldProperty - parentWorldProperty);
                for(int i=0; i<3; i++) newLocalProperty[i] /= parentWorldScale[i];
            
            commandString = "ASSIGN-PROPERTY \"" + holder.internalObjectName + "\" \"" + holder.property + "\" " + (Matrix.newRowVector(newLocalProperty)).ToStringSquareDelim();
            commandString += (holder.duration > 0.1f ? " " + holder.duration.ToString("F1") : "");
            
        } else if(holder.command == CommandType.PlaySound) {
            commandString = "PLAY SOUND \"" + holder.editorObjectName + "\"";
            
        } else if(holder.command == CommandType.DrawGrid) {
            commandString = "DRAW GRID \"" + holder.internalObjectName + "\"";
            
            toParentName.Add(holder.internalObjectName, "mangle_origin");
            toProperties.Add(holder.internalObjectName, getInitialGridProperties());
            
        } else if(holder.command == CommandType.DrawPoint) {
            commandString = "DRAW POINT \"" + holder.internalObjectName + "\" ON \"" + holder.affiliatedObjects[0] + "\"";
            
            toParentName.Add(holder.internalObjectName, holder.affiliatedObjects[0]);
            toProperties.Add(holder.internalObjectName, getInitialProperties());
            
        } else if(holder.command == CommandType.DrawVector) {
            commandString = "DRAW VECTOR \"" + holder.internalObjectName + "\" FROM \"" + holder.affiliatedObjects[0] + "\" TO \"" + holder.affiliatedObjects[1] + "\"";
            
        } else if(holder.command == CommandType.ApplyMatrix) {
            commandString = "APPLY-MATRIX \"" + holder.affiliatedObjects[0] + "\" TO \"" + holder.internalObjectName;
            
        } else {
            Debug.Log("Unrecognized holder command.");
            return null;
        }
        return new OldActionHolder(holder.time, commandString);
    }

    public void Refresh(string filename)
    {
        readFromJsonFile(filename);
    }
    //
    // private void readFromOldFile(string filename)
    // {
    //     ResetOldOldDictionaries();
    // }
    //
    // private ActionHolder ActionHolderFromOldOld(OldActionHolder old) {
    //     float time = old.time;
    //     string command = old.command;
    //     if(Regex.Match(command, "^CREATE-OBJECT \"[\\w\\-. ]+\"( AS \"[\\w\\-. ]+\")?$").Success) {
    //         string[] names = command.Split("\"");
    //         if(names.Length > 3) {
    //             return new ActionHolder(time, CommandType.CreateObject, names[3], null, names[1], null, null, 0f);
    //         } else {
    //             return new ActionHolder(time, CommandType.CreateObject, null, null, names[1], null, null, 0f);
    //         }
    //     } else if (Regex.Match(command, "^CREATE-MATRIX \"[\\w\\- ]+\" {((-?[0-9]+(.[0-9]+)?)(,-?[0-9]+(.[0-9]+)?)*)(;(-?[0-9]+(.[0-9]+)?)(,-?[0-9]+(.[0-9]+)?)*)*}$").Success) {
    //         // Obtain matrix elements
    //         string matrixElements = command.Split(new char[] {'{', '}'})[1];
    //         string[] names = command.Split("\"");
    //         try {
    //             // Attempt to create matrix
    //             Matrix mat = new Matrix(matrixElements);
    //             return new ActionHolder(time, CommandType.CreateMatrix, names[1], null, null, null, mat.values, 0f);
    //         } catch {
    //             Debug.Log("Error: inconsistent matrix size when loading from command JSON document.");
    //             return null;
    //         }
    //     } else if(Regex.Match(command, "^DELETE-OBJECT \"[\\w\\-. ]+\"$").Success) {
    //         string[] names = command.Split("\"");
    //         return new ActionHolder(time, CommandType.DeleteObject, names[1], null, null, null, null, 0f);
    //     } else if(Regex.Match(command, "^ASSIGN-PROPERTY \"[\\w-. ]+\" \"[A-Z]+\" ({(-?[0-9]+(.[0-9]+)?)((;|,)(-?[0-9]+(.[0-9]+)?))*})( [0-9]+(.[0-9]+))?$").Success) {
    //         string[] names = command.Split("\"");
    //         string[] parsedString = command.Split(new char[] {'{', '}'});
    //         float movementTime = 0f;
    //         if(!string.IsNullOrEmpty(parsedString[2])) {
    //             movementTime = float.Parse(parsedString[2]);
    //         }
    //         try {
    //             string matrixElements = parsedString[1];
    //             Matrix mat = new Matrix(matrixElements);
    //             return new ActionHolder(time, CommandType.AssignProperty, names[1], null, null, names[3], mat.values, movementTime);
    //         } catch {
    //             Debug.Log("Error: inconsistent matrix size when loading from command JSON document.");
    //             return null;
    //         }
    //     } else if(Regex.Match(command, "^PLAY SOUND \"[\\w\\- ]+\"").Success) {
    //         string[] names = command.Split("\"");
    //         return new ActionHolder(time, CommandType.PlaySound, null, null, names[1], null, null, 0f);
    //     } else if(Regex.Match(command, "^DRAW GRID \"[\\w\\- ]+\"").Success) {
    //         string[] parsedString = command.Split("\"");
    //         return new ActionHolder(time, CommandType.DrawGrid, parsedString[1], null, null, null, null, 0f);
    //     } else if(Regex.Match(command, "^DRAW POINT \"[\\w\\- ]+\" ON \"[\\w\\- ]+\"$").Success) {
    //         string[] parsedString = command.Split("\"");
    //         string[] grid = new string[]{parsedString[3]};
    //         return new ActionHolder(time, CommandType.DrawPoint, parsedString[1], grid, null, null, null, 0f);
    //     } else if(Regex.Match(command, "^DRAW VECTOR \"[\\w\\- ]+\" FROM \"[\\w\\- ]+\" TO \"[\\w\\- ]+\"$").Success) {
    //         string[] parsedString = command.Split("\"");
    //         string[] endpoints = new string[]{parsedString[3], parsedString[5]};
    //         return new ActionHolder(time, CommandType.DrawVector, parsedString[1], endpoints, null, null, null, 0f);
    //     } else if(Regex.Match(command, "^APPLY-MATRIX \"[\\w\\- ]+\" TO \"[\\w\\-. ]+\"$").Success) {
    //         string[] names = command.Split("\"");
    //         string[] appliedTo = new string[]{names[3]};
    //         return new ActionHolder(time, CommandType.ApplyMatrix, names[1], appliedTo, null, null, null, 0f);
    //     } else {
    //         Debug.Log(string.Format("No command, or invalid command, received from command JSON document."));
    //         return null;
    //     }
    // }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CommandType
{
    CreateObject,
    CreateMatrix,
    DeleteObject,
    AssignProperty,
    PlaySound,
    DrawGrid,
    DrawPoint,
    DrawVector,
    ApplyMatrix
}

public class ActionHolder
{
    public float time;
    public CommandType command;
    public string internalObjectName;
    public string[] affiliatedObjects;
    public string editorObjectName;
    public string property;
    public float[,] matrixFields;
    public float duration;

    public ActionHolder() {
        this.time = 0f;
        this.command = CommandType.CreateObject;
        this.internalObjectName = "";
        this.affiliatedObjects = new string[2];
        this.editorObjectName = "";
        this.property = "";
        this.matrixFields = new float[3,3];
        this.duration = 0f;
    }
    
    public ActionHolder(float time, CommandType cmd, string iON, string[] aff, string eON, string prop, float[,] mat, float dur) {
        this.time = time;
        this.command = cmd;
        this.internalObjectName = iON;
        this.affiliatedObjects = aff;
        this.editorObjectName = eON;
        this.property = prop;
        this.matrixFields = mat;
        this.duration = dur;
    }
}
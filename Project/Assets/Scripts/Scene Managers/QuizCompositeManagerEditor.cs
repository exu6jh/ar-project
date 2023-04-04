#if (UNITY_EDITOR)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class StateUserContainer : ScriptableObject
{
    [SerializeReference]
    public QuizStateUser stateUser;
}

[CustomEditor(typeof(QuizCompositeManager))]
public class QuizCompositeManagerEditor : Editor
{
    private bool addFoldoutOpen;
    
    (string name, Type stateUserType, StateUserContainer potNewState, bool active)[] users;

    private int active;

    private void Awake()
    {
        QuizCompositeManager quizCompositeManager = target as QuizCompositeManager;
        quizCompositeManager.stateUsers = new List<QuizStateUser>();
        
        users = new (string name, Type stateUserType, StateUserContainer potNewState, bool active)[]
        {
            ("Slider User", typeof(SliderStateUser), CreateInstance<StateUserContainer>(), false),
            ("Vector User", typeof(VectorStateUser), CreateInstance<StateUserContainer>(), false),
        };
        
        // foreach ((string name, Type stateUserType, StateUserContainer potNewState, bool active) _ in users)
        // {
        //     Debug.Log(_.stateUserType.IsSubclassOf(typeof(QuizStateUser)));
        // }
    }

    private void SetActive(int j)
    {
        active = j;
        for (int i = 0; i < users.Length; i++)
        {
            if (i == j) continue;
            users[i].active = false;
        }
    }

    public override void OnInspectorGUI()
    {
        
        base.OnInspectorGUI();

        addFoldoutOpen = EditorGUILayout.Foldout(addFoldoutOpen, "New State Users");

        if (addFoldoutOpen)
        {
            for (int i = 0; i < users.Length; i++)
            {
                users[i].active = EditorGUILayout.Toggle(users[i].name, users[i].active);
                
                if (users[i].active)
                {
                    SetActive(i);
                        
                    if (users[i].potNewState.stateUser == null)
                    {
                        users[i].potNewState.stateUser = users[i].stateUserType.GetConstructor(Type.EmptyTypes).Invoke(null) as QuizStateUser;
                    }
                    
                    SerializedObject newStateObj = new SerializedObject(users[i].potNewState);
                    
                    SerializedProperty newStateProp = newStateObj.FindProperty("stateUser");

                    EditorGUILayout.PropertyField(newStateProp, new GUIContent(users[i].name), true);

                    newStateObj.ApplyModifiedProperties();
                }
            }

            if (GUILayout.Button("Add New State User"))
            {
                QuizCompositeManager quizCompositeManager = target as QuizCompositeManager;
                
                quizCompositeManager.stateUsers.Add(users[active].potNewState.stateUser);

                for (int i = 0; i < users.Length; i++)
                {
                    users[i].active = false;
                    users[i].potNewState.stateUser = null;
                }
            }
        }
    }
}

#endif // (UNITY_EDITOR)
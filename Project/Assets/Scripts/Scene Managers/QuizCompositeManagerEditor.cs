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
    
    (string name, Type stateUserType, StateUserContainer potNewState)[] users;

    private string[] userNames;

    private int active;

    private void Awake()
    {
        QuizCompositeManager quizCompositeManager = target as QuizCompositeManager;
        quizCompositeManager.stateUsers = new List<QuizStateUser>();
        
        var userCreation = new (string name, Type stateUserType)[]
        {
            ("Slider User", typeof(SliderStateUser)),
            ("Vector User", typeof(VectorStateUser)),
        };
        
        users = (from user in userCreation select (user.name, user.stateUserType, CreateInstance<StateUserContainer>())).ToArray();
        
        userNames = (from user in userCreation select user.name).ToArray();

        active = -1;

        // foreach ((string name, Type stateUserType, StateUserContainer potNewState, bool active) _ in users)
        // {
        //     Debug.Log(_.stateUserType.IsSubclassOf(typeof(QuizStateUser)));
        // }
    }

    public override void OnInspectorGUI()
    {
        
        base.OnInspectorGUI();

        addFoldoutOpen = EditorGUILayout.Foldout(addFoldoutOpen, "New State Users");

        if (addFoldoutOpen)
        {
            bool addNewUser = GUILayout.Button("Add New State User");
            
            active = EditorGUILayout.Popup("Choose State User", active, userNames);
            
            if(active >= 0)
            {

                users[active].potNewState.stateUser ??=
                    users[active].stateUserType.GetConstructor(Type.EmptyTypes).Invoke(null) as QuizStateUser;

                SerializedObject newStateObj = new SerializedObject(users[active].potNewState);

                SerializedProperty newStateProp = newStateObj.FindProperty("stateUser");

                EditorGUILayout.PropertyField(newStateProp, new GUIContent(users[active].name), true);

                newStateObj.ApplyModifiedProperties();


                if (addNewUser)
                {
                    QuizCompositeManager quizCompositeManager = target as QuizCompositeManager;

                    quizCompositeManager.stateUsers.Add(users[active].potNewState.stateUser);

                    // for (int i = 0; i < users.Length; i++)
                    // {
                    //     users[i].potNewState.stateUser = null;
                    // }

                    users[active].potNewState.stateUser =
                        users[active].stateUserType.GetConstructor(Type.EmptyTypes).Invoke(null) as QuizStateUser;

                    active = -1;
                }
            }
        }
    }
}

#endif // (UNITY_EDITOR)
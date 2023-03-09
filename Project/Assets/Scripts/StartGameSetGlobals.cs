using UnityEngine;

// This class serves to initialize the majority of the global variables 
// defined in Globals.cs. 
public class StartGameSetGlobals : MonoBehaviour
{
    // Class is created on first load of the MainMenu scene so this creation is
    // when 'done' is initialized here to 0
    public static bool done = false;
    
    // Start is called before the first frame update in navigating to the
    // MainMenu scene
    void Start()
    {
        
        // 'done' = 0 only when this class is first created which occurs on
        // the first entry into the MainMenu scene (i.e. when the game is first
        // launched). Thus, the below initializations occur only when the 
        // game is first launched.
        if (!done)
        {
            done = true;

            // Initialization of all the various in-game behavior settings
            // variables

            // DEBUG
            Debug.Log(Globals.lesson);
        }
    }

    // This method sets up the output folder structure for all the game
    // outputs from the trials.
    public static void SetUpFolderStructure()
    {
        // // Create PTOutput Folder
        // Directory.CreateDirectory(Application.dataPath + "/" + "PTOutput");
        //
        // // Create Patient Folder
        // Directory.CreateDirectory(Application.dataPath + "/" + "PTOutput" + 
        //     "/" + "Patient_ING" + Globals.USER_ID);
        //
        // Debug.Log("ID: " + Globals.USER_ID);
        //
        // // Create folder for date
        // DateTime dT = DateTime.Now;
        // string day = $"{dT.Month}.{dT.Day}.{dT.Year}";
        //
        // string folderRoot = $"PTOutput/Patient_ING{Globals.USER_ID}/{day}";
        //
        // Directory.CreateDirectory($"{Application.dataPath}/{folderRoot}");
        //
        //
        // int sessionNumForDay = 1;
        //
        // // Checks for other output folders for the same patient on the same day.
        // // If necessary, it increments the session number for that patient
        // // to help identify the output files from the same day.
        // while(Directory.Exists(Application.dataPath + "/" + folderRoot + "/" + "Session" + "_" + "#" + sessionNumForDay)) 
        // {
        //     sessionNumForDay++;
        // }
        //
        // string sessionFolderRoot = folderRoot + "/" + "Session" + "_" + "#" + sessionNumForDay;
        //
        // //update some GameStates with the new folder location
        // GAME_STATE_ENUM[] statesToUpdate = new GAME_STATE_ENUM[]
        // {
        //     GAME_STATE_ENUM.Trials,
        //     GAME_STATE_ENUM.PreTest,
        //     GAME_STATE_ENUM.PostTest
        // };
        //
        // foreach (GAME_STATE_ENUM state in statesToUpdate)
        // {
        //     Globals.GAME_STATES[state].path = sessionFolderRoot;
        //     Globals.GAME_STATES[state].ziFilePrefix = "Patient_ING" + Globals.USER_ID + "_" + dT.Month + 
        //         "." + dT.Day + "." + dT.Year + "_" + "Session#" + sessionNumForDay;
        //     Globals.GAME_STATES[state].ziFilePath = folderRoot;
        // }
    }
}

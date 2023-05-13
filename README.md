# ar-project

# **Running the program:**
When running on the Unity Editor, the main starting point is MenuScreen.unity. On a computer, one can simulate the AR headset movement through using WASD and Q/E to move around, and right click-drag to look around. Using the spacebar allows the user to simulate their right hand, while the left-shift button allows the user to simulate their left hand. Left-clicking interacts, which is used for clicking buttons, and for dragging components around.

There are four lesson buttons in the starting scene, which you may click to activate. Currently, only two lessons are implemented, so the other two buttons will redirect to lesson 1. To return to the main menu, press either of the two return buttons. Lesson 1 concerns the basics of what a vector is, and lesson 2 concerns certain aspects of vector spaces (such as the duality between points in a coordinate grid and vectors.) Lesson 3 will concern the components of vectors and bases, while lesson 4 will concern the basics of linear operations on vectors.

In the TwoGrid scene accessible to the very right of the lesson buttons, there are, as the name would suggest, two grids. In the grid on the right, there are 3 blue vectors displayed, along with 1 green vector, 1 red vector, and 4 blue points. The blue points also have labels indicating their position relative to the origin. By walking up to the grid and left-clicking / pinching the blue points, one can move these points around. (Since points snap to the nearest gridpoint, you will have to move the point more than halfway to a different gridpoint to see movement.) Since the endpoints of the blue vectors are constrained to these points, the vectors will move around as well. The green and red vectors are also constrained to the vector starting at the origin, such that they represent the scaled version of basis vectors that make up that vector.

In the grid to the left, you will see 1 blue vector, and 2 vectors (red and green) representing basis vectors. By default, the grid is transformable, meaning that the basis vectors are also movable (in a similar way to moving points earlier, by pinching near the arrow-ends of the basis vectors), and when the basis vectors change, the entire grid also changes to reflect. The label of the point at the end of the blue vector will also change to reflect the new components of the blue vector measured with the new basis.

Slightly to the right of this grid, there are two circular buttons, one to toggle the grid being transformable (disabling moving the basis vectors, and allowing one to snap-move the blue vector around), and one to enable “linear maps”. When “linear maps” is enabled, not only will the gridlines move with the basis vectors as they are changed, but the blue vector will also be transformed along with, to keep the same components with the changing basis.

To return to the main menu, press the square button to the right of the two circular buttons.

<Note: The version deployed on the hololens contains only the TwoGrid scene.>

# **Code base:**
There are 3 “primitive” objects introduced in the environment; grids, vectors, and points. All three objects have “Manager” components, which monitor the state of each object, including their geometric position and “value”, and contain helper functions to read and set values. Vectors and points reference GridManager in order to determine the current basis, which are used to convert from their position relative to the standard euclidean basis,  to their components with the basis stored in GridManager. Points and Vectors may be manipulated by adding “Constraint” components, which include constraining a vector to lie between two endpoints, constraining a vector to represent the linear combination that makes up a vector along a certain basis, and constraining a point to “snap” to the nearest grid point, along with a hidden “NearInteractionGrabbable” GameObject.

GridManager is responsible for updating gridlines when its basis vectors change, as well as other arbitrary game objects when “linearMap” is true. To aid in performing the necessary transformations, the Transformable record interface is used to enable different methods of adjusting the positions of objects to correctly reflect linearMap. For example, any object with a “Constraint” component is not updated directly, as the object will be updated when its constraints are updated.

Lessons are implemented through means of our custom scripting language. The language is parsed within the LessonReader class through a regex-based parser. A comprehensive list of available commands, as well as sample lessons, is present in the dedicated document entitled “Script Commands and Example Lessons.pdf”. These lessons can also be found within the Lessons folder of the Assets folder within Project.

Custom script loading is temporarily disabled due to the issues with deployment and incompatible file systems. The functionality for it is present in ChangeLesson.cs, which was originally attached to a TextMeshPro object to allow the user to input the name of the custom script, but the object was removed because of the aforementioned issue.

# **Data Collection:**
The linear algebra application contains a data collection system that collects engagement data like the total amount of time a user has spent in a certain scene of a lesson. For the quiz sections, it also collects data on how many questions a user has answered correctly / inocorrectly. 

Data was stored in the following format: 

Entries: This is the array which stores multiple “entries”. Each entry contains data of the user on each scene that they visited. Entries are arranged in chronological order of the time they were visited.

Entry format:
- Name: Name of the scene visited 
- Total_time: Total time spent on the scene
- StartTime: The start timestamp of the scene (formatted in YYYY-MM-DD HH:MM:SS) 
- EndTime: The end timestamp of the scene with the same format 
- Completed: For the quiz scenes, checks if the quiz was completed or not 
- Quiz: Data for the quiz scene, contains an array of quiz questions and a boolean for which questions are right and wrong 

The other fields were not filled up during data collection.

/Project/Assets/Scripts/Data Scripts/ contains all the classes in which the data is formatted as well as the scripts which serialize the data collected into a JSON string. After the JSON string has been serialized, a UnityWebRequest is sent to a NextJS api hosted on vercell which then transfers the string into the MongoDB database. 

# ar-project

# **Running the program**
When running on the Unity Editor, the main starting point is MenuScreen.unity. On a computer, one can simulate the AR headset movement through using WASD and Q/E to move around, and right click-drag to look around. Using the spacebar allows the user to simulate their right hand, while the left-shift button allows the user to simulate their left hand. Left-clicking interacts, which is used for clicking buttons, and for dragging components around.

There are three buttons in the starting screen: One to start the user study, one to view an expanded playground/sandbox, and one to showcase the custom lesson feature.

## User study
The following is a full description of the user study process, including the components of the user study scene.

1. Students fill out a pre-questionnaire with basic demographic information: age, class year and area of study

2. The students would then take a VARK quiz with 16 multiple-choice questions to gauge their learning style. The result of the quiz would provide the learning style that the students are most likely to have: visual, aural, read/write, and kinesthetic 

3. Students then take a short pre-quiz that would test them on basic linear algebra concepts that the lessons would go over. The pre-quiz would contain around 7 basic linear algebra questions. The final quiz scores would be calculated (answers would not be shared with the students). The total time taken to complete the quiz would also be collected using a stopwatch. 

4. The students are then randomly assigned to either watch a 2D video on a computer that would go over linear algebra concepts or engage in an interactive our AR application that would cover the exact same concepts. The only difference is that our AR application includes an interactive section

5. Once the students start their respective modules, the total time taken to complete them would be recorded. Since the AR activity has a lesson and an interactive module, the total time spent on each mode would be tracked. For the 2D lesson time taken would be calculated using a stopwatch while for the AR section time is calculated internally by the HL2 application itself. 

6. Once the student is done, they would be told that there would be reminded that there is a second quiz that they need to take. They would then be given the opportunity to review the content they watched. If they choose to review, the total time would be calculated for that.

7. After review, the students would then be introduced to a post-quiz with 9 linear algebra questions of similar difficulty as the pre-quiz. The final quiz results would be calculated and the time taken to complete the quiz would also be tracked. 

8. Finally, the students fill out a small likeability survey to gauge their preferences with 6 Likert scale questions to on a google form to gauge how positive/negative they found their respective experiences.

## Playground
In the playground, there are two grids. In the grid on the right, there are 3 blue vectors displayed, along with 1 green vector, 1 red vector, and 4 blue points. The blue points also have labels indicating their position relative to the origin. By walking up to the grid and left-clicking / pinching the blue points, one can move these points around. (Since points snap to the nearest gridpoint, you will have to move the point more than halfway to a different gridpoint to see movement.) Since the endpoints of the blue vectors are constrained to these points, the vectors will move around as well. The green and red vectors are also constrained to the vector starting at the origin, such that they represent the scaled version of basis vectors that make up that vector. Note: the grid on the right has also been enhanced with an experimental 3d shader to allow visualizing gridlines in a 3d grid without visual clutter resulting from the sheer number of gridlines.

The shader works as follows: There is a center point a fixed distance in front of the users head. (This point moves as the users head moves) The closer a gridline is to this center point, the higher it's opacity. The further away, the lower the opacity, until it reaches a value of zero some set distance away. Linear, quadratic, and inverse quadratic falloffs are supported, up to that set zero distance. There is also a subtle color change as the distance from the center point changes. Based on feedback, the shader may behave differently in the future, to have a global illumination effect around all points & vectors, or even just around the hand / fingertip, especially when interaction is occuring

In this 3D grid, points are no longer constrained to move in the x-y plane, and can instead also move in the z direction. The previous 2D linear combination is also now extended to 3D.

## Custom lesson
The custom lesson loads an arbitrary lesson in a txt file in the old, human readable format. Currently, the file is hard coded to a file named "active1.txt" to avoid the need of file selection on the hololens, in the future there will be a better system to detect available lessons on the file system / external web database and load them.

The file "active1.txt" is expected to be in the "Application.persistantDataPath/CustomLessons" folder. On windows, this is in LocalLow/company name/product name, on hololens, this is in LocalAppData/app name + version / Local state.

'active1.txt' can actually be hot loaded while the app is running, allowing the user to currently change the lesson without restarting the app, just by going back to the main menu, changing the contents of "active1.txt", and re-entering the custom lesson scene.

# Lesson creator
The lesson creator can be used by opening Unity, going to LessonOverhaul/LessonCreator/Main Creator.unity, and then pressing play. There are two options, either creating a new lesson file with a user-specified name (which will be saved by default to LessonOverhaul/Lessons), or opening a pre-existing saved lesson file (which will be saved exactly where the file was opened from.)

Opening creating or opening a file, the user is taken to the lesson modification scene. There are three main features to this scene: a timeline with lesson tokens, a token creator, and a token editor.

The timeline (which is measured in units of seconds) can be navigated by scrolling to zoom in and out in specific times, and times can be controlled by moving the slider like a video controller. Tokens that overlap too closely in time are aggregated into "multitokens", which when clicked, present an accordion-style view of the constituent tokens, ordered chronologically. The last token of a lesson is marked by "End" (although this is a bit of a misnomer, as actions with durations may last until after this token, for example played sounds and animations.) Lessons can be at most 7200 seconds (2 hours) long.

The token creator allows users to create tokens of a specified command type at a specified time. The command fields must be set in the command token editor afterwards. Note that when tokens with concurrent times are created, they are placed in the lesson in the order that they were created in. This actually matters; for example, creating an object and setting its initial position must be done with the object creation first and the position setting second, even if done with no delay.

There are currently nine types of supported commands. They are:
1. Object creation
2. Object deletion
3. Property assignment (instantaneous or over time)
3a. Position
3b. Rotation
3c. Scale
4. Grid creation (as object)
5. Point creation (as object on grid)
6. Vector creation (as object from points)
7. Matrix/linear operation creation
8. Matrix/linear operation application to object
9. Sound playback

The token editor allows users to edit token fields. For example, the internal name of an object being created, the rotation of an object,
the filename of a sound being played, the duration of a movement, and so on.

When the user is done with their lesson, they may save the lesson in two available formats: one of which is a JSON file, and one of which is a human-readable domain-specific language custom to the application. It is worth noting that only the former can be recognized and edited directly, while only the latter can be uploaded to the HoloLens; as the methods to convert between the two formats exist, we hope to rectify this issue shortly.

# **Code base**
There are 3 “primitive” objects introduced in the environment; grids, vectors, and points. All three objects have “manager” components, which monitor the state of each object, including their geometric position and “value”, and contain helper functions to read and set values. Vectors and points reference GridManager in order to determine the current basis, which are used to convert from their position relative to the standard euclidean basis,  to their components with the basis stored in GridManager. Points and vectors may be manipulated by adding “Constraint” components, which include constraining a vector to lie between two endpoints, constraining a vector to represent the linear combination that makes up a vector along a certain basis, and constraining a point to “snap” to the nearest grid point, along with a hidden “NearInteractionGrabbable” GameObject.

GridManager is responsible for updating gridlines when its basis vectors change, as well as other arbitrary game objects when “linearMap” is true. To aid in performing the necessary transformations, the Transformable record interface is used to enable different methods of adjusting the positions of objects to correctly reflect linearMap. For example, any object with a “Constraint” component is not updated directly, as the object will be updated when its constraints are updated.

Lessons are parsed on the HoloLens side within the LessonReader class through a regex-based parser that reads lesson files line by line, executing one by one like a procedural scripting language. One notable limitation of this method of reading is the lack of timestamps, which means that the parser relies on recursive calls to maintain accurate command flow, which can lead to too many recursive calls. On the custom lesson creator side of things, as suggested by the LessonOverhaul folder's name, lessons are parsed instead from JSON directly into timestamped event objects, which are kept in an array. Though the lesson reader still procedurally scans through the array, it now executes commands when the proper time is reached, rather than relying on recursive calls.

# **Data collection**
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

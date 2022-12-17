// Scenes available as specified in build settings
public enum SCENES
{
    MENU,
    LESSON,
    TWO_GRIDS,
    TEMPLATE
}

// Lessons avilable
public enum LESSONS
{
    LESSON_1,
    LESSON_2,
    // LESSON_3,
    // LESSON_4,
}

public static class Globals
{
    public static string lesson;

    // Convert LESSONS to lesson text file name
    public static string LessonEnumToString(LESSONS lessons)
    {
        switch (lessons)
        {
            case LESSONS.LESSON_1:
                return "lesson1.txt";
            case LESSONS.LESSON_2:
                return "lesson2.txt";
            default:
                return "lesson1.txt";
        }
    }
}
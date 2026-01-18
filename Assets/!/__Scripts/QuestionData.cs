using UnityEngine;

[System.Serializable]
public class QuestionData
{
    [TextArea(2, 4)] public string questionText;

    [Header("Options")]
    public string option1Text = "Yes";
    public string option2Text = "No";

    [Header("Correct Answers")]
    public bool option1IsCorrect = true;
    public bool option2IsCorrect = false;
}

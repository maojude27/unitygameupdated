using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestionData
{
    public int questionId;
    public string questionText;
    public string questionType; // "input_field", "multiple_choice", "drag_drop"
    public List<string> correctAnswers;
    public List<string> multipleChoiceOptions;
    public List<int> correctMultipleChoiceIndices;
    public int maxAnswers = 5;
    public int minimumAnswersRequired = 3;
    public bool allowDuplicateAnswers = false;
    public bool caseSensitiveAnswers = false;
    public float passingScorePercentage = 70f;
    public string category = "General";
    public int difficultyLevel = 1;
    public string description = "";

    public QuestionData()
    {
        correctAnswers = new List<string>();
        multipleChoiceOptions = new List<string>();
        correctMultipleChoiceIndices = new List<int>();
    }
}

[System.Serializable]
public class QuestionSet
{
    public string setName;
    public string category;
    public int difficultyLevel;
    public List<QuestionData> questions;

    public QuestionSet()
    {
        questions = new List<QuestionData>();
    }
}

[System.Serializable]
public class QuestionDatabase
{
    public List<QuestionSet> questionSets;

    public QuestionDatabase()
    {
        questionSets = new List<QuestionSet>();
    }
}

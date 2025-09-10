using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class QuestionManager : MonoBehaviour
{
    [Header("Question Sources")]
    public bool useLocalJSON = true;
    public bool useFlaskAPI = false;
    public string localJSONPath = "Questions/questions.json";
    public string flaskAPIEndpoint = "/api/questions";
    
    [Header("Question Selection")]
    public string currentCategory = "Programming";
    public int currentDifficultyLevel = 1;
    public int questionsPerSession = 3;
    public bool randomizeQuestions = true;
    
    [Header("Current Session")]
    public QuestionData currentQuestion;
    public List<QuestionData> sessionQuestions;
    public int currentQuestionIndex = 0;
    
    private QuestionDatabase questionDatabase;
    private GameStageManager gameStageManager;
    
    public static QuestionManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize lists
            if (sessionQuestions == null)
                sessionQuestions = new List<QuestionData>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        gameStageManager = FindFirstObjectByType<GameStageManager>();
        StartCoroutine(LoadQuestions());
    }
    
    public IEnumerator LoadQuestions()
    {
        Debug.Log("=== LOADING DYNAMIC QUESTIONS ===");
        
        if (useFlaskAPI && !string.IsNullOrEmpty(flaskAPIEndpoint))
        {
            yield return StartCoroutine(LoadQuestionsFromAPI());
        }
        else if (useLocalJSON)
        {
            yield return StartCoroutine(LoadQuestionsFromJSON());
        }
        else
        {
            // Fallback to default questions
            CreateDefaultQuestions();
        }
        
        // Setup session questions
        SetupSessionQuestions();
        
        // Load first question
        LoadCurrentQuestion();
    }
    
    private IEnumerator LoadQuestionsFromAPI()
    {
        Debug.Log("Loading questions from Flask API...");
        
        string url = gameStageManager?.flaskURL + flaskAPIEndpoint;
        
        // Create request payload
        string jsonPayload = $"{{\"category\":\"{currentCategory}\",\"difficulty\":{currentDifficultyLevel},\"count\":{questionsPerSession}}}";
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string jsonResponse = request.downloadHandler.text;
                questionDatabase = JsonUtility.FromJson<QuestionDatabase>(jsonResponse);
                Debug.Log($"Successfully loaded {questionDatabase.questionSets.Count} question sets from API");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse API response: {e.Message}");
                CreateDefaultQuestions();
            }
        }
        else
        {
            Debug.LogWarning($"API request failed: {request.error}. Using fallback questions.");
            CreateDefaultQuestions();
        }
        
        request.Dispose();
    }
    
    private IEnumerator LoadQuestionsFromJSON()
    {
        Debug.Log("Loading questions from local JSON...");
        
        string filePath = Path.Combine(Application.streamingAssetsPath, localJSONPath);
        
        if (Application.platform == RuntimePlatform.Android)
        {
            // Android uses UnityWebRequest for StreamingAssets
            UnityWebRequest request = UnityWebRequest.Get(filePath);
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    questionDatabase = JsonUtility.FromJson<QuestionDatabase>(request.downloadHandler.text);
                    Debug.Log($"Successfully loaded {questionDatabase.questionSets.Count} question sets from Android JSON");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse JSON: {e.Message}");
                    CreateDefaultQuestions();
                }
            }
            else
            {
                Debug.LogWarning($"Failed to load JSON from Android: {request.error}");
                CreateDefaultQuestions();
            }
            request.Dispose();
        }
        else
        {
            // Other platforms can use File.ReadAllText
            if (File.Exists(filePath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(filePath);
                    questionDatabase = JsonUtility.FromJson<QuestionDatabase>(jsonContent);
                    Debug.Log($"Successfully loaded {questionDatabase.questionSets.Count} question sets from JSON");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse JSON file: {e.Message}");
                    CreateDefaultQuestions();
                }
            }
            else
            {
                Debug.LogWarning($"JSON file not found at: {filePath}");
                CreateDefaultQuestions();
            }
            yield return null;
        }
    }
    
    private void CreateDefaultQuestions()
    {
        Debug.Log("Creating default question database...");
        
        questionDatabase = new QuestionDatabase();
        
        // Programming Questions Set
        QuestionSet programmingSet = new QuestionSet();
        programmingSet.setName = "Programming Languages";
        programmingSet.category = "Programming";
        programmingSet.difficultyLevel = 1;
        
        // Input Field Question
        QuestionData inputQuestion = new QuestionData();
        inputQuestion.questionId = 1;
        inputQuestion.questionText = "Name 5 popular programming languages:";
        inputQuestion.questionType = "input_field";
        inputQuestion.maxAnswers = 5;
        inputQuestion.minimumAnswersRequired = 3;
        inputQuestion.allowDuplicateAnswers = false;
        inputQuestion.caseSensitiveAnswers = false;
        inputQuestion.passingScorePercentage = 60f;
        inputQuestion.category = "Programming";
        inputQuestion.difficultyLevel = 1;
        
        string[] programmingLanguages = {
            "python", "javascript", "c#", "java", "c++",
            "ruby", "go", "swift", "php", "kotlin",
            "typescript", "rust", "dart", "scala", "perl",
            "html", "css", "sql", "r", "matlab",
            "c", "objective-c", "shell", "powershell", "bash"
        };
        
        inputQuestion.correctAnswers.AddRange(programmingLanguages);
        programmingSet.questions.Add(inputQuestion);
        
        // Multiple Choice Question
        QuestionData mcQuestion = new QuestionData();
        mcQuestion.questionId = 2;
        mcQuestion.questionText = "Which of the following are programming languages?";
        mcQuestion.questionType = "multiple_choice";
        mcQuestion.passingScorePercentage = 70f;
        mcQuestion.category = "Programming";
        mcQuestion.difficultyLevel = 1;
        
        mcQuestion.multipleChoiceOptions.AddRange(new string[] {
            "Python", "JavaScript", "HTML", "SQL", "Photoshop", "Excel", "Java", "CSS"
        });
        
        mcQuestion.correctMultipleChoiceIndices.AddRange(new int[] { 0, 1, 3, 6 }); // Python, JavaScript, SQL, Java
        programmingSet.questions.Add(mcQuestion);
        
        // Drag and Drop Question
        QuestionData dragQuestion = new QuestionData();
        dragQuestion.questionId = 3;
        dragQuestion.questionText = "Drag the correct programming concepts:";
        dragQuestion.questionType = "drag_drop";
        dragQuestion.passingScorePercentage = 70f;
        dragQuestion.category = "Programming";
        dragQuestion.difficultyLevel = 1;
        
        dragQuestion.correctAnswers.AddRange(new string[] {
            "variable", "function", "loop", "array", "object",
            "class", "method", "string", "integer", "boolean"
        });
        programmingSet.questions.Add(dragQuestion);
        
        questionDatabase.questionSets.Add(programmingSet);
        
        // Math Questions Set
        QuestionSet mathSet = new QuestionSet();
        mathSet.setName = "Basic Mathematics";
        mathSet.category = "Mathematics";
        mathSet.difficultyLevel = 1;
        
        QuestionData mathQuestion = new QuestionData();
        mathQuestion.questionId = 4;
        mathQuestion.questionText = "Name 4 basic mathematical operations:";
        mathQuestion.questionType = "input_field";
        mathQuestion.maxAnswers = 4;
        mathQuestion.minimumAnswersRequired = 3;
        mathQuestion.allowDuplicateAnswers = false;
        mathQuestion.caseSensitiveAnswers = false;
        mathQuestion.passingScorePercentage = 75f;
        mathQuestion.category = "Mathematics";
        mathQuestion.difficultyLevel = 1;
        
        mathQuestion.correctAnswers.AddRange(new string[] {
            "addition", "subtraction", "multiplication", "division",
            "add", "subtract", "multiply", "divide", "+", "-", "*", "/"
        });
        mathSet.questions.Add(mathQuestion);
        
        questionDatabase.questionSets.Add(mathSet);
        
        Debug.Log($"Created default database with {questionDatabase.questionSets.Count} question sets");
    }
    
    private void SetupSessionQuestions()
    {
        Debug.Log("Setting up session questions...");
        
        sessionQuestions.Clear();
        
        // Find questions matching current criteria
        List<QuestionData> availableQuestions = new List<QuestionData>();
        
        foreach (var questionSet in questionDatabase.questionSets)
        {
            if (questionSet.category.ToLower() == currentCategory.ToLower() || 
                questionSet.difficultyLevel == currentDifficultyLevel)
            {
                availableQuestions.AddRange(questionSet.questions);
            }
        }
        
        // If no matching questions, use all questions
        if (availableQuestions.Count == 0)
        {
            foreach (var questionSet in questionDatabase.questionSets)
            {
                availableQuestions.AddRange(questionSet.questions);
            }
        }
        
        // Select questions for this session
        if (randomizeQuestions)
        {
            // Shuffle the list
            for (int i = 0; i < availableQuestions.Count; i++)
            {
                QuestionData temp = availableQuestions[i];
                int randomIndex = Random.Range(i, availableQuestions.Count);
                availableQuestions[i] = availableQuestions[randomIndex];
                availableQuestions[randomIndex] = temp;
            }
        }
        
        // Take the required number of questions
        int questionsToTake = Mathf.Min(questionsPerSession, availableQuestions.Count);
        for (int i = 0; i < questionsToTake; i++)
        {
            sessionQuestions.Add(availableQuestions[i]);
        }
        
        currentQuestionIndex = 0;
        Debug.Log($"Setup {sessionQuestions.Count} questions for this session");
    }
    
    public void LoadCurrentQuestion()
    {
        if (sessionQuestions.Count == 0)
        {
            Debug.LogError("No questions available in session!");
            return;
        }
        
        if (currentQuestionIndex >= sessionQuestions.Count)
        {
            Debug.Log("All questions completed!");
            // Could trigger completion logic here
            return;
        }
        
        currentQuestion = sessionQuestions[currentQuestionIndex];
        Debug.Log($"=== LOADING QUESTION {currentQuestionIndex + 1}/{sessionQuestions.Count} ===");
        Debug.Log($"Question: {currentQuestion.questionText}");
        Debug.Log($"Type: {currentQuestion.questionType}");
        Debug.Log($"Correct Answers: {currentQuestion.correctAnswers.Count}");
        
        // Apply question to GameStageManager
        ApplyQuestionToGameManager();
    }
    
    private void ApplyQuestionToGameManager()
    {
        if (gameStageManager == null)
        {
            gameStageManager = FindFirstObjectByType<GameStageManager>();
            if (gameStageManager == null)
            {
                Debug.LogError("GameStageManager not found!");
                return;
            }
        }
        
        // Apply question settings (only if properties exist)
        gameStageManager.currentQuestion = currentQuestion.questionText;
        gameStageManager.maxAnswers = currentQuestion.maxAnswers;
        gameStageManager.minimumAnswersRequired = currentQuestion.minimumAnswersRequired;
        gameStageManager.allowDuplicateAnswers = currentQuestion.allowDuplicateAnswers;
        gameStageManager.caseSensitiveAnswers = currentQuestion.caseSensitiveAnswers;
        gameStageManager.passingScorePercentage = currentQuestion.passingScorePercentage;
        
        // Update the correct answers in GameStageManager
        gameStageManager.UpdateCorrectAnswers(currentQuestion.correctAnswers);
        
        // Update question text in UI
        if (gameStageManager.questionText != null)
        {
            gameStageManager.questionText.text = currentQuestion.questionText;
        }
        
        // Setup multiple choice options if applicable
        if (currentQuestion.questionType == "multiple_choice")
        {
            SetupMultipleChoiceOptions();
        }
        
        Debug.Log($"Applied question to GameStageManager: {currentQuestion.questionText}");
    }
    
    private void SetupMultipleChoiceOptions()
    {
        if (gameStageManager.answerToggles == null || gameStageManager.answerToggles.Count == 0)
        {
            Debug.LogWarning("No toggle UI elements found for multiple choice question");
            return;
        }
        
        // Clear existing selections
        foreach (var toggle in gameStageManager.answerToggles)
        {
            if (toggle != null)
            {
                toggle.isOn = false;
            }
        }
        
        // Apply options to toggles
        for (int i = 0; i < gameStageManager.answerToggles.Count && i < currentQuestion.multipleChoiceOptions.Count; i++)
        {
            var toggle = gameStageManager.answerToggles[i];
            if (toggle != null)
            {
                var toggleText = toggle.GetComponentInChildren<TMPro.TMP_Text>();
                if (toggleText != null)
                {
                    toggleText.text = currentQuestion.multipleChoiceOptions[i];
                    toggle.gameObject.SetActive(true);
                }
            }
        }
        
        // Hide unused toggles
        for (int i = currentQuestion.multipleChoiceOptions.Count; i < gameStageManager.answerToggles.Count; i++)
        {
            if (gameStageManager.answerToggles[i] != null)
            {
                gameStageManager.answerToggles[i].gameObject.SetActive(false);
            }
        }
        
        Debug.Log($"Setup {currentQuestion.multipleChoiceOptions.Count} multiple choice options");
    }
    
    public void NextQuestion()
    {
        currentQuestionIndex++;
        LoadCurrentQuestion();
    }
    
    public bool HasMoreQuestions()
    {
        return currentQuestionIndex < sessionQuestions.Count - 1;
    }
    
    public bool IsAnswerCorrect(string answer)
    {
        if (currentQuestion == null) return false;
        
        string normalizedAnswer = currentQuestion.caseSensitiveAnswers ? answer : answer.ToLower();
        
        foreach (string correctAnswer in currentQuestion.correctAnswers)
        {
            string normalizedCorrect = currentQuestion.caseSensitiveAnswers ? correctAnswer : correctAnswer.ToLower();
            if (normalizedAnswer == normalizedCorrect)
            {
                return true;
            }
        }
        
        return false;
    }
    
    public bool IsMultipleChoiceAnswerCorrect(int optionIndex)
    {
        if (currentQuestion == null || currentQuestion.questionType != "multiple_choice")
            return false;
            
        return currentQuestion.correctMultipleChoiceIndices.Contains(optionIndex);
    }
    
    public float CalculateMultipleChoiceScore(List<int> selectedIndices)
    {
        if (currentQuestion == null || selectedIndices.Count == 0)
            return 0f;
            
        int correctCount = 0;
        foreach (int index in selectedIndices)
        {
            if (currentQuestion.correctMultipleChoiceIndices.Contains(index))
            {
                correctCount++;
            }
        }
        
        return (float)correctCount / selectedIndices.Count * 100f;
    }
    
    // Public API for external access
    public QuestionData GetCurrentQuestion() => currentQuestion;
    public List<QuestionData> GetSessionQuestions() => new List<QuestionData>(sessionQuestions);
    public int GetCurrentQuestionIndex() => currentQuestionIndex;
    public int GetTotalQuestionsInSession() => sessionQuestions.Count;
    
    // Configuration methods
    public void SetCategory(string category)
    {
        currentCategory = category;
        Debug.Log($"Question category set to: {category}");
    }
    
    public void SetDifficultyLevel(int level)
    {
        currentDifficultyLevel = level;
        Debug.Log($"Difficulty level set to: {level}");
    }
    
    public void SetQuestionsPerSession(int count)
    {
        questionsPerSession = count;
        Debug.Log($"Questions per session set to: {count}");
    }
}

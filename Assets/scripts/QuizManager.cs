using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class QuestionData
{
    [Tooltip("Kosakata dalam Bahasa Indonesia yang ditanyakan (misal: Pisang, Rumah, Buku)")]
    public string questionIndonesian;

    [Tooltip("Gambar pendukung untuk soal")]
    public Sprite questionImage;

    [Tooltip("Jawaban benar dalam Bahasa Inggris (misal: Banana, House, Book)")]
    public string correctAnswer;

    [Tooltip("Pilihan A")]
    public string answerA;

    [Tooltip("Pilihan B")]
    public string answerB;

    [Tooltip("Pilihan C")]
    public string answerC;

    public string GetIndonesianWord()
    {
        if (!string.IsNullOrWhiteSpace(questionIndonesian))
            return questionIndonesian.ToUpper();

        // Fallback default otomatis jika belum sempat diisi di Inspector
        switch (correctAnswer?.Trim().ToLower())
        {
            case "banana": return "PISANG";
            case "house": return "RUMAH";
            case "book": return "BUKU";
            case "school": return "SEKOLAH";
            case "happy": return "SENANG";
            case "apple": return "APEL";
            case "orange": return "JERUK";
            default: return correctAnswer?.ToUpper() ?? "SOAL";
        }
    }
}

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    [Header("Prefab")]
    public GameObject answerBoxPrefab;

    [Header("Question UI")]
    [Tooltip("Panel UI soal yang tampil di depan pandangan player")]
    public GameObject questionPanel;

    [Tooltip("Teks kosakata Bahasa Indonesia pada panel soal")]
    public TMP_Text questionText;

    [Tooltip("Gambar pendukung pada panel soal")]
    public Image questionImage;

    [Header("Question List")]
    public QuestionData[] questions;

    [Header("Position & Spacing")]
    [Tooltip("Posisi Z awal soal pertama")]
    public float firstQuestionZ = 20f;

    [Tooltip("Jarak antara posisi player saat berhasil menjawab dengan soal berikutnya")]
    public float questionSpacing = 25f;

    [Tooltip("Jarak Z antar box A, B, dan C pada satu soal agar tidak terlalu mepet dan mudah dihindari")]
    public float boxSpacing = 12f;

    [Tooltip("Jarak jalur horizontal kiri (-laneDistance), tengah (0), dan kanan (+laneDistance)")]
    public float laneDistance = 3f;

    [Tooltip("Jika di-centang, box A, B, C muncul sejajar di baris Z yang sama. Jika tidak, staggered berjarak.")]
    public bool spawnInSameRow = false;

    [Header("Score")]
    public int score = 0;

    [Header("References")]
    public Transform player;

    private int currentQuestion = 0;
    private GameObject[] currentAnswers;
    private bool isFirstQuestion = true;
    private bool isProcessingAnswer = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        AutoFindAndSetupUI();
        ExtendRoadIfNeeded();
        SpawnQuestion();
    }

    private void AutoFindAndSetupUI()
    {
        if (questionPanel == null)
        {
            questionPanel = GameObject.Find("QuestionPanel");
        }

        if (questionPanel != null)
        {
            if (questionText == null)
            {
                questionText = questionPanel.GetComponentInChildren<TMP_Text>();
            }

            if (questionImage == null)
            {
                Transform imgTr = questionPanel.transform.Find("QuestionImage");
                if (imgTr != null)
                {
                    questionImage = imgTr.GetComponent<Image>();
                }
                else
                {
                    Image[] imgs = questionPanel.GetComponentsInChildren<Image>();
                    foreach (var img in imgs)
                    {
                        if (img.gameObject != questionPanel)
                        {
                            questionImage = img;
                            break;
                        }
                    }
                }
            }

            SetupPanelLayout();
        }
    }

    private void SetupPanelLayout()
    {
        if (questionPanel == null) return;

        RectTransform panelRect = questionPanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.localScale = Vector3.one;
            panelRect.localRotation = Quaternion.identity;

            // Posisikan di tengah atas layar, jelas dan nyaman dilihat di depan pandangan player
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -25f);
            panelRect.sizeDelta = new Vector2(300f, 190f);

            Image panelBg = questionPanel.GetComponent<Image>();
            if (panelBg != null)
            {
                panelBg.color = new Color(0.12f, 0.15f, 0.22f, 0.88f);
            }
        }

        if (questionText != null)
        {
            RectTransform textRect = questionText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.localScale = Vector3.one;
                textRect.anchorMin = new Vector2(0.5f, 1f);
                textRect.anchorMax = new Vector2(0.5f, 1f);
                textRect.pivot = new Vector2(0.5f, 1f);
                textRect.anchoredPosition = new Vector2(0f, -15f);
                textRect.sizeDelta = new Vector2(280f, 45f);
            }
            questionText.alignment = TextAlignmentOptions.Center;
            questionText.fontSize = 32f;
            questionText.fontStyle = FontStyles.Bold;
            questionText.color = Color.white;
        }

        if (questionImage != null)
        {
            RectTransform imgRect = questionImage.GetComponent<RectTransform>();
            if (imgRect != null)
            {
                imgRect.localScale = Vector3.one;
                imgRect.anchorMin = new Vector2(0.5f, 1f);
                imgRect.anchorMax = new Vector2(0.5f, 1f);
                imgRect.pivot = new Vector2(0.5f, 1f);
                imgRect.anchoredPosition = new Vector2(0f, -68f);
                imgRect.sizeDelta = new Vector2(105f, 105f);
            }
            questionImage.preserveAspect = true;
        }
    }

    public void SpawnQuestion()
    {
        if (questions == null || questions.Length == 0)
        {
            Debug.LogWarning("Belum ada soal di QuizManager!");
            return;
        }

        QuestionData question = questions[currentQuestion];

        UpdateQuestionPanelUI(question);

        // Tentukan posisi awal Z untuk pertanyaan ini
        float startZ;
        if (isFirstQuestion)
        {
            startZ = firstQuestionZ;
            isFirstQuestion = false;
        }
        else
        {
            // Spawn di depan player berdasarkan posisi terkini player + questionSpacing
            float playerZ = player != null ? player.position.z : 0f;
            startZ = playerZ + questionSpacing;
        }

        currentAnswers = new GameObject[3];

        float zOffsetB = spawnInSameRow ? 0f : boxSpacing;
        float zOffsetC = spawnInSameRow ? 0f : (boxSpacing * 2f);

        // Box A di jalur kiri (-laneDistance)
        currentAnswers[0] = SpawnAnswer(
            question.answerA,
            -laneDistance,
            startZ,
            new Color(0.2f, 0.6f, 1f)
        );

        // Box B di jalur tengah (0)
        currentAnswers[1] = SpawnAnswer(
            question.answerB,
            0f,
            startZ + zOffsetB,
            new Color(1f, 0.75f, 0.2f)
        );

        // Box C di jalur kanan (+laneDistance)
        currentAnswers[2] = SpawnAnswer(
            question.answerC,
            laneDistance,
            startZ + zOffsetC,
            new Color(0.9f, 0.3f, 0.6f)
        );

        isProcessingAnswer = false;
    }

    private void UpdateQuestionPanelUI(QuestionData question)
    {
        if (question == null) return;

        if (questionPanel != null && !questionPanel.activeSelf)
        {
            questionPanel.SetActive(true);
        }

        if (questionText != null)
        {
            questionText.text = question.GetIndonesianWord();
        }

        if (questionImage != null)
        {
            Sprite spriteToUse = GetQuestionSprite(question);
            if (spriteToUse != null)
            {
                questionImage.sprite = spriteToUse;
                questionImage.gameObject.SetActive(true);
            }
            else
            {
                questionImage.gameObject.SetActive(false);
            }
        }
    }

    private Sprite GetQuestionSprite(QuestionData question)
    {
        if (question.questionImage != null)
            return question.questionImage;

        #if UNITY_EDITOR
        if (question.correctAnswer?.Trim().ToLower() == "banana")
        {
            string path = "Assets/Images/banana-graphic-clipart-design-free-png.jpg";
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;
        }
        #endif

        return null;
    }

    private GameObject SpawnAnswer(
        string answer,
        float x,
        float z,
        Color color
    )
    {
        if (answerBoxPrefab == null)
        {
            Debug.LogError("answerBoxPrefab belum dipasang pada QuizManager!");
            return null;
        }

        GameObject box = Instantiate(
            answerBoxPrefab,
            new Vector3(x, 1f, z),
            Quaternion.identity
        );

        AnswerBox answerBox = box.GetComponent<AnswerBox>();

        if (answerBox != null)
        {
            answerBox.Setup(answer, color);
        }

        return box;
    }

    public void CheckAnswer(string answer, AnswerBox hitBox = null)
    {
        if (isProcessingAnswer)
            return;

        if (questions == null || currentQuestion >= questions.Length)
            return;

        QuestionData question = questions[currentQuestion];

        string cleanAnswer = answer != null ? answer.Trim().ToLower() : "";
        string cleanCorrect = question.correctAnswer != null ? question.correctAnswer.Trim().ToLower() : "";

        if (cleanAnswer == cleanCorrect)
        {
            CorrectAnswer(hitBox);
        }
        else
        {
            WrongAnswer();
        }
    }

    private void CorrectAnswer(AnswerBox hitBox)
    {
        isProcessingAnswer = true;
        score += 10;

        Debug.Log("CORRECT! Score: " + score);

        // Update score di EnglishRunnerGame & tampilkan popup UI
        if (EnglishRunnerGame.Instance != null)
        {
            EnglishRunnerGame.Instance.AddScore(10);
            EnglishRunnerGame.Instance.ShowWord("CORRECT! +10");
        }

        // Jalankan efek cling pada box yang ditabrak
        if (hitBox != null)
        {
            hitBox.PlayClingAndDisappear();
        }

        // Hilangkan sisa box lainnya secara halus
        DismissOtherAnswers(hitBox != null ? hitBox.gameObject : null);

        // Lanjut ke soal berikutnya sesuai Question List
        currentQuestion++;

        if (currentQuestion >= questions.Length)
        {
            currentQuestion = 0; // Loop kembali ke soal pertama jika sudah habis
        }

        SpawnQuestion();
    }

    private void WrongAnswer()
    {
        isProcessingAnswer = true;
        Debug.Log("WRONG ANSWER!");

        if (EnglishRunnerGame.Instance != null)
        {
            EnglishRunnerGame.Instance.GameOver();
        }
    }

    private void DismissOtherAnswers(GameObject hitBoxObj)
    {
        if (currentAnswers == null)
            return;

        foreach (GameObject obj in currentAnswers)
        {
            if (obj != null && obj != hitBoxObj)
            {
                AnswerBox box = obj.GetComponent<AnswerBox>();
                if (box != null)
                {
                    box.FadeAndDisappear();
                }
                else
                {
                    Destroy(obj);
                }
            }
        }

        currentAnswers = null;
    }

    private void ExtendRoadIfNeeded()
    {
        // Otomatis perpanjang Road dan LaneLine jika masih pendek agar player tidak jatuh setelah soal pertama
        GameObject[] grounds = GameObject.FindGameObjectsWithTag("Ground");
        foreach (GameObject ground in grounds)
        {
            if (ground.name.Contains("Road") && ground.transform.localScale.z < 1000f)
            {
                Vector3 scale = ground.transform.localScale;
                scale.z = 1000f;
                ground.transform.localScale = scale;

                Vector3 pos = ground.transform.position;
                pos.z = 490f;
                ground.transform.position = pos;
            }
        }

        GameObject leftLine = GameObject.Find("LaneLineLeft");
        if (leftLine != null && leftLine.transform.localScale.z < 1000f)
        {
            leftLine.transform.localScale = new Vector3(leftLine.transform.localScale.x, leftLine.transform.localScale.y, 1000f);
            leftLine.transform.position = new Vector3(leftLine.transform.position.x, leftLine.transform.position.y, 490f);
        }

        GameObject rightLine = GameObject.Find("LaneLineRight");
        if (rightLine != null && rightLine.transform.localScale.z < 1000f)
        {
            rightLine.transform.localScale = new Vector3(rightLine.transform.localScale.x, rightLine.transform.localScale.y, 1000f);
            rightLine.transform.position = new Vector3(rightLine.transform.position.x, rightLine.transform.position.y, 490f);
        }
    }
}
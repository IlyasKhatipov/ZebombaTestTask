using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Pendulum hand;
    public HingeJoint2D ballPrefab;
    public Transform ballSpawnPoint;
    public float destroyDelay = 0.5f;
    public GameObject gameOverCanvas;
    public float dropCooldown = 0.5f;
    public TextMeshProUGUI ScoresText;
    public int RedScore = 1;
    public int GreenScore = 2;
    public int BlueScore = 3;
    public int maxHeight = 3;

    private bool isOnCooldown = false;
    private HingeJoint2D currentBall;
    private int totalScore = 0;


    private List<GameObject>[] columns = new List<GameObject>[3];

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            columns[i] = new List<GameObject>();
        }

        currentBall = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation).gameObject.GetComponent<HingeJoint2D>();
        currentBall.connectedBody = hand.GetComponent<Rigidbody2D>();
        SetRandomBallColor(currentBall.gameObject);
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        ScoresText.text = "Score: " + totalScore.ToString();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) ||
           (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            if (!isOnCooldown)
            {
                DropBall();
                StartCoroutine(StartCooldown());
            }
        }
        CheckMatches();
    }
    IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(dropCooldown);
        isOnCooldown = false;
    }

    void SpawnBall()
    {
        currentBall = Instantiate(ballPrefab, currentBall.gameObject.transform.position, currentBall.gameObject.transform.rotation);
        currentBall.connectedBody = hand.GetComponent<Rigidbody2D>();
        SetRandomBallColor(currentBall.gameObject);
    }

    void DropBall()
    {
        currentBall.connectedBody = null;
        currentBall.enabled = false;
        SpawnBall();
    }

    void SetRandomBallColor(GameObject ball)
    {
        SpriteRenderer sr = ball.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        int colorId = Random.Range(0, 3);
        switch (colorId)
        {
            case 0: sr.color = Color.red; break;
            case 1: sr.color = Color.green; break;
            case 2: sr.color = Color.blue; break;
        }
    }

    public void OnBallLanded(int columnIndex, GameObject ball)
    {
        List<GameObject> stack = columns[columnIndex];
        stack.Add(ball);

        if (stack.Count > maxHeight)
        {
            Debug.Log($"Game Over: column {columnIndex} overflowed.");
            return;
        }
    }

    void CheckMatches()
    {
        StartCoroutine(CheckMatchesWithDelay());
    }

    IEnumerator CheckMatchesWithDelay()
    {
        bool columnMatch = CheckColumns();
        bool rowMatch = CheckRows();
        bool diagonalMatch = CheckDiagonals();

        if (columnMatch || rowMatch || diagonalMatch)
        {
            yield return new WaitForSeconds(destroyDelay);
        }
        else
        {
            bool allColumnsFull = true;
            for (int i = 0; i < 3; i++)
            {
                if (columns[i].Count < 3)
                {
                    allColumnsFull = false;
                    break;
                }
            }

            if (allColumnsFull)
            {
                yield return new WaitForSeconds(destroyDelay);
                Debug.Log("Game Over - all columns are full with no matches!");
                gameOverCanvas.SetActive(true);

                Time.timeScale = 0;
                enabled = false; 
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu(int ind) 
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(ind);
    }

    bool CheckColumns()
    {
        bool foundMatch = false;
        for (int col = 0; col < 3; col++)
        {
            if (columns[col].Count >= 3)
            {
                int c = columns[col].Count;
                var b0 = columns[col][c - 1].GetComponent<SpriteRenderer>().color;
                var b1 = columns[col][c - 2].GetComponent<SpriteRenderer>().color;
                var b2 = columns[col][c - 3].GetComponent<SpriteRenderer>().color;

                if (b0 == b1 && b1 == b2)
                {
                    Debug.Log($"3 in a column {col}!");
                    StartCoroutine(RemoveMatchedBallsWithDelay(col, c - 3, 3));
                    foundMatch = true;
                }
            }
        }
        return foundMatch;
    }

    bool CheckRows()
    {
        bool foundMatch = false;
        int maxRow = 0;
        for (int i = 0; i < 3; i++)
        {
            if (columns[i].Count > maxRow)
                maxRow = columns[i].Count;
        }

        for (int row = 0; row < maxRow; row++)
        {
            if (columns[0].Count > row && columns[1].Count > row && columns[2].Count > row)
            {
                var b0 = columns[0][row].GetComponent<SpriteRenderer>().color;
                var b1 = columns[1][row].GetComponent<SpriteRenderer>().color;
                var b2 = columns[2][row].GetComponent<SpriteRenderer>().color;

                if (b0 == b1 && b1 == b2)
                {
                    Debug.Log($"3 in a row at height {row}!");
                    StartCoroutine(RemoveMatchedBallsWithDelay(0, row, 1));
                    StartCoroutine(RemoveMatchedBallsWithDelay(1, row, 1));
                    StartCoroutine(RemoveMatchedBallsWithDelay(2, row, 1));
                    foundMatch = true;
                }
            }
        }
        return foundMatch;
    }

    bool CheckDiagonals()
    {
        bool foundMatch = false;
        if (CheckDiagonalDescending()) foundMatch = true;
        if (CheckDiagonalAscending()) foundMatch = true;
        return foundMatch;
    }

    bool CheckDiagonalDescending()
    {
        bool foundMatch = false;
        for (int h = 2; h < columns[0].Count; h++)
        {
            if (columns[1].Count > h - 1 && columns[2].Count > h - 2)
            {
                var b0 = columns[0][h].GetComponent<SpriteRenderer>().color;
                var b1 = columns[1][h - 1].GetComponent<SpriteRenderer>().color;
                var b2 = columns[2][h - 2].GetComponent<SpriteRenderer>().color;

                if (b0 == b1 && b1 == b2)
                {
                    Debug.Log("3 in a descending diagonal!");
                    StartCoroutine(RemoveMatchedBallsWithDelay(0, h, 1));
                    StartCoroutine(RemoveMatchedBallsWithDelay(1, h - 1, 1));
                    StartCoroutine(RemoveMatchedBallsWithDelay(2, h - 2, 1));
                    foundMatch = true;
                }
            }
        }
        return foundMatch;
    }

    bool CheckDiagonalAscending()
    {
        bool foundMatch = false;
        for (int h = 2; h < columns[2].Count; h++)
        {
            if (columns[1].Count > h - 1 && columns[0].Count > h - 2)
            {
                var b0 = columns[2][h].GetComponent<SpriteRenderer>().color;
                var b1 = columns[1][h - 1].GetComponent<SpriteRenderer>().color;
                var b2 = columns[0][h - 2].GetComponent<SpriteRenderer>().color;

                if (b0 == b1 && b1 == b2)
                {
                    Debug.Log("3 in an ascending diagonal!");
                    StartCoroutine(RemoveMatchedBallsWithDelay(2, h, 1));
                    StartCoroutine(RemoveMatchedBallsWithDelay(1, h - 1, 1));
                    StartCoroutine(RemoveMatchedBallsWithDelay(0, h - 2, 1));
                    foundMatch = true;
                }
            }
        }
        return foundMatch;
    }

    IEnumerator RemoveMatchedBallsWithDelay(int column, int startIndex, int count)
    {
        yield return new WaitForSeconds(destroyDelay);

        List<GameObject> ballsToDestroy = new List<GameObject>();
        for (int i = startIndex + count - 1; i >= startIndex; i--)
        {
            if (i < columns[column].Count)
            {
                ballsToDestroy.Add(columns[column][i]);
                columns[column].RemoveAt(i);
            }
        }

        foreach (var ball in ballsToDestroy)
        {
            var ballComponent = ball.GetComponent<ball>();
            if (ballComponent != null)
            {
                ballComponent.OnDestroy();

                SpriteRenderer sr = ball.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (sr.color == Color.red)
                        totalScore += RedScore;
                    else if (sr.color == Color.green)
                        totalScore += GreenScore;
                    else if (sr.color == Color.blue)
                        totalScore += BlueScore;
                }
            }
        }

        UpdateScoreText();

        foreach (var ball in ballsToDestroy)
        {
            var ballComponent = ball.GetComponent<ball>();
            if (ballComponent != null)
            {
                ballComponent.OnDestroy(); 
            }
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game")]
    [SerializeField]
    private float m_TimeLimit = 180f;

    [Header("Scene")]
    [SerializeField]
    private string m_GameSceneName = "Game";

    [SerializeField]
    private string m_ResultSceneName = "Result";

    [SerializeField]
    private string m_TitleSceneName = "Start";

    private float m_RemainingTime;

    private int m_RescuedPenguins;
    private int m_LostPenguins;
    private int m_Score;

    private bool m_IsGameOver;

    public float RemainingTime => m_RemainingTime;
    public int RescuedPenguins => m_RescuedPenguins;
    public int LostPenguins => m_LostPenguins;
    public int Score => m_Score;
    public bool IsGameOver => m_IsGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ResetGame();
    }

    private void Update()
    {
        // ゲーム終了後はタイマーを動かさない
        if (m_IsGameOver)
        {
            return;
        }

        // ゲームシーン以外ではタイマーを動かさない
        if (SceneManager.GetActiveScene().name != m_GameSceneName)
        {
            return;
        }

        m_RemainingTime -= Time.deltaTime;

        if (m_RemainingTime <= 0f)
        {
            m_RemainingTime = 0f;
            FinishGame();
        }
    }

    public void AddPenguin()
    {
        if (m_IsGameOver)
        {
            return;
        }

        m_RescuedPenguins++;
    }

    public void AddLostPenguin()
    {
        if (m_IsGameOver)
        {
            return;
        }

        m_LostPenguins++;
    }

    /// <summary>
    /// ゴール時・時間切れ時の共通終了処理
    /// </summary>
    public void FinishGame()
    {
        if (m_IsGameOver)
        {
            return;
        }

        // 最初にtrueにして、これ以降タイマーを止める
        m_IsGameOver = true;

        SaveResult();

        SceneManager.LoadScene(m_ResultSceneName);
    }

    public void SaveResult()
    {
        m_Score =
            m_RescuedPenguins * 100 +
            Mathf.FloorToInt(m_RemainingTime) * 10;
    }

    public void ResetGame()
    {
        m_RemainingTime = m_TimeLimit;
        m_RescuedPenguins = 0;
        m_LostPenguins = 0;
        m_Score = 0;
        m_IsGameOver = false;
    }

    public void StartGame()
    {
        ResetGame();
        SceneManager.LoadScene(m_GameSceneName);
    }

    public void ReturnToTitle()
    {
        ResetGame();
        SceneManager.LoadScene(m_TitleSceneName);
    }

    public string GetRank()
    {
        if (m_Score >= 2000)
        {
            return "S";
        }

        if (m_Score >= 1500)
        {
            return "A";
        }

        if (m_Score >= 1000)
        {
            return "B";
        }

        return "C";
    }
}
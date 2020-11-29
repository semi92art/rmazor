using Helpers;

public delegate void ScoreHandler(int _Score);

public delegate void NoArgsHandler();

public interface IScoreController
{
    event ScoreHandler OnScoreChanged;
    event ScoreHandler OnNecessaryScoreChanged;
    event NoArgsHandler OnNecessaryScoreReached;
    int Score { get; set; }
    int NecessaryScore { get; set; }
}


public class ScoreController : IScoreController
{
    private int m_Score;
    private int m_NecessaryScore;
    
    public event ScoreHandler OnScoreChanged;
    public event ScoreHandler OnNecessaryScoreChanged;
    public event NoArgsHandler OnNecessaryScoreReached;

    public int Score
    {
        get => m_Score;
        set
        {
            m_Score = value;
            OnScoreChanged?.Invoke(value);
            if (m_Score >= m_NecessaryScore && m_NecessaryScore != 0)
                OnNecessaryScoreReached?.Invoke();
        }
    }
    
    public int NecessaryScore
    {
        get => m_NecessaryScore;
        set
        {
            m_NecessaryScore = value;
            OnNecessaryScoreChanged?.Invoke(value);
        }
    }
}

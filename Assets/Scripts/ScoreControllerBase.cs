public interface IScoringModel
{
    event IntHandler OnScoreChanged;
    event IntHandler OnNecessaryScoreChanged;
    event NoArgsHandler OnNecessaryScoreReached;
    int Score { get; set; }
    int NecessaryScore { get; set; }
}

public abstract class ScoringModelBase : IScoringModel
{
    private int m_Score;
    private int m_NecessaryScore;
    
    public event IntHandler OnScoreChanged;
    public event IntHandler OnNecessaryScoreChanged;
    public event NoArgsHandler OnNecessaryScoreReached;

    public int Score
    {
        get => m_Score;
        set
        {
            OnScoreChanged?.Invoke(m_Score = value);
            if (m_Score >= m_NecessaryScore && m_NecessaryScore != 0)
                OnNecessaryScoreReached?.Invoke();
        }
    }
    
    public int NecessaryScore
    {
        get => m_NecessaryScore;
        set => OnNecessaryScoreChanged?.Invoke(m_NecessaryScore = value);
    }
}

public class ScoringModelDefault : ScoringModelBase
{
    
}

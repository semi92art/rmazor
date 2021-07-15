public interface IScoringModel
{
    event IntHandler ScoreChanged;
    event IntHandler OnNecessaryScoreChanged;
    event NoArgsHandler NecessaryScoreReached;
    int Score { get; set; }
    int NecessaryScore { get; set; }
}

public abstract class ScoringModelBase : IScoringModel
{
    private int m_Score;
    private int m_NecessaryScore;
    
    public event IntHandler ScoreChanged;
    public event IntHandler OnNecessaryScoreChanged;
    public event NoArgsHandler NecessaryScoreReached;

    public int Score
    {
        get => m_Score;
        set
        {
            ScoreChanged?.Invoke(m_Score = value);
            if (m_Score >= m_NecessaryScore && m_NecessaryScore != 0)
                NecessaryScoreReached?.Invoke();
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

using UnityEngine;

[System.Serializable]
public struct UiState
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Color color;

    public Sprite Sprite => sprite;
    public Color Color => color;
}

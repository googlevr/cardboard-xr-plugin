using UnityEngine;

[CreateAssetMenu(fileName = "XRCardboardInputSettings", menuName = "Google Cardboard/Cardboard Input Settings")]
public class XRCardboardInputSettings : ScriptableObject
{
    public string ClickInput => clickInputName;
    public float GazeTime => gazeTimeInSeconds;

    [SerializeField]
    string clickInputName = "Submit";
    [SerializeField, Range(.5f, 5)]
    float gazeTimeInSeconds = 2f;
}
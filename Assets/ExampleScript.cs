using UnityEngine;

public class ExampleScript : MonoBehaviour
{

    //表示したログ、SerializeFieldを付ける事でInspectorに表示されるように
    [SerializeField]
    private string _log = "";

    /// <summary>
    /// _logを"ぷぅあぶりっく！"に変更してConsoleに表示
    /// </summary>
    public void PublicMethod ()
    {
        _log = "ぷぅあぶりっく！";
        Debug.Log(_log);
    }

    [ContextMenu("Do ContestMenu")]
    void hogehoge ()
    {
        Debug.Log("Exe ContestMenu!!");
    }
}
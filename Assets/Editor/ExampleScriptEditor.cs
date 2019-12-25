using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ExampleScript))]//拡張するクラスを指定
public class ExampleScriptEditor : Editor
{

    Vector2 buttonSize = new Vector2(100, 20);

    Vector2 buttonMinSize = new Vector2(100, 20);
    Vector2 buttonMaxSize = new Vector2(1000, 200);

    /// <summary>
    /// InspectorのGUIを更新
    /// </summary>
    public override void OnInspectorGUI ()
    {
        //元のInspector部分を表示
        base.OnInspectorGUI();

        //ボタンを表示
        if(GUILayout.Button("Button", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y)))
        {
            Debug.Log("押した!");
        }

        //targetを変換して対象を取得
        ExampleScript exampleScript = target as ExampleScript;

        //PublicMethodを実行する用のボタン
        if(GUILayout.Button("PublicMethod"))
        {
            exampleScript.PublicMethod();
        }
    }

}
// AnimatorCreator.cs
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

/// <summary>
/// Animator / Animation Clip を自動生成
/// </summary>
public static class AnimatorCreator
{
    /// <summary>ファイルを置く場所</summary>
    private const string AssetPath = "Assets/Animations/Test";

    /// <summary>パラメータ名</summary>
    private const string ParamName = "Param";

    /// <summary>
    /// 作成
    /// </summary>
    [MenuItem("Tools/Animation/Create Animator")]
    public static void Create ()
    {
        // 選択したオブジェクトをベースにする
        var obj = Selection.activeGameObject;
        if(obj == null)
        {
            Debug.LogErrorFormat("InfoObjがアタッチされたGameObjectを選択した状態で起動してください。");
            return;
        }

        var info = obj.GetComponent<InfoObj>();
        if(info == null)
        {
            Debug.LogErrorFormat("選択されているオブジェクトにInfoObjがアタッチされていません。処理を中止します。");
            return;
        }

        // animator作成
        if(!Directory.Exists(AssetPath))
        {
            Directory.CreateDirectory(AssetPath);
        }

        var path = string.Format("{0}/Anim{1}.controller", AssetPath, info.index);
        if(File.Exists(path))
        {
            if(!EditorUtility.DisplayDialog("Overwrite Confirmation", string.Format("{0}はすでに存在します。上書きして続行しますか？", path), "OK", "Cancel"))
            {
                Debug.LogFormat("操作はキャンセルされました。");
                return;
            }
        }

        // AnimatorController作成
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(path);
        animatorController.AddParameter(ParamName, AnimatorControllerParameterType.Int);

        AnimatorStateMachine stateMachine = animatorController.layers[0].stateMachine;

        var states = new List<AnimatorState>();

        // Stateの作成
        for(int i = 0; i < info.count; i++)
        {
            var clip = new AnimationClip();
            clip.wrapMode = WrapMode.Clamp;
            clip.name = string.Format("Anim{0}_Eff{1}", info.index, i);
            AssetDatabase.CreateAsset(clip, string.Format("{0}/{1}.anim", AssetPath, clip.name));
            var state = stateMachine.AddState(clip.name, new Vector2(320, 0 + i * 70));
            state.motion = clip;
            state.writeDefaultValues = false;
            states.Add(state);
        }

        // Transitionの作成
        stateMachine.defaultState = states[0];
        for(int i = 0; i < stateMachine.states.Length - 1; i++)
        {
            var transition = stateMachine.states[i].state.AddTransition(stateMachine.states[i + 1].state);
            transition.AddCondition(AnimatorConditionMode.Equals, i + 1, ParamName);
            transition.hasExitTime = false;
            transition.duration = 0;
        }

        // 設定反映
        var animator = obj.GetComponent<Animator>();
        if(animator == null)
        {
            obj.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = animatorController;

        // セーブ
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(animatorController);
    }
}
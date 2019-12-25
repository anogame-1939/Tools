// AnimatorCreator.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Animator / Animation Clip を自動生成
/// </summary>
public class AnimatorCreator : MonoBehaviour
{
    /// <summary>ファイルを置く場所</summary>
    private const string AssetPath = "Assets/Animations/Test";

    /// <summary>パラメータ名</summary>
    private const string ParamName = "Param";

    private const string Idle = "idle";

    private const string Walk = "walk";

    private const string Run = "run";

    private const string Attack = "attack";

    private const string Defense = "defense";

    private const string Dameged = "dameged";

    private const string Die = "die";

    // private const string[] StateNames = {"idle", "idle"}



    private static readonly 
        ReadOnlyCollection<string> BaseStateNames 
        = Array.AsReadOnly(new string[] {
            "idle",
            "walk",
            "run",
            "attack",
            "defense",
            "dameged",
            "die"
        });


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
            Debug.LogErrorFormat("GameObjectを選択した状態で起動してください。");
            return;
        }

        var animation = obj.GetComponent<Animation>();
        if(animation == null)
        {
            Debug.LogErrorFormat("選択されているオブジェクトにInfoObjがアタッチされていません。処理を中止します。");
            return;
        }

        // animator作成
        if(!Directory.Exists(AssetPath))
        {
            Directory.CreateDirectory(AssetPath);
        }

        var path = string.Format("{0}/Anim{1}.controller", AssetPath, obj.name);
        if(File.Exists(path))
        {
            if(!EditorUtility.DisplayDialog("Overwrite Confirmation", string.Format("{0}はすでに存在します。上書きして続行しますか？", path), "OK", "Cancel"))
            {
                Debug.LogFormat("操作はキャンセルされました。");
                return;
            }
        }

        // アニメーションクリップを全て取得
        List<AnimationClip> animationClips = new List<AnimationClip>();
        animationClips = GetAnimationClips(obj);


        // AnimatorController作成
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(path);
        SetAnimatorController(animatorController);


        // AnimationClip => AnimatorStateに変換、
        var states = new List<AnimatorState>();
        states = ConvAniClipsToAniStates(animationClips);

        Debug.Log(states.Count);

        // AnimatorStateMachine作成
        AnimatorStateMachine stateMachine = animatorController.layers[0].stateMachine;

        // Transitionの作成
        SetAnimatorStateMachine(stateMachine, states);


        // Animatorに設定反映
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

    /// <summary>
    /// AnimationClipのリストを取得
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    private static List<AnimationClip> GetAnimationClips(GameObject gameObject)
    {
        List<AnimationClip> animationClips = new List<AnimationClip>();
        Animation animation = gameObject.GetComponent<Animation>();

        foreach(AnimationState anim in animation)
        {
            animationClips.Add(anim.clip);
        }
        return animationClips;
    }

    /// <summary>
    /// 対象のアニメーションを検索する
    /// </summary>
    /// <param name="animationStates"></param>
    /// <param name="findName"></param>
    /// <returns></returns>
    /*
    private static AnimationClip FindAnimation (List<AnimationClip> animationClips, string findName)
    {
        foreach(AnimationClip animationClip in animationClips)
        {
            Regex regex = new Regex(findName, RegexOptions.IgnoreCase);
            if(regex.IsMatch(animationClip.name))
            {
                return animationClip;
            }
        }
        return null;
    }
    */


    private static AnimatorState FindAnimation (List<AnimatorState> animationStates, string findName)
    {
        foreach(AnimatorState animationState in animationStates)
        {
            Regex regex = new Regex(findName, RegexOptions.IgnoreCase);
            Debug.Log($"{findName}   ");
            Debug.Log(animationState);
            Debug.Log(animationState.name);
            Debug.Log(animationState.motion);
            Debug.Log($"{animationState.motion.name}");
            if(regex.IsMatch(animationState.motion.name))
            {
                return animationState;
            }
        }
        return null;
    }

    /// <summary>
    /// Animatorの各種セットアップ
    /// </summary>
    /// <param name="animator"></param>
    private static void SetAnimatorController (AnimatorController animatorController)
    {
        foreach(string stateName in BaseStateNames)
        {
            animatorController.AddParameter(stateName, AnimatorControllerParameterType.Bool);
        }
        animatorController.AddParameter(ParamName, AnimatorControllerParameterType.Int);
    }

    /// <summary>
    /// AnimationClipをAnimatorStateに変換
    /// </summary>
    /// <param name="stateMachine"></param>
    /// <param name="animationClips"></param>
    /// <returns></returns>
    private static List<AnimatorState> ConvAniClipsToAniStates (List<AnimationClip> animationClips)
    {
        // Stateの作成
        var states = new List<AnimatorState>();
        for(int i = 0; i < animationClips.Count; i++)
        {
            AnimationClip animationClip = animationClips[0];
            // var state = stateMachine.AddState(animationClip.name, new Vector2(320, 0 + i * 70));
            AnimatorState state = new AnimatorState
            {
                motion = animationClip,
                writeDefaultValues = false
            };
            Debug.Log(animationClip.name);
            state.motion.name = animationClip.name;
            
            Debug.Log(state.motion.name);

            states.Add(state);
        }

        return states;
    }

    private static void SetAnimatorStateMachine (AnimatorStateMachine stateMachine, List<AnimatorState> states)
    {
        // デフォルトはアイドル状態
        stateMachine.defaultState = FindAnimation(states, Idle);

        for(int i = 0; i < states.Count; i++)
        {
            AnimatorState animatorState = stateMachine.states[i].state;

            // デフォルトステートはスキップ
            if(stateMachine.defaultState.Equals(animatorState))
            {
                Debug.Log("デフォルトステートはスキップ");
                continue;
            }

            Regex regex = new Regex($"{animatorState.name}.*", RegexOptions.IgnoreCase);


            // 基本となるステート名と一致したらステートマシンに加える
            foreach(string stateName in BaseStateNames)
            {
                if(regex.IsMatch(Idle))
                {
                    stateMachine.AddState(animatorState.name, new Vector2(320, stateMachine.states.Length * 70));

                    // 全てデフォルトステートとつなぐ
                    var transition = stateMachine.defaultState.AddTransition(animatorState);
                    transition.AddCondition(AnimatorConditionMode.If, 0, ParamName);
                    transition.hasExitTime = false;
                    transition.duration = 0;
                }
            }
            
        }

    }
}
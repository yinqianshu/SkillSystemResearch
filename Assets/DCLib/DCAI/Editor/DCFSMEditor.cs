﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace DC.AI
{
    public class DCFSMEditor : EditorWindow
    {
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(DCFSMEditor));
            var screenResolution = Screen.currentResolution;
            //left,top to center,center
            var w = 300;
            var h = 150;
            var r = new Rect(screenResolution.width * 0.5f - w * 0.5f, screenResolution.height * 0.5f - h * 0.5f, w, h);
            window.position = r;
        }

        public string defaultState;

        void OnGUI()
        {
            defaultState = EditorGUILayout.TextField("DefaultState", defaultState);
            if (string.IsNullOrEmpty(defaultState))
            {
                defaultState = "Idle";
            }

            if (GUILayout.Button("Create"))
            {
                AnimatorControllerToFSMCfg();
                Close();
            }
        }

        [MenuItem("DC/FSM/CreateConfig", false, 1)]
        public static void AnimatorControllerToFSMCfg()
        {
            if (Selection.activeObject is AnimatorController)
            {
                var controller = Selection.activeObject as AnimatorController;

                var converter = new AnimatorControllerToConfig();
                converter.StateToId = StateToId;
                converter.TransToId = TransToId;

                var config = converter.Convert(controller);
                File.WriteAllText(Application.dataPath + "/FSMCfg.bytes", config, Encoding.UTF8);
                AssetDatabase.ImportAsset("Assets/FSMCfg.bytes");
            }
            else
            {
                Debug.Log(Selection.activeObject.name + "is not a controller");
            }
        }

        public static int StateToId(AnimatorState state)
        {
            return state.name.GetExtHashCode();
        }

        public static int TransToId(AnimatorStateTransition transition)
        {
            return ("To" + transition.destinationState.name).GetExtHashCode();
        }
    }

    public class AnimatorControllerToConfig
    {
        public Convert<int, AnimatorState> StateToId;
        public Convert<int, AnimatorStateTransition> TransToId;

        public string Convert(AnimatorController controller)
        {
            var stateToIdDic = new Dictionary<AnimatorState, int>();
            var stateIdToTransState = new Dictionary<int, List<KeyValuePair<int, int>>>();

            var controllerLayer = controller.layers[0];
            var stateMachine = controllerLayer.stateMachine;
            var machineStates = stateMachine.states;
            var defaultState = stateMachine.defaultState;

            //所有状态
            foreach (var machineState in machineStates)
            {
                var animatorState = machineState.state;
                int stateId = StateToId(animatorState);
                stateToIdDic.Add(animatorState, stateId);
                stateIdToTransState.Add(stateId, new List<KeyValuePair<int, int>>());
            }

            //所有明确的状态关系
            foreach (var machineState in machineStates)
            {
                var animatorState = machineState.state;
                var stateId = stateToIdDic[animatorState];
                var transRelations = stateIdToTransState[stateId];

                var stateTransitions = animatorState.transitions;
                foreach (var transition in stateTransitions)
                {
                    var dstStateId = StateToId(transition.destinationState);
                    var transId = TransToId(transition);
                    transRelations.Add(new KeyValuePair<int, int>(transId, dstStateId));
                }
            }

            //任意可进入的
            var anyStateTransitions = stateMachine.anyStateTransitions;
            foreach (var transition in anyStateTransitions)
            {
                var dstStateId = StateToId(transition.destinationState);
                var transId = TransToId(transition);

                foreach (var machineState in machineStates)
                {
                    var animatorState = machineState.state;
                    var stateId = stateToIdDic[animatorState];
                    stateIdToTransState[stateId].Add(new KeyValuePair<int, int>(transId, dstStateId));
                }
            }

            var json = new JSONObject();

            json.Add("defaultState", StateToId(defaultState));

            var jsonStates = new JSONArray();
            foreach (var stateId in stateIdToTransState.Keys)
            {
                jsonStates.Add(stateId);
            }

            json.Add("states", jsonStates);

            var relationsJson = new JSONObject();
            foreach (var kv in stateIdToTransState)
            {
                var relationJson = new JSONArray();

                var stateId = kv.Key;
                var relation = kv.Value;
                foreach (var relationItem in relation)
                {
                    var relationItemJson = new JSONObject();

                    var trans = relationItem.Key;
                    var dstState = relationItem.Value;
                    relationItemJson.Add("trans", trans);
                    relationItemJson.Add("state", dstState);

                    relationJson.Add(relationItemJson);
                }

                relationsJson.Add(stateId.ToString(), relationJson);
            }

            json.Add("relations", relationsJson);

            return json.ToString(1);
        }
    }
}
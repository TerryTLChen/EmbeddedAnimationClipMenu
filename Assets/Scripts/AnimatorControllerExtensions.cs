using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace UnityEditor {

    public static class AnimatorControllerExtensions {

        static Predicate<AnimatorState> ByMotion(AnimationClip animationClip) {
            return delegate(AnimatorState state) {
                return (state.motion && state.motion == animationClip);
            };
        }

        static Predicate<AnimatorState> ByName(string name) {
            return delegate(AnimatorState state) {
                return (state.name == name);
            };
        }

        static Predicate<StateMachineBehaviour> ByType(Type type) {
            return delegate(StateMachineBehaviour behaviour) {
                return (behaviour.GetType() == type);
            };
        }

        static Predicate<AnimatorState> BySMB(Type type) {
            return delegate(AnimatorState state) {
                return (state.behaviours != null && Array.Exists<StateMachineBehaviour>(state.behaviours, ByType(type)));
            };
        }

        /// <summary>
        /// Does animator controller has parameter of the specified name and type
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool HasParameterOfType(this UnityEditor.Animations.AnimatorController animator, string name, AnimatorControllerParameterType type) {
            return Array.Exists<AnimatorControllerParameter>(animator.parameters
                , parameter => (parameter.nameHash == Animator.StringToHash(name)) && (parameter.type == type));
        }

        /// <summary>
        /// Does animator controller has parameter
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static bool HasParameter(this UnityEditor.Animations.AnimatorController animator, AnimatorControllerParameter parameter) {
            return HasParameterOfType(animator, parameter.name, parameter.type);
        }

        /// <summary>
        /// Return parameter with specified name and type
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static AnimatorControllerParameter GetParameterOfType(this AnimatorController animator, string name, AnimatorControllerParameterType type) {
            return Array.Find<AnimatorControllerParameter>(animator.parameters
                , parameter => (parameter.nameHash == Animator.StringToHash(name)) && (parameter.type == type));
        }

        /// <summary>
        /// Return all states with specified animation clip in the animator
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="animationClip"></param>
        /// <returns></returns>
        public static AnimatorState[] GetAllStatesWithMotion(this AnimatorController animator, AnimationClip animationClip) {
            return animator.GetAllStates(ByMotion(animationClip));
        }

        /// <summary>
        /// Return all states with specified name in the animator
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AnimatorState[] GetAllStatesWithName(this AnimatorController animator, string name) {
            return animator.GetAllStates(ByName(name));
        }

        /// <summary>
        /// Return all states with specified type of state machine behaviour attached
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static AnimatorState[] GetAllStatesWithBehaviour(this AnimatorController animator, Type type) {
            return animator.GetAllStates(BySMB(type));
        }

        /// <summary>
        /// Return all states in the animator
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        public static AnimatorState[] GetAllStates(this AnimatorController animator, Predicate<AnimatorState> predicate = null) {

            List<AnimatorState> stateList = new List<AnimatorState>();
            foreach (var stateMachine in animator.GetAllStateMachines()) {
                foreach (var state in stateMachine.GetAllStates()) {
                    if (predicate == null)
                        stateList.Add(state);
                    else if (predicate != null && predicate.Invoke(state))
                        stateList.Add(state);
                }
            }
            return stateList.ToArray();
        }

        /// <summary>
        /// Return all state machines in the animator
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        public static AnimatorStateMachine[] GetAllStateMachines(this AnimatorController animator) {

            List<AnimatorStateMachine> stateMachineList = new List<AnimatorStateMachine>();
            foreach (var layer in animator.layers) {
                stateMachineList.Add(layer.stateMachine);
                foreach (var childStateMachine in layer.stateMachine.stateMachines) {
                    stateMachineList.Add(childStateMachine.stateMachine);
                }
            }
            return stateMachineList.ToArray();
        }
    }

    public static class AnimatorStateMachineExtensions {

        /// <summary>
        /// Return all states in the state machine
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <returns></returns>
        public static AnimatorState[] GetAllStates(this AnimatorStateMachine stateMachine) {

            List<AnimatorState> stateList = new List<AnimatorState>();
            foreach (var childState in stateMachine.states) { // get all states
                stateList.Add(childState.state);
            }
            return stateList.ToArray();
        }
    }

    public static class AnimatorControllerParameterExtensions {

        /// <summary>
        /// Create a new animator controller float parameter from specified name
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static AnimatorControllerParameter CreateFloat(this AnimatorControllerParameter parameter, string name, float defaultValue) {
            AnimatorControllerParameter newParameter = new AnimatorControllerParameter();
            newParameter.name = name;
            newParameter.type = AnimatorControllerParameterType.Float;
            newParameter.defaultFloat = defaultValue;
            return newParameter;
        }

        /// <summary>
        /// Create a new animator controller integer parameter from specified name
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static AnimatorControllerParameter CreateInteger(this AnimatorControllerParameter parameter, string name, int defaultValue) {
            AnimatorControllerParameter newParameter = new AnimatorControllerParameter();
            newParameter.name = name;
            newParameter.type = AnimatorControllerParameterType.Int;
            newParameter.defaultInt = defaultValue;
            return newParameter;
        }

        /// <summary>
        /// Create a new animator controller bool parameter from specified name
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static AnimatorControllerParameter CreateBool(this AnimatorControllerParameter parameter, string name, bool defaultValue) {
            AnimatorControllerParameter newParameter = new AnimatorControllerParameter();
            newParameter.name = name;
            newParameter.type = AnimatorControllerParameterType.Bool;
            newParameter.defaultBool = defaultValue;
            return newParameter;
        }

        /// <summary>
        /// Create a new animator controller trigger parameter from specified name
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AnimatorControllerParameter CreateTigger(this AnimatorControllerParameter parameter, string name) {
            AnimatorControllerParameter newParameter = new AnimatorControllerParameter();
            newParameter.name = name;
            newParameter.type = AnimatorControllerParameterType.Trigger;
            return newParameter;
        }
    }
}

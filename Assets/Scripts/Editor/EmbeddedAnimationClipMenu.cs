using System.IO;
using UnityEditor.Animations;
using UnityEngine;

namespace UnityEditor {

    public class EmbeddedAnimationClipMenu {

        static Object[] selectedAnimationClips {
            get { return Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Unfiltered); }
        }

        [MenuItem("Assets/Embedded Animation/New Animation", priority = 1)]
        internal protected static void CreateEmbeddedAnimationClip() {

            var newAnimationClip = new AnimationClip();
            newAnimationClip.name = "New Animation Clip";

            AssetDatabase.AddObjectToAsset(newAnimationClip, Selection.activeObject);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newAnimationClip));
            ShowRenameWindow(newAnimationClip);

            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Embedded Animation/New Animation", validate = true)]
        internal protected static bool ValidateAnimatorController() {

            return (Selection.activeObject) && (Selection.activeObject.GetType() == typeof(AnimatorController));
        }

        [MenuItem("Assets/Embedded Animation/Delete Animations", priority = 4)]
        internal protected static void DeleteEmbeddedAnimationClips() {

            if (EditorUtility.DisplayDialog("Delete Animations", "Are you sure you want to delete these animations?", "Delete", "Do Not Delete")) {

                string assetPath = AssetDatabase.GetAssetPath(selectedAnimationClips[0]);

                Object[] animationClips = selectedAnimationClips;
                for (int i = 0; i < animationClips.Length; i++) {
                    Object animationClip = animationClips[i];
                    Object.DestroyImmediate(animationClip, true);
                }
                AssetDatabase.ImportAsset(assetPath);
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("Assets/Embedded Animation/Rename Animation", priority = 2)]
        internal protected static void RenameEmbeddedAnimationClip() {

            ShowRenameWindow(Selection.activeObject);
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Embedded Animation/Attach Animations", priority = 3)]
        internal protected static void AttachAnimationClip() {

            ShowAttachAnimationWindow(selectedAnimationClips);
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Embedded Animation/Detach Animations", priority = 5)]
        internal protected static void DetachEmbeddedAnimationClip() {

            if (EditorUtility.DisplayDialog("Detach Animations", "Are you sure you want to detach these animations?", "Detach", "Do Not Detach")) {

                string mainAssetPath = AssetDatabase.GetAssetPath(selectedAnimationClips[0]);

                Object[] animationClips = selectedAnimationClips;
                Object[] duplicateAnimationClips = new Object[animationClips.Length];
                for (int i = 0; i < animationClips.Length; i++) {
                    Object animationClip = animationClips[i];

                    string destPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(animationClip));
                    destPath += "/" + animationClip.name + ".anim";

                    if (AssetDatabase.LoadAssetAtPath<AnimationClip>(destPath)) {
                        Debug.LogWarningFormat("Asset file {0} already exit, operation ignored", destPath);
                        continue;
                    }

                    AnimatorController animator = AssetDatabase.LoadMainAssetAtPath(mainAssetPath) as AnimatorController;
                    AnimatorState[] states = animator.GetAllStatesWithMotion(animationClip as AnimationClip);

                    var duplicatAnimationClip = Object.Instantiate<Object>(animationClip);
                    Object.DestroyImmediate(animationClip, true);
                    AssetDatabase.CreateAsset(duplicatAnimationClip, destPath);

                    duplicateAnimationClips[i] = duplicatAnimationClip;

                    foreach (var state in states) {
                        state.motion = duplicatAnimationClip as Motion;
                    }
                }

                Selection.objects = duplicateAnimationClips; // keep the selection after detached

                AssetDatabase.ImportAsset(mainAssetPath);
                AssetDatabase.Refresh();
            }
        }

        static void ShowRenameWindow(Object animation) {

            string title = "Enter New Animation Name";
            Vector2 position = new Vector2((Screen.width) / 2, (Screen.height) / 2);
            Vector2 size = new Vector2(250f, 80f);
            RenameEmbeddedAnimationClipWindow window = EditorWindow.GetWindowWithRect<RenameEmbeddedAnimationClipWindow>(new Rect(position, size), true, title, true);
            window.animation = animation;
            window.animationName = animation.name;
            window.Focus();
        }

        static void ShowAttachAnimationWindow(Object[] animationClips) {

            string title = "Select An Animator Controller";
            Vector2 position = new Vector2((Screen.width) / 2, (Screen.height) / 2);
            Vector2 size = new Vector2(300f, 80f);
            AttachAnimationClipWindow window = EditorWindow.GetWindowWithRect<AttachAnimationClipWindow>(new Rect(position, size), true, title, true);
            window.animationClips = animationClips;
            window.Focus();

        }

        [MenuItem("Assets/Embedded Animation/Rename Animation", validate = true)]
        internal protected static bool ValidateEmbeddedAnimationClip() {

            if (Selection.objects.Length != 1 || selectedAnimationClips.Length != 1) // only one thing selected one animation clip only
                return false;

            if (!AssetDatabase.IsSubAsset(selectedAnimationClips[0])) // and it is a sub-asset
                return false;

            return true;
        }

        [MenuItem("Assets/Embedded Animation/Delete Animations", validate = true)]
        [MenuItem("Assets/Embedded Animation/Detach Animations", validate = true)]
        internal protected static bool ValidateEmbeddedAnimationClips() {

            if (selectedAnimationClips.Length == 0) { // no animation clips selected at all
                return false;
            }

            if (selectedAnimationClips.Length != Selection.objects.Length) { // something else has been selected too
                return false;
            }

            for (int i = 0; i < selectedAnimationClips.Length; i++) {
                if (!AssetDatabase.IsSubAsset(selectedAnimationClips[i])) // not all embedded animation clips
                    return false;
            }

            return true;
        }

        [MenuItem("Assets/Embedded Animation/Attach Animations", validate = true)]
        internal protected static bool ValidateAnimationClips() {

            if (selectedAnimationClips.Length == 0) { // no animation clips selected at all
                return false;
            }

            if (selectedAnimationClips.Length != Selection.objects.Length) { // something else has been selected too
                return false;
            }

            for (int i = 0; i < selectedAnimationClips.Length; i++) {
                if (!AssetDatabase.IsMainAsset(selectedAnimationClips[i])) // not all animation clips
                    return false;
            }

            return true;
        }
    }

    public class RenameEmbeddedAnimationClipWindow : EditorWindow {

        public Object animation;
        public string animationName;

        void OnGUI() {

            EditorGUILayout.LabelField("Name:");
            GUI.SetNextControlName("AnimationName");
            this.animationName = EditorGUILayout.TextField(this.animationName);
            EditorGUI.FocusTextInControl("AnimationName");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("OK")) {
                this.animation.name = this.animationName;
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animation));
                this.Close();
            }
            if (GUILayout.Button("Cancel"))
                this.Close();

            EditorGUILayout.EndHorizontal();
        }
    }

    public class AttachAnimationClipWindow : EditorWindow {

        public Object[] animationClips;
        public AnimatorController animator;

        void OnGUI() {

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animator Controller:");
            this.animator = EditorGUILayout.ObjectField(this.animator, typeof(AnimatorController), false) as AnimatorController; ;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("OK")) {

                if (!this.animator)
                    return;

                for (int i = 0; i < animationClips.Length; i++) {
                    Object animationClip = animationClips[i];

                    AnimatorState[] states = animator.GetAllStatesWithMotion(animationClip as AnimationClip);

                    var duplicateAnimationClip = Object.Instantiate<Object>(animationClip);
                    duplicateAnimationClip.name = animationClip.name;
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(animationClip));
                    AssetDatabase.AddObjectToAsset(duplicateAnimationClip, animator);

                    foreach (var state in states) {
                        state.motion = duplicateAnimationClip as Motion;
                    }
                }

                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(animator));
                this.Close();
            }

            if (GUILayout.Button("Cancel"))
                this.Close();

            EditorGUILayout.EndHorizontal();
        }
    }
}

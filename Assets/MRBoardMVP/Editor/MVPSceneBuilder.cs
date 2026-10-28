#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MRBoardMVP.Editor
{
    public static class MVPSceneBuilder
    {
        [MenuItem("MR Board MVP/Create Demo Scene")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0, 1.20f, -.50f);
            camera.transform.LookAt(new Vector3(0, .75f, 0));
            camera.nearClipPlane = .01f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.035f, .045f, .065f);
            var light = new GameObject("Key Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(45, -25, 0);
            RenderSettings.ambientLight = new Color(.45f, .45f, .45f);
            var platform = new GameObject("MR Board Platform").AddComponent<MRBoardPlatform>();
            platform.interactionCamera = camera;
            platform.initialGame = Resources.Load<TextAsset>("demo-game");
            EditorSceneManager.SaveScene(scene, "Assets/MRBoardMVP/Demo.unity");
            Selection.activeGameObject = platform.gameObject;
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Linq;
using MRBoardMVP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LivingTable.Editor
{
    public static class LivingTableBuilder
    {
        const string Folder = "Assets/LivingTable";
        [MenuItem("Living Table/Create MVP Scenes")]
        public static void CreateScenes()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/TableSurface.mat");
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(material, Folder + "/TableSurface.mat");
            }
            var scene = EditorSceneManager.OpenScene("Assets/LiveTable001.unity", OpenSceneMode.Single);
            var boardMaterial = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/BoardSurface.mat");
            if (boardMaterial == null)
            {
                boardMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(boardMaterial, Folder + "/BoardSurface.mat");
            }
            // Work on a copy; leave the user's experimental scene intact.
            EditorSceneManager.SaveScene(scene, Folder + "/LivingTableQuest.unity", true);
            scene = EditorSceneManager.OpenScene(Folder + "/LivingTableQuest.unity", OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            foreach (var script in root.GetComponentsInChildren<MonoBehaviour>(true))
                if (script is PinchBlock || script is PinchPopup || script is PinchPlaceTest ||
                    script is TableAnchorPlacer || script is MRBoardPlacer || script is MRBoardPlatform)
                    UnityEngine.Object.DestroyImmediate(script);
            var experience = new GameObject("Living Table").AddComponent<LivingTableExperience>();
            experience.surfaceMaterial = material;
            experience.boardMaterial = boardMaterial;
            EditorSceneManager.SaveScene(scene);
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.gameObject.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0, 1.32f, -.65f);
            camera.transform.LookAt(new Vector3(0, .75f, 0));
            camera.nearClipPlane = .01f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.018f, .03f, .045f);
            var light = new GameObject("Soft key").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.4f;
            light.transform.rotation = Quaternion.Euler(45, -25, 0);
            RenderSettings.ambientLight = new Color(.55f, .55f, .55f);
            experience = new GameObject("Living Table").AddComponent<LivingTableExperience>();
            experience.desktopPreview = true;
            experience.surfaceMaterial = material;
            experience.boardMaterial = boardMaterial;
            EditorSceneManager.SaveScene(scene, Folder + "/LivingTableDesktop.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("LIVING_TABLE_SCENES_CREATED");
        }

        // Real Play Mode smoke test; invoked by Unity batch mode, no additional packages.
        public static void SmokeTest()
        {
            EditorSceneManager.OpenScene(Folder + "/LivingTableDesktop.unity", OpenSceneMode.Single);
            SessionState.SetBool("LivingTableSmoke", true);
            EditorApplication.EnterPlaymode();
        }

        public static void BuildAndTest()
        {
            CreateScenes();
            SmokeTest();
        }

        static double deadline;
        static int stage;
        [InitializeOnLoadMethod]
        static void ResumeSmoke()
        {
            if (!SessionState.GetBool("LivingTableSmoke", false)) return;
            deadline = EditorApplication.timeSinceStartup + 90;
            stage = 0;
            EditorApplication.update += Tick;
        }
        static void Require(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }
        static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Smoke test timed out.");
                if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
                var board = UnityEngine.Object.FindFirstObjectByType<BoardRuntime>();
                var experience = UnityEngine.Object.FindFirstObjectByType<LivingTableExperience>();
                if (board == null || board.Definition == null || experience == null) return;
                if (stage == 0)
                {
                    Require(board.Definition.pieces.Length == 8, "Initial layout must have eight pieces.");
                    Require(!board.TryMove("azure-1", new Vector2Int(-1, 0)), "Out-of-bounds move accepted.");
                    Require(!board.TryMove("azure-1", new Vector2Int(2, 0)), "Occupied move accepted.");
                    board.InteractCell(Vector2Int.zero);
                    Require(board.SelectedId == "azure-1", "Cell selection failed.");
                    Require(board.TryMove("azure-1", new Vector2Int(1, 2)), "Valid move rejected.");
                    Require(!board.TryMove("azure-2", new Vector2Int(2, 2)), "Concurrent move accepted.");
                    stage = 1;
                }
                else if (stage == 1 && !board.IsAnimating)
                {
                    Require(board.MoveCount == 1, "Move completion was not recorded.");
                    Require(board.TryGetPieceCell("azure-1", out var cell) && cell == new Vector2Int(1, 2), "Moved piece has wrong cell.");
                    Require(board.TryMove("azure-1", new Vector2Int(1, 3)), "Second move rejected.");
                    experience.ResetGame();
                    Require(!board.IsAnimating && board.MoveCount == 0, "Reset did not cancel move.");
                    Require(board.TryGetPieceCell("azure-1", out cell) && cell == Vector2Int.zero, "Reset did not restore piece.");
                    experience.SwitchGame();
                    Require(board.Definition.id == "constellation" && board.Definition.pieces.Length == 6, "Layout switch failed.");
                    experience.SwitchGame();
                    Require(board.Definition.id == "tabletop-sandbox", "Layout cycle failed.");
                    bool rejected = false;
                    try { new GameRegistry().Register("{\"id\":\"invalid\",\"columns\":0}"); }
                    catch (ArgumentException) { rejected = true; }
                    Require(rejected, "Invalid definition accepted.");
                    stage = 2;
                }
                else if (stage == 2)
                {
                    var camera = Camera.main;
                    var target = new RenderTexture(1440, 1000, 24);
                    camera.targetTexture = target;
                    camera.Render();
                    var previous = RenderTexture.active;
                    RenderTexture.active = target;
                    var image = new Texture2D(1440, 1000, TextureFormat.RGB24, false);
                    image.ReadPixels(new Rect(0, 0, 1440, 1000), 0, 0);
                    image.Apply();
                    System.IO.File.WriteAllBytes("Logs/LivingTable-preview.png", image.EncodeToPNG());
                    RenderTexture.active = previous;
                    camera.targetTexture = null;
                    UnityEngine.Object.Destroy(image);
                    UnityEngine.Object.Destroy(target);
                    Finish(0, "LIVING_TABLE_SMOKE_PASSED");
                }
            }
            catch (Exception error) { Finish(1, error.ToString()); }
        }
        static void Finish(int code, string message)
        {
            SessionState.SetBool("LivingTableSmoke", false);
            EditorApplication.update -= Tick;
            Debug.Log(message);
            EditorApplication.Exit(code);
        }
    }
}
#endif


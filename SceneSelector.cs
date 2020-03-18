using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
#if UNITY_EDITOR 
using UnityEditor;
#endif

public class SceneSelector : MonoBehaviour
{
    /// TODO: add headers array in between scenes that will be rendered as large labels in between the buttons
    /// quick way : scenes.Add( "title::My Header Text Value" );
    /// 
    [SerializeField, HideInInspector] internal Texture header = null;

    string main = "";

    List<string> scenes;

    private void Awake()
    {
        if (scenes != null) scenes.Clear();
        else scenes = new List<string>();

        int overflow = 100, count = 0;
        
        while( overflow -- > 0 )
        {
            try
            {
                var path = SceneUtility.GetScenePathByBuildIndex( count ++ );
                if (string.IsNullOrEmpty(path)) break;
                //int start = path.LastIndexOf("/");
                //Debug.Log(start + " " + path.Length + " " + path);
                //string name = path.Substring(start, path.Length - start);
                if( string.IsNullOrEmpty(main) ) main = path;
                scenes.Add( path );
            }
            catch( System.Exception e ) { e.ToString(); break; }
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        //SceneManager.activeSceneChanged += (_, o) =>
        //{
        //    changed = true;
        //    TryAddES();
        //};

        // SceneManager.sceneUnloaded += _ => changed = true;

        TryAddES();

        FindObjectsOfType<Camera>().ToList().ForEach(c => c.enabled = false);

        BlackCamera();
    }

    void TryAddES()
    {
        if (FindObjectOfType<EventSystem>() == null)

            new GameObject().AddComponent<EventSystem>();
    }

    private void OnGUI()
    {
        screen = new Rect(0, 0, Screen.width, Screen.height);

        scale = Screen.dpi / 96;

        if ( sBtn == null ) InitStyles();

        if (!changed)
        {
            LoadingGUI();
            return;
        }

        if (swap && Input.touchCount > 0) HandleTouchScroll();

        if (MenuGUI()) return;

        if (header != null) HeaderGUI();

        ButtonsGUI();
    }

    Rect screen = Rect.zero;

    bool changed = true;

    float scale = 1;

    Vector2 scroll = Vector2.zero;

    bool swap = true;

    Vector2 point = Vector2.zero;

    bool scrolling = false;

    GUIStyle sScroll, sBtn, sBtnBack, sLoadText;

    float texW = 0, texH = 0;

    void InitStyles()
    {
        sBtn = new GUIStyle(GUI.skin.button);
        sBtn.fontSize = Mathf.FloorToInt(14 * scale);
        sBtn.wordWrap = true;
        sBtn.normal.background = Texture2D.grayTexture;
        sBtn.border = new RectOffset(1, 1, 1, 1);
        var tex2D = new Texture2D(3, 3);
        for (var x = 0; x < 3; ++x) for (var y = 0; y < 3; ++y) tex2D.SetPixel(x, y, Color.white);
        tex2D.SetPixel(sBtn.border.left, sBtn.border.top, Color.grey); tex2D.filterMode = FilterMode.Point; tex2D.Apply();
        sBtn.hover.background = sBtn.active.background = sBtn.focused.background = tex2D;

        sBtnBack = new GUIStyle(sBtn);
        sBtnBack.fontSize = Mathf.FloorToInt(12 * scale);

        sScroll = new GUIStyle(GUI.skin.scrollView);
        int pad = Mathf.FloorToInt(10 * scale);
        sScroll.padding = new RectOffset(pad, Mathf.FloorToInt(2 * scale), pad, pad);

        var scrollBarTex = new Texture2D(1, 1);
        scrollBarTex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f));
        scrollBarTex.Apply();

        GUI.skin.verticalScrollbarThumb.fixedWidth = 14f * scale;
        GUI.skin.verticalScrollbar.fixedWidth = 14f * scale;
        GUI.skin.verticalScrollbarThumb.normal.background = scrollBarTex;

        sLoadText = new GUIStyle(GUI.skin.label);
        sLoadText.fontStyle = FontStyle.Bold;
        sLoadText.fontSize = Mathf.FloorToInt(24 * scale);
        sLoadText.alignment = TextAnchor.MiddleCenter;
    }

    void BlackCamera()
    {
        var c = new GameObject().AddComponent<Camera>();
        c.clearFlags = CameraClearFlags.Color;
        c.backgroundColor = Color.black;
    }

    IEnumerator StopTouch()
    {
        yield return new WaitForSeconds(0.15f);
        scrolling = false;
    }

    void LoadingGUI()
    {
        string s = $"{Mathf.RoundToInt(load_progress*98f)}%";
        GUI.Label(screen, "Loading " + s , sLoadText);
    }

    void HandleTouchScroll()
    {
        var touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                point = touch.position;
                scrolling = true;
                break;
            case TouchPhase.Moved:
                scroll.y += touch.deltaPosition.y / 4f;
                scrolling = true;
                break;
            case TouchPhase.Canceled:
            case TouchPhase.Ended:
                bool click = (touch.position - point).magnitude < 5f * scale;
                if (click) scrolling = false;
                else StartCoroutine(StopTouch());
                break;
        }
    }

    bool MenuGUI()
    {
        var scene = SceneManager.GetActiveScene();

        if (!(!swap && scene.path != main)) return false;

        var rect = new Rect(0, 0, 50 * scale, 25 * scale);

        rect.x = screen.width - rect.width * 1.2f;
        rect.y = (screen.height - rect.height) * 0.5f;

        if (GUI.Button(rect, "M☰NU", sBtnBack))
        {
            //SceneManager.UnloadSceneAsync( scene ); // not supported ...
            //changed = false;

            scene.GetRootGameObjects().ToList().ForEach(go =>
            {
                if (Application.isEditor) DestroyImmediate(go);
                Destroy(go);
            });

            BlackCamera();

            changed = true;
            swap = true;
        }

        return true;
    }

    void HeaderGUI()
    {
        var vertical = screen.width < screen.height;
        var ratio = header.width / (float)header.height;

        texH = Mathf.Max(screen.height * 0.22f, header.height);
        texW = texH * ratio;
        Rect rect = Rect.zero;

        if (texW > screen.width * 0.9f)
        {
            texW = screen.width * 0.85f;
            texH = texW / ratio;
        }

        GUILayout.Space((vertical ? 10 : 2) * scale);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            GUI.enabled = false;
            GUI.color = Color.black;
            GUILayout.Button(GUIContent.none, GUILayout.Width(texW), GUILayout.Height(texH));
            GUI.enabled = true;
            GUI.color = Color.white;
            rect = GUILayoutUtility.GetLastRect();
            GUILayout.FlexibleSpace();
        }

        GUILayout.Space((vertical ? 10 : 2) * scale);

        GUI.DrawTexture(rect, header);
    }

    float load_progress = 0f;

    void SceneLoading(AsyncOperation op )
    {
        load_progress = op.progress;

        if( op.isDone )
        {
            changed = true;
            TryAddES();
            sBtn = null;
        }
    }

    void ButtonsGUI()
    {
        var oScroll = new GUILayoutOption[] {
            GUILayout.ExpandWidth( true ),
            GUILayout.ExpandHeight( true ),
            GUILayout.MaxWidth( screen.width ),
            GUILayout.MaxHeight( screen.height - ( header == null ? 0 : texH ) )
        };

        using (new GUILayout.VerticalScope(oScroll))
        {
            using (new GUILayout.HorizontalScope())
            {
                using (var scope = new GUILayout.ScrollViewScope(scroll, sScroll, oScroll))
                {
                    scroll = scope.scrollPosition;

                    foreach (var scene in scenes )
                    {
                        if ( scene == main ) continue;

                        GUILayout.Space(6 * scale);

                        var index = scene.LastIndexOf("/");
                        if (index < 0) index = scene.LastIndexOf("\\");
                        if (index < 0) index = 0;

                        var label = scene.Substring(index == 0 ? index : index + 1, scene.Length - 7 - index);

                        if (GUILayout.Button(label, sBtn) && !scrolling)
                        {
                            swap = false;
                            changed = false;

                            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single).completed += SceneLoading;
                        }
                    }

                    GUILayout.Space(6 * scale);
                }

                // GUILayout.Space( 10 * scale );
            }
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(SceneSelector))]
public class SceneSelectorEditor : Editor
{
    //[InitializeOnLoadMethod]
    //static void Hook() => AssemblyReloadEvents.beforeAssemblyReload += UpdateSceneList;
    //static void UpdateSceneList()
    //{
    //    AssemblyReloadEvents.beforeAssemblyReload -= UpdateSceneList;
    //    if( staticTarget != null ) staticTarget.scenes = GetEnabledScenes();
    //}

    static SceneSelector staticTarget = null;


    static bool foldout = true;
    static bool arrange = false;

    SerializedProperty header;

    private void OnEnable()
    {
        if (Application.isPlaying) return;

        staticTarget = ( SceneSelector ) target;

        header = serializedObject.FindProperty("header");

        PrependFirstScene();

        //UpdateTargetScenes();
        //UpdateTargetLoader();
    }

    #region GUI

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            PlayModeGUI();
            return;
        }

        InitStyles();

        GUILayout.Space(5);

        serializedObject.Update();
        EditorGUILayout.PropertyField(header);
        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(5);

        if (GUILayout.Button("Find and Add all scenes"))
        {
            IncludeAllScenesToBuild();

            PrependFirstScene();

            foldout = true;

            return;
        }

        GUILayout.Space(5);

        var current = GetCurrentScenePath();

        var current_scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(current);

        bool allowSceneObjects = !EditorUtility.IsPersistent(current_scene);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Main Scene", EditorStyles.boldLabel);

            EditorGUILayout.ObjectField(current_scene, typeof(SceneAsset), allowSceneObjects);
        }

        EditorGUILayout.HelpBox("Current scene is the main scene that holds the navigation options", MessageType.None);

        GUILayout.Space(10);

        foldout = EditorGUILayout.Foldout(foldout, "Edit Scenes", true);

        if (!foldout) return;

        GUILayout.Space(5);
        GUILayout.Label(GUIContent.none, GUI.skin.horizontalSlider);
        GUILayout.Space(20);

        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Enable All", EditorStyles.miniButtonLeft))
            {
                SetBuildActiveAll(true);
            }
            if (GUILayout.Button("Disable All", EditorStyles.miniButtonRight))
            {
                SetBuildActiveAll(false);
                ToggleBuildActive(0, true);
            }
        }

        GUILayout.Space(5);

        arrange = EditorGUILayout.ToggleLeft("Rearrange", arrange);

        GUILayout.Space(5);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox)) SceneListGUI();
    }

    void PlayModeGUI()
    {
        EditorGUILayout.HelpBox("Component changes locked during playmode", MessageType.Warning);
    }

    void SceneListGUI()
    {
        var current = GetCurrentScenePath();
        var scenes = EditorBuildSettings.scenes;
        var lenght = scenes.Length;

        for (var i = 0; i < lenght; ++i)
        {
            var scene = scenes[i];
            var path = scene.path;

            if (path == current) continue;

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

            using (new GUILayout.HorizontalScope())
            {
                bool buildCheck = EditorGUILayout.ToggleLeft(GUIContent.none, scene.enabled, GUILayout.Width(15));

                if (buildCheck != scene.enabled) ToggleBuildActive(i, buildCheck);

                GUILayout.Space(10);

                // GUILayout.TextArea( sceneAsset.name );
                EditorGUILayout.ObjectField(sceneAsset, typeof(SceneAsset), false);
                //if(GUILayout.Button( "Select", EditorStyles.miniButtonLeft, GUILayout.Width( 50 ) ))
                //    EditorGUIUtility.PingObject( sceneAsset );

                if (arrange)
                {
                    using (new EditorGUI.DisabledGroupScope(i < 2 )) // 1st one at 0 is main 
                        if (GUILayout.Button("▲", sBtnUp, GUILayout.Width(25)))
                            SwapBuildSceneIndexes(i, i - 1);

                    using (new EditorGUI.DisabledGroupScope(i == lenght - 1))
                        if (GUILayout.Button("▼", sBtnDown, GUILayout.Width(25)))
                            SwapBuildSceneIndexes(i, i + 1);
                }

                if (GUILayout.Button("X", sBtnDel, GUILayout.Width(25)))
                {
                    RemoveScene(path);
                    //UpdateTargetScenes();
                    return;
                }
            }
        }
    }

    #endregion

    #region Build Scenes - Methods

    void PrependFirstScene()
    {
        var build = GetBuildScenes();

        var current = GetCurrentScenePath();

        var index = build.ToList().IndexOf(current);

        // Auto add current scene to build settings if missing 

        if (index == -1) Prepend(current);

        // Swap if main scene is not first

        else if (index > 0) SwapBuildSceneIndexes(0, index);
    }

    void SwapBuildSceneIndexes(int a, int b)
    {
        var tmp = EditorBuildSettings.scenes.ToList();
        var t = tmp[a]; tmp[a] = tmp[b]; tmp[b] = t;
        EditorBuildSettings.scenes = tmp.ToArray();
    }

    void ToggleBuildActive(int build_index, bool active)
    {
        var list = EditorBuildSettings.scenes.ToList();
        list[build_index].enabled = active;
        EditorBuildSettings.scenes = list.ToArray();
    }

    void SetBuildActiveAll(bool active)
    {
        var list = EditorBuildSettings.scenes.ToList();
        list.ForEach(scene => scene.enabled = active);
        EditorBuildSettings.scenes = list.ToArray();
    }

    void Prepend(string path)
    {
        EditorBuildSettings.scenes = (new EditorBuildSettingsScene[] {
            new EditorBuildSettingsScene( path, true )
        })
        .Concat(EditorBuildSettings.scenes.ToList())
        .ToArray();
    }

    static List<string> GetEnabledScenes() => EditorBuildSettings.scenes
        
        .Where(s => s.enabled).Select(s => s.path).ToList();

    //void UpdateTargetScenes() => ((SceneSelector)target).scenes = GetEnabledScenes();

    //void UpdateTargetLoader() => ((SceneSelector)target).loader = GetCurrentScenePath();

    void RemoveScene(string path)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        scenes.Remove(scenes.Where(x => x.path == path).First());
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    string GetCurrentScenePath() => SceneManager.GetActiveScene().path;

    string[] GetAllScenes()
    {
        // source : https://github.com/Demkeys/SceneLoaderWindow/blob/master/SceneLoaderWindow.cs#L59
        return AssetDatabase.FindAssets("t:scene")
            .Select(x => AssetDatabase.GUIDToAssetPath(x))
            .ToArray();
    }

    string[] GetBuildScenes()
    {
        return EditorBuildSettings.scenes.Select(s => s.path).ToArray();
    }

    void IncludeAllScenesToBuild()
    {
        var build = GetBuildScenes();

        EditorBuildSettings.scenes = GetAllScenes()
            .Select(x => new EditorBuildSettingsScene(x, build.Contains(x)))
            .ToArray();
    }

    #endregion

    #region GUI Styles

    GUIStyle sBtnUp, sBtnDown, sBtnDel;

    void InitStyles()
    {
        if (sBtnUp != null) return;
        sBtnUp = new GUIStyle(EditorStyles.miniButtonLeft);
        sBtnUp.fontSize -= 2;
        sBtnDown = new GUIStyle(EditorStyles.miniButtonMid);
        sBtnDown.fontSize -= 2;
        sBtnDel = new GUIStyle(EditorStyles.miniButtonRight);
        sBtnDel.fontStyle = FontStyle.Bold;
    }

    #endregion
}

#endif 

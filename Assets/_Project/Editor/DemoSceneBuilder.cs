using System;
using System.IO;
using PortfolioShowcase.Core.Combat;
using PortfolioShowcase.Data;
using PortfolioShowcase.Gameplay.Enemies;
using PortfolioShowcase.Gameplay.Flow;
using PortfolioShowcase.Gameplay.Player;
using PortfolioShowcase.Gameplay.Spawning;
using PortfolioShowcase.Gameplay.Weapons;
using PortfolioShowcase.Installers;
using PortfolioShowcase.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PortfolioShowcase.EditorTools
{
    /// <summary>
    /// Generates the playable demo (scene, prefabs, materials, ScriptableObject
    /// data) from code via Tools > Showcase > Build Demo Scene.
    ///
    /// Why code instead of hand-authored assets: every wiring decision the
    /// README describes is visible and reviewable here in one diff-friendly
    /// file, and the whole demo can be regenerated deterministically. Re-running
    /// it updates existing assets in place, so GUIDs (and references) stay stable.
    /// </summary>
    public static class DemoSceneBuilder
    {
        private const string ContentFolder = "Assets/_Project/Content";
        private const string DataFolder = ContentFolder + "/Data";
        private const string PrefabFolder = ContentFolder + "/Prefabs";
        private const string MaterialFolder = ContentFolder + "/Materials";
        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string ScenePath = SceneFolder + "/Arena.unity";

        private const float ArenaHalfSize = 15f;

        [MenuItem("Tools/Showcase/Build Demo Scene", priority = 0)]
        public static void Build()
        {
            // TMP needs its essential resources (default font + settings) in the
            // project before any TMP_Text can render. Import once, then resume.
            if (!EnsureTmpEssentials()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureTag("Enemy");
            EnsureFolder(DataFolder);
            EnsureFolder(PrefabFolder);
            EnsureFolder(MaterialFolder);
            EnsureFolder(SceneFolder);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // --- Materials ---------------------------------------------------
            var groundMat = Mat("Ground", new Color(0.18f, 0.2f, 0.24f));
            var wallMat = Mat("Wall", new Color(0.32f, 0.35f, 0.42f));
            var playerMat = Mat("Player", new Color(0.2f, 0.6f, 1f));
            var gruntMat = Mat("Grunt", new Color(0.95f, 0.55f, 0.2f));
            var bruteMat = Mat("Brute", new Color(0.85f, 0.15f, 0.2f));
            var accentMat = Mat("Accent", new Color(0.95f, 0.95f, 0.95f));
            var projectileMat = Mat("Projectile", new Color(1f, 0.9f, 0.3f), emissive: true);

            // --- Weapon ------------------------------------------------------
            var projectilePrefab = BuildProjectilePrefab(projectileMat);

            var blaster = LoadOrCreate<WeaponData>($"{DataFolder}/Blaster.asset");
            blaster.projectilePrefab = projectilePrefab;
            blaster.damage = 10f;
            blaster.fireRate = 6f;
            blaster.projectileSpeed = 22f;
            blaster.projectileLifetime = 1.5f;
            EditorUtility.SetDirty(blaster);

            // --- Enemies: two variants, same code, different data ------------
            var gruntData = LoadOrCreate<EnemyData>($"{DataFolder}/Grunt.asset");
            SetEnemyStats(gruntData, health: 20f, speed: 4f, damage: 8f, range: 1.4f, cooldown: 0.8f, score: 10);

            var bruteData = LoadOrCreate<EnemyData>($"{DataFolder}/Brute.asset");
            SetEnemyStats(bruteData, health: 80f, speed: 2.2f, damage: 20f, range: 1.9f, cooldown: 1.2f, score: 40);

            var gruntPrefab = BuildEnemyPrefab("Grunt", gruntData, 1f, gruntMat, accentMat);
            var brutePrefab = BuildEnemyPrefab("Brute", bruteData, 1.6f, bruteMat, accentMat);

            // --- Waves -------------------------------------------------------
            var waves = new[]
            {
                Wave("Wave1", Entry(gruntData, gruntPrefab, 6, 0.6f)),
                Wave("Wave2", Entry(gruntData, gruntPrefab, 10, 0.4f), Entry(bruteData, brutePrefab, 2, 1.5f)),
                Wave("Wave3", Entry(gruntData, gruntPrefab, 14, 0.3f), Entry(bruteData, brutePrefab, 5, 1f)),
            };

            // --- Scene -------------------------------------------------------
            SetupCamera();
            BuildArena(groundMat, wallMat);
            BuildPlayer(playerMat, accentMat, blaster);
            var spawnPoints = BuildSpawnPoints();

            var spawner = new GameObject("WaveSpawner").AddComponent<WaveSpawner>();
            Configure(spawner, so =>
            {
                SetArray(so.FindProperty("waves"), waves);
                SetArray(so.FindProperty("spawnPoints"), spawnPoints);
            });

            var bootstrap = new GameObject("Bootstrap");
            bootstrap.AddComponent<GameInstaller>();
            bootstrap.AddComponent<GameFlowController>();

            BuildHud();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();

            Debug.Log($"[Showcase] Demo built at {ScenePath}. Press Play.");
        }

        // ------------------------------------------------------------------
        // Prefabs
        // ------------------------------------------------------------------

        private static GameObject BuildProjectilePrefab(Material mat)
        {
            var go = Primitive(PrimitiveType.Sphere, "Projectile", null, Vector3.zero, Vector3.one * 0.3f, mat, keepCollider: true);
            go.GetComponent<SphereCollider>().isTrigger = true;
            go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            go.AddComponent<Projectile>();
            return SavePrefab(go, "Projectile");
        }

        private static GameObject BuildEnemyPrefab(string name, EnemyData data, float size, Material body, Material accent)
        {
            // Pivot at the feet so every variant, whatever its size, stands on the ground.
            var root = new GameObject(name) { tag = "Enemy" };

            var col = root.AddComponent<CapsuleCollider>();
            col.radius = 0.5f * size;
            col.height = 2f * size;
            col.center = new Vector3(0f, size, 0f);

            Primitive(PrimitiveType.Capsule, "Body", root.transform, new Vector3(0f, size, 0f), Vector3.one * size, body, false);
            Primitive(PrimitiveType.Cube, "Eyes", root.transform, new Vector3(0f, size * 1.4f, 0.4f * size),
                new Vector3(0.6f, 0.15f, 0.25f) * size, accent, false);

            var health = root.AddComponent<Health>();
            Configure(health, so =>
            {
                so.FindProperty("maxHealth").floatValue = data.maxHealth;
                so.FindProperty("isPlayer").boolValue = false;
                so.FindProperty("scoreValueOnDeath").intValue = data.scoreValue;
            });

            var controller = root.AddComponent<EnemyController>();
            Configure(controller, so => so.FindProperty("data").objectReferenceValue = data);

            return SavePrefab(root, name);
        }

        private static void SetEnemyStats(EnemyData d, float health, float speed, float damage, float range, float cooldown, int score)
        {
            d.maxHealth = health;
            d.moveSpeed = speed;
            d.contactDamage = damage;
            d.detectionRadius = ArenaHalfSize * 4f; // arena-wide: every spawned enemy hunts the player
            d.attackRange = range;
            d.attackCooldown = cooldown;
            d.scoreValue = score;
            EditorUtility.SetDirty(d);
        }

        private static WaveData Wave(string name, params WaveEntry[] entries)
        {
            var wave = LoadOrCreate<WaveData>($"{DataFolder}/{name}.asset");
            wave.entries = entries;
            EditorUtility.SetDirty(wave);
            return wave;
        }

        private static WaveEntry Entry(EnemyData data, GameObject prefab, int count, float interval) =>
            new WaveEntry { enemyData = data, enemyPrefab = prefab, count = count, spawnInterval = interval };

        // ------------------------------------------------------------------
        // Scene content
        // ------------------------------------------------------------------

        private static void SetupCamera()
        {
            var cam = Camera.main;
            cam.transform.position = new Vector3(0f, 27f, -17f);
            cam.transform.LookAt(new Vector3(0f, 0f, -1.5f));
            cam.fieldOfView = 55f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.08f, 0.1f);
        }

        private static void BuildArena(Material groundMat, Material wallMat)
        {
            var arena = new GameObject("Arena").transform;

            var size = ArenaHalfSize * 2f;
            Primitive(PrimitiveType.Plane, "Ground", arena, Vector3.zero, new Vector3(size / 10f, 1f, size / 10f), groundMat, true);

            const float height = 1.5f;
            var offset = ArenaHalfSize + 0.5f;
            Primitive(PrimitiveType.Cube, "Wall N", arena, new Vector3(0f, height / 2f, offset), new Vector3(size + 2f, height, 1f), wallMat, true);
            Primitive(PrimitiveType.Cube, "Wall S", arena, new Vector3(0f, height / 2f, -offset), new Vector3(size + 2f, height, 1f), wallMat, true);
            Primitive(PrimitiveType.Cube, "Wall E", arena, new Vector3(offset, height / 2f, 0f), new Vector3(1f, height, size), wallMat, true);
            Primitive(PrimitiveType.Cube, "Wall W", arena, new Vector3(-offset, height / 2f, 0f), new Vector3(1f, height, size), wallMat, true);
        }

        private static void BuildPlayer(Material bodyMat, Material accentMat, WeaponData weaponData)
        {
            var player = Primitive(PrimitiveType.Capsule, "Player", null, new Vector3(0f, 1.1f, 0f), Vector3.one, bodyMat, false);
            player.tag = "Player";

            var cc = player.AddComponent<CharacterController>();
            cc.center = Vector3.zero;
            cc.height = 2f;
            cc.radius = 0.5f;

            Primitive(PrimitiveType.Cube, "Visor", player.transform, new Vector3(0f, 0.45f, 0.4f), new Vector3(0.6f, 0.15f, 0.3f), accentMat, false);

            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(player.transform, false);
            muzzle.localPosition = new Vector3(0f, 0f, 0.8f);

            var health = player.AddComponent<Health>();
            Configure(health, so =>
            {
                so.FindProperty("maxHealth").floatValue = 100f;
                so.FindProperty("isPlayer").boolValue = true;
            });

            var weapon = player.AddComponent<WeaponController>();
            Configure(weapon, so =>
            {
                so.FindProperty("weaponData").objectReferenceValue = weaponData;
                so.FindProperty("muzzle").objectReferenceValue = muzzle;
            });

            var controller = player.AddComponent<PlayerController>();
            Configure(controller, so => so.FindProperty("weapon").objectReferenceValue = weapon);
        }

        private static Transform[] BuildSpawnPoints()
        {
            var parent = new GameObject("SpawnPoints").transform;
            var d = ArenaHalfSize - 2f;
            var positions = new[]
            {
                new Vector3(-d, 0f, d), new Vector3(0f, 0f, d), new Vector3(d, 0f, d),
                new Vector3(-d, 0f, 0f), new Vector3(d, 0f, 0f),
                new Vector3(-d, 0f, -d), new Vector3(0f, 0f, -d), new Vector3(d, 0f, -d),
            };

            var points = new Transform[positions.Length];
            for (var i = 0; i < positions.Length; i++)
            {
                points[i] = new GameObject($"Spawn {i + 1}").transform;
                points[i].SetParent(parent, false);
                points[i].localPosition = positions[i];
            }
            return points;
        }

        private static void BuildHud()
        {
            var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var t = canvasGo.transform;
            var score = Label(t, "Score", new Vector2(0f, 1f), new Vector2(40f, -30f), TextAlignmentOptions.TopLeft, 44, "Score: 0");
            var wave = Label(t, "Wave", new Vector2(0.5f, 1f), new Vector2(0f, -30f), TextAlignmentOptions.Top, 44, "Wave 1");
            var health = Label(t, "Health", new Vector2(1f, 1f), new Vector2(-40f, -30f), TextAlignmentOptions.TopRight, 44, "HP: 100");
            var banner = Label(t, "Banner", new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center, 96, string.Empty);
            Label(t, "Controls", new Vector2(0.5f, 0f), new Vector2(0f, 24f), TextAlignmentOptions.Bottom, 28,
                "WASD: move    Mouse: aim    Hold left click: shoot");

            var hud = canvasGo.AddComponent<HUDController>();
            Configure(hud, so =>
            {
                so.FindProperty("scoreLabel").objectReferenceValue = score;
                so.FindProperty("waveLabel").objectReferenceValue = wave;
                so.FindProperty("healthLabel").objectReferenceValue = health;
                so.FindProperty("bannerLabel").objectReferenceValue = banner;
            });
        }

        private static TMP_Text Label(Transform parent, string name, Vector2 anchor, Vector2 position,
            TextAlignmentOptions alignment, float fontSize, string text)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(1000f, 250f);

            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 localPosition,
            Vector3 localScale, Material material, bool keepCollider)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static GameObject SavePrefab(GameObject instance, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, $"{PrefabFolder}/{name}.prefab");
            Object.DestroyImmediate(instance);
            return prefab;
        }

        private static Material Mat(string name, Color color, bool emissive = false)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(DefaultLitShader());
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.color = color;
            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 1.5f);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // Works on Built-in and on any SRP the project might be switched to later.
        private static Shader DefaultLitShader()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline;
            return pipeline != null && pipeline.defaultMaterial != null
                ? pipeline.defaultMaterial.shader
                : Shader.Find("Standard");
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        // Private [SerializeField]s are set through SerializedObject so the
        // runtime classes don't need public setters just for tooling.
        private static void Configure(Object target, Action<SerializedObject> edit)
        {
            var so = new SerializedObject(target);
            edit(so);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(SerializedProperty property, Object[] values)
        {
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void EnsureTag(string tag)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tags = tagManager.FindProperty("tags");
            for (var i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static bool EnsureTmpEssentials()
        {
            if (AssetDatabase.FindAssets("t:TMP_Settings", new[] { "Assets" }).Length > 0) return true;

            var ugui = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.ugui/package.json");
            var package = ugui == null
                ? null
                : Path.Combine(ugui.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");

            if (package == null || !File.Exists(package))
            {
                Debug.LogError("[Showcase] Could not find TMP Essential Resources. Import them via " +
                               "Window > TextMeshPro > Import TMP Essential Resources, then run this again.");
                return false;
            }

            Debug.Log("[Showcase] Importing TMP Essential Resources first; the build resumes automatically.");
            AssetDatabase.importPackageCompleted += OnTmpImported;
            AssetDatabase.ImportPackage(package, false);
            return false;
        }

        private static void OnTmpImported(string packageName)
        {
            AssetDatabase.importPackageCompleted -= OnTmpImported;
            EditorApplication.delayCall += Build;
        }
    }
}

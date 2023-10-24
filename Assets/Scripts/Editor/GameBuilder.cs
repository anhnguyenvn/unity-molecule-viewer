using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;
using System;
using System.IO;

public class GameBuilder : MonoBehaviour
{
    private const string KEYSTORE_PASS  = "KEYSTORE_PASS";
    private const string KEY_ALIAS_PASS = "KEY_ALIAS_PASS";
    private const string KEY_ALIAS_NAME = "KEY_ALIAS_NAME";
    private const string KEYSTORE       = "keystore.keystore";
    private const string BUILD_OPTIONS_ENV_VAR = "BuildOptions";
    private const string ANDROID_BUNDLE_VERSION_CODE = "VERSION_BUILD_VAR";
    private const string ANDROID_APP_BUNDLE = "BUILD_APP_BUNDLE";
    private const string SCRIPTING_BACKEND_ENV_VAR = "SCRIPTING_BACKEND";
    private const string VERSION_NUMBER_VAR = "VERSION_NUMBER_VAR";
    private const string VERSION_iOS = "VERSION_BUILD_VAR";

    [MenuItem("Build/Build Windows")]
    public static void PerformWindowsBuild()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetEnabledScenes();
        buildPlayerOptions.locationPathName = "build/windows/myproject.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.totalTime} ms, {summary.totalSize} bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log($"Build failed!");
        }
    }

    [MenuItem("Build/Build WebGL")]
    public static void PerformWebGlBuild()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetEnabledScenes();
        buildPlayerOptions.locationPathName = "build/webgl";
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.totalTime} ms, {summary.totalSize} bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log($"Build failed!");
        }
    }

    [MenuItem("Build/Build Android")]
    public static void PerformAndroidBuild()
    {
        //EditorPrefs.SetBool("JdkUseEmbedded", true);
        //EditorPrefs.SetBool("NdkUseEmbedded", true);
        //EditorPrefs.SetBool("SdkUseEmbedded", true);
        //EditorPrefs.SetBool("GradleUseEmbedded", true);
        //EditorPrefs.SetBool("AndroidGradleStopDaemonsOnExit", true);

        //HandleAndroidAppBundle();
        //HandleAndroidBundleVersionCode();
        //HandleAndroidKeystore();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetEnabledScenes();
        buildPlayerOptions.locationPathName = "build/android/myproject.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.totalTime} ms, {summary.totalSize} bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log($"Build failed!");
        }
    }

    static string[] GetEnabledScenes()
    {
        return (
            from scene in EditorBuildSettings.scenes
            where scene.enabled
            where !string.IsNullOrEmpty(scene.path)
            select scene.path
        ).ToArray();
    }

    private static void HandleAndroidAppBundle()
    {
        if (TryGetEnv(ANDROID_APP_BUNDLE, out string value))
        {
#if UNITY_2018_3_OR_NEWER
            if (bool.TryParse(value, out bool buildAppBundle))
            {
                EditorUserBuildSettings.buildAppBundle = buildAppBundle;
                Console.WriteLine($":: {ANDROID_APP_BUNDLE} env var detected, set buildAppBundle to {value}.");
            }
            else
            {
                Console.WriteLine($":: {ANDROID_APP_BUNDLE} env var detected but the value \"{value}\" is not a boolean.");
            }
#else
            Console.WriteLine($":: {ANDROID_APP_BUNDLE} env var detected but does not work with lower Unity version than 2018.3");
#endif
        }
    }

    private static void HandleAndroidBundleVersionCode()
    {
        if (TryGetEnv(ANDROID_BUNDLE_VERSION_CODE, out string value))
        {
            if (int.TryParse(value, out int version))
            {
                PlayerSettings.Android.bundleVersionCode = version;
                Console.WriteLine($":: {ANDROID_BUNDLE_VERSION_CODE} env var detected, set the bundle version code to {value}.");
            }
            else
                Console.WriteLine($":: {ANDROID_BUNDLE_VERSION_CODE} env var detected but the version value \"{value}\" is not an integer.");
        }
    }

    private static string GetIosVersion()
    {
        if (TryGetEnv(VERSION_iOS, out string value))
        {
            if (int.TryParse(value, out int version))
            {
                Console.WriteLine($":: {VERSION_iOS} env var detected, set the version to {value}.");
                return version.ToString();
            }
            else
                Console.WriteLine($":: {VERSION_iOS} env var detected but the version value \"{value}\" is not an integer.");
        }

        throw new ArgumentNullException(nameof(value), $":: Error finding {VERSION_iOS} env var");
    }

    private static void HandleAndroidKeystore()
    {
#if UNITY_2019_1_OR_NEWER
        PlayerSettings.Android.useCustomKeystore = false;
#endif

        if (!File.Exists(KEYSTORE)) {
            Console.WriteLine($":: {KEYSTORE} not found, skipping setup, using Unity's default keystore");
            return;    
        }

        PlayerSettings.Android.keystoreName = KEYSTORE;

        string keystorePass;
        string keystoreAliasPass;

        if (TryGetEnv(KEY_ALIAS_NAME, out string keyaliasName)) {
            PlayerSettings.Android.keyaliasName = keyaliasName;
            Console.WriteLine($":: using ${KEY_ALIAS_NAME} env var on PlayerSettings");
        } else {
            Console.WriteLine($":: ${KEY_ALIAS_NAME} env var not set, using Project's PlayerSettings");
        }

        if (!TryGetEnv(KEYSTORE_PASS, out keystorePass)) {
            Console.WriteLine($":: ${KEYSTORE_PASS} env var not set, skipping setup, using Unity's default keystore");
            return;
        }

        if (!TryGetEnv(KEY_ALIAS_PASS, out keystoreAliasPass)) {
            Console.WriteLine($":: ${KEY_ALIAS_PASS} env var not set, skipping setup, using Unity's default keystore");
            return;
        }
#if UNITY_2019_1_OR_NEWER
        PlayerSettings.Android.useCustomKeystore = true;
#endif
        PlayerSettings.Android.keystorePass = keystorePass;
        PlayerSettings.Android.keyaliasPass = keystoreAliasPass;
    }

    static bool TryGetEnv(string key, out string value)
    {
        value = Environment.GetEnvironmentVariable(key);
        return !string.IsNullOrEmpty(value);
    }
}

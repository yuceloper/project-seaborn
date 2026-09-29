using System;
using Seaborn.Persistence;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Seaborn.Editor
{
    public static class PrototypeNewCaptainSetup
    {
        private const string Menu = "Seaborn/Testing/Reset Player Progress";
        private const string HarborScene = "Assets/_Project/Scenes/PrototypeHarbor.unity";

        [MenuItem(Menu)]
        private static void ResetProgress()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(HarborScene) == null)
            {
                Debug.LogError("Liman sahnesi bulunamadı; kayıt değiştirilmedi: " + HarborScene);
                return;
            }
            // Preserve unsaved scene work; cancellation must not touch the captain save.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try
            {
                string backup = PrototypeProgressPersistence.ResetSavedProgressForNewCaptain();
                EditorSceneManager.OpenScene(HarborScene, OpenSceneMode.Single);
                Debug.Log("Yeni oyuncu testi hazır. Play'e bas: Seaborn Sloop, seviye 1, " +
                    "0 Silver, yükseltmesiz 6 top, 120 standart / 12 zincir / 16 saçma, " +
                    "10 hafif zıpkın, 1 tonik. Yedek: " + (backup ?? "Önceki kayıt yok"));
            }
            catch (Exception exception)
            {
                Debug.LogError("Yeni oyuncu testi hazırlanamadı: " + exception.Message);
            }
        }

        [MenuItem(Menu, true)]
        private static bool CanResetProgress()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling;
        }
    }
}

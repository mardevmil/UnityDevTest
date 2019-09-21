namespace mardevmil.Tools
{
    class UniqueIdGeneratorWindow : UnityEditor.EditorWindow
    {
        [UnityEditor.MenuItem("Tools/Generate Saveable IDs")]
        static void GenerateSaveableIDs()
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var all = FindObjectsOfType<UnityEngine.GameObject>();
            var isaveableList = new System.Collections.Generic.List<ISaveable>();
            for (int i = 0; i < all.Length; i++)
            {
                var isaveable = all[i].GetComponent<ISaveable>();
                if (isaveable != null)
                {
                    isaveable.SaveableIdStr = UniqueIdProvider.GenerateUniqueStringID(all[i]);
                    isaveable.SaveableId = UniqueIdProvider.GenerateUniqueID(all[i]);
                    isaveableList.Add(isaveable);
                    UnityEditor.EditorUtility.SetDirty(all[i].GetComponent<SaveableExample>());
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
                }
            }

            if (UniqueIdProvider.CheckOnDuplicates(isaveableList))
            {
                UnityEngine.Debug.Log("Regnerate because duplicate ids!");
                GenerateSaveableIDs();
            }
        }

        [UnityEditor.MenuItem("Tools/Check On Duplicate IDs")]
        static void CheckOnDuplicates()
        {
            var all = FindObjectsOfType<UnityEngine.GameObject>();
            var isaveableList = new System.Collections.Generic.List<ISaveable>();
            for (int i = 0; i < all.Length; i++)
            {
                var isaveable = all[i].GetComponent<ISaveable>();
                if (isaveable != null)
                    isaveableList.Add(isaveable);
            }

            if (UniqueIdProvider.CheckOnDuplicates(isaveableList))
            {
                UnityEngine.Debug.Log("Regnerate because duplicate ids!");
                GenerateSaveableIDs();
            }
            else
            {
                UnityEngine.Debug.Log("All IDs is unique!");
            }
        }
    }
}
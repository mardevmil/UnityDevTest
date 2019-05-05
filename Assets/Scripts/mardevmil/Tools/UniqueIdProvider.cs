namespace mardevmil.Tools
{
    public static class UniqueIdProvider
    {
        public static string GenerateUniqueStringID(UnityEngine.GameObject go = null)
        {
            System.DateTime epochStart = new System.DateTime(2000, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            int currentEpochTime = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
            int z1 = UnityEngine.Random.Range(0, 1000000);
            int z2 = UnityEngine.Random.Range(0, 1000000);

            int goTransformTotal = 0;
            if(go != null)
            {
                float posTotal    = go.transform.position.x + go.transform.position.y + go.transform.position.z;
                float rotTotal    = go.transform.rotation.x + go.transform.rotation.y + go.transform.rotation.z;
                float scaleTotal  = go.transform.localScale.x + go.transform.localScale.y + go.transform.localScale.z;
                float total = posTotal + rotTotal + scaleTotal;
                goTransformTotal = (int)total;
            }

            var activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            activeSceneName = activeSceneName.Replace(" ", "");
            string uid = activeSceneName.ToLower() + currentEpochTime;
            uid += z1 + z2;

            if (goTransformTotal > 0)
                uid += goTransformTotal.ToString();

            return uid;
        }

        public static bool CheckOnDuplicates(System.Collections.Generic.List<ISaveable> saveables)
        {        
            for (int i = 0; i < saveables.Count; i++)
            {
                for (int j = 0; j < saveables.Count; j++)
                {
                    if(saveables[i] != saveables[j] && saveables[i].SaveableIdStr == saveables[j].SaveableIdStr)                    
                        return true;                
                }
            }
            return false;
        }

        public static int GenerateUniqueID(UnityEngine.GameObject go = null)
        {
            System.DateTime epochStart = new System.DateTime(2019, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            int currentEpochTime = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;       
            int z1 = UnityEngine.Random.Range(0, 10);
            //int z2 = UnityEngine.Random.Range(0, 10);

            int goTransformTotal = 0;
            if (go != null)
            {
                float posTotal = go.transform.position.x + go.transform.position.y + go.transform.position.z;
                float rotTotal = go.transform.rotation.x + go.transform.rotation.y + go.transform.rotation.z;
                float scaleTotal = go.transform.localScale.x + go.transform.localScale.y + go.transform.localScale.z;
                float total = posTotal + rotTotal + scaleTotal;
                goTransformTotal = (int)total;
            }

            string uid = currentEpochTime.ToString();
            uid += z1;
            //uid += z2;
            if (goTransformTotal > 0)
                uid += goTransformTotal.ToString();
        
            int finalId = 0;
            if(int.TryParse(uid, out finalId))
            {
                return finalId;
            }
            else
            {
                UnityEngine.Debug.LogError("******************* uid " + uid + " 2147483647");
                return 0;
            }
        }    
    }
}


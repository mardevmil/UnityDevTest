using mardevmil;

public class SaveableExample : UnityEngine.MonoBehaviour, ISaveable
{
    [UnityEngine.SerializeField]
    private string _saveableId;
    //[SerializeField]
    private int _saveableIdNum = -1;

    void Start()
    {
        Introduce();
    }

    #region ISaveable
    public string SaveableIdStr { get { return _saveableId; } set { _saveableId = value; } }
    public int SaveableId { get { return _saveableIdNum; } set { _saveableIdNum = value; } }

    public void Introduce()
    {
        UnityEngine.Debug.Log("+++ " + gameObject.name + " SaveableIdStr: " + SaveableIdStr + " SaveableId: " + SaveableId);
    }
    #endregion
}

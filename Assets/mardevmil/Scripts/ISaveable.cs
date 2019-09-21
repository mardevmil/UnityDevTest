namespace mardevmil
{
    public interface ISaveable
    {
        string SaveableIdStr { get; set; }
        int SaveableId { get; set; }
        void Introduce();
    }
}


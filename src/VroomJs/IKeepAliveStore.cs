namespace VroomJs
{
    public interface IKeepAliveStore
    {
        int MaxSlots { get; }
        int AllocatedSlots { get; }
        int UsedSlots { get; }

        int Add(object obj);
        object Get(int slot);
        void Remove(int slot);
        void Clear();
    }
}

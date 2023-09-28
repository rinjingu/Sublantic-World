public sealed class ItemInstance {
    public ItemData itemType;
    public int Count;    

    public ItemInstance(ItemData itemType, int count) {
        this.itemType = itemType;
        this.Count = count;
    }
}
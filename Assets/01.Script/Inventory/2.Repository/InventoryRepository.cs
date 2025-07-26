using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class InventoryRepository : FirebaseRepositoryBase
{
    private const string DEFAULT_USER_ID = "DefaultUser";
    private const string COLLECTION_NAME = "inventories";

    private string GetUserInventoryPath()
    {
        return $"{COLLECTION_NAME}/{DEFAULT_USER_ID}";
    }

    public async Task SaveInventory(List<InventoryItem> items)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Document(GetUserInventoryPath());

            var itemDtoList = new List<InventoryItemDto>();
            foreach (var item in items)
            {
                itemDtoList.Add(new InventoryItemDto(item));
            }

            var docData = new Dictionary<string, object>
            {
                { "items", itemDtoList }
            };

            await docRef.SetAsync(docData);
        }, "Save User Inventory");
    }

    public async Task<List<InventoryItem>> LoadInventory()
    {
        return await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Document(GetUserInventoryPath());
            var snapshot = await docRef.GetSnapshotAsync();

            var result = new List<InventoryItem>();

            if (snapshot.Exists && snapshot.ContainsField("items"))
            {
                var itemDtos = snapshot.ConvertTo<Dictionary<string, List<InventoryItemDto>>>()["items"];

                foreach (var itemDto in itemDtos)
                {
                    result.Add(itemDto.ToInventoryItem());
                }
            }

            return result;
        }, "Load User Inventory");
    }

    public async Task AddItem(InventoryItem item)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Document(GetUserInventoryPath());
            var snapshot = await docRef.GetSnapshotAsync();

            var itemList = new List<InventoryItemDto>();

            if (snapshot.Exists && snapshot.ContainsField("items"))
            {
                itemList = snapshot.ConvertTo<Dictionary<string, List<InventoryItemDto>>>()["items"];
            }

            itemList.Add(new InventoryItemDto(item));

            var docData = new Dictionary<string, object>
            {
                { "items", itemList }
            };

            await docRef.SetAsync(docData);
        }, $"Add Item [{item.ItemType}] x{item.Quantity}");
    }

    public async Task RemoveItem(string itemId)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Document(GetUserInventoryPath());
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists && snapshot.ContainsField("items"))
            {
                var itemList = snapshot.ConvertTo<Dictionary<string, List<InventoryItemDto>>>()["items"];
                itemList.RemoveAll(item => item.ItemId == itemId);

                var docData = new Dictionary<string, object>
                {
                    { "items", itemList }
                };

                await docRef.SetAsync(docData);
            }
        }, $"Remove Item [ID: {itemId}]");
    }

    public async Task UpdateItemQuantity(string itemId, int newQuantity)
    {
        await ExecuteAsync(async () =>
        {
            var docRef = Firestore.Document(GetUserInventoryPath());
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists && snapshot.ContainsField("items"))
            {
                var itemList = snapshot.ConvertTo<Dictionary<string, List<InventoryItemDto>>>()["items"];
                var targetItem = itemList.Find(item => item.ItemId == itemId);

                if (targetItem != null)
                {
                    targetItem.Quantity = newQuantity;

                    var docData = new Dictionary<string, object>
                    {
                        { "items", itemList }
                    };

                    await docRef.SetAsync(docData);
                }
            }
        }, $"Update Item Quantity [ID: {itemId}] to {newQuantity}");
    }
}
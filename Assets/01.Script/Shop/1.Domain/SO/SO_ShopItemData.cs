using UnityEngine;

[CreateAssetMenu(fileName = "SO_ShopItemData", menuName = "Scriptable Objects/SO_ShopItemData")]
public class SO_ShopItemData : ScriptableObject
{
    public EItemType Type;
    public int Stock = 99;
    public int BuyPrice = 10;
    public int SellPrice = 5;
    public bool CanBuy = true;
    public bool CanSell = true;
    public bool HasUnlimitedStock = true;
}

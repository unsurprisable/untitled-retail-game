
public class ProductOrderItem : InteractableObject
{
    public override void OnInteract(PlayerController player)
    {
        ProductShopUI.Instance.Show();
    }
}


public class ProductOrderItem : InteractableObject
{
    public override void OnInteract(PlayerController player)
    {
        OrderMenuUI.Instance.Show();
    }
}

public interface IMenu
{
    protected virtual void OnShow(){}
    protected virtual void OnHide(){}

    public void Show() ;
    public void Hide();
}

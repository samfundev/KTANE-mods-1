using UnityEngine;

public abstract class Widget : MonoBehaviour
{
    public delegate void WidgetActivateDelegate();
    public WidgetActivateDelegate OnWidgetActivate;

    public delegate string WidgetQueryDelegate(string queryKey, string queryInfo);
    public WidgetQueryDelegate OnQueryRequest;
}
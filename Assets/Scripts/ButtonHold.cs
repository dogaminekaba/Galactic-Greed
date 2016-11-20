using UnityEngine;
using UnityEngine.UI;
public class ButtonHold : Button
{
    private void LateUpdate()
    {
        if (IsPressed())
        {
            KeyPressed();
            return;
        }
        KeyReleased();
    }
    private void KeyPressed()
    {
        transform.localScale = Vector3.one * 0.9f;
    }
    private void KeyReleased()
    {
        transform.localScale = Vector3.one;
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
public class ColorChange : MonoBehaviour
{
    public Gradient marbleColorGr;
    public bool changeColor;
    private Coroutine setMatColor;
    private float eval;
    public Material potsMarbleMat;
    private Color originalColor;

    public InputActionReference action;

    private void Start()
    {
        action.action.Enable();
        action.action.performed += (ctx) => ToggleColorChange();
        originalColor = potsMarbleMat.color;
    }
    private void ToggleColorChange()
    {
        changeColor = !changeColor;
        if (changeColor)
        {
            setMatColor = StartCoroutine(SetMatColor());
        }
        else
        {
            StopCoroutine(setMatColor);
            potsMarbleMat.color = originalColor;
        }
    }

    private IEnumerator SetMatColor()
    {
        while (true)
        {
            eval = Mathf.PingPong(Time.time * 0.1f, 1);
            potsMarbleMat.color = marbleColorGr.Evaluate(eval);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
}

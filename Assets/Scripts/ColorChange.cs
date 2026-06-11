using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
public class ColorChange : MonoBehaviour
{

    public InputActionReference action;
    private Coroutine setMatColor;
    public Material material;
    public float transitionTime = 1f;
    public Color newColor;
    public Color startColor;
    private bool colorChanged = false;
    private void Start()
    {
        action.action.Enable();
        action.action.performed += (ctx) => changeColor();
    }
    

    private void Awake()
    {
        material.color = startColor;
    }
    public void changeColor()
    {
        Debug.Log("color changed");
        if (colorChanged)
        {
            setMatColor = StartCoroutine(SetMatColor(newColor, startColor));
            colorChanged = false;
        }
        else
        {
            setMatColor = StartCoroutine(SetMatColor(startColor, newColor));
            colorChanged = true;
        }
    }
    private IEnumerator SetMatColor(Color firstColor, Color secondColor)
    {
        float timer = 0f;
        while (timer < transitionTime)
        {
            timer += Time.deltaTime;
            float t = timer / transitionTime;

            material.color = Color.Lerp(firstColor, secondColor, t);

            yield return null;
        }
        material.color = secondColor;
    }
}

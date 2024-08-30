using System;
using UnityEngine;
using UnityEngine.UI;

public class RadialFiller : MonoBehaviour
{
    [SerializeField] private Image radialImage;
    [SerializeField] private float maxTime;
    [SerializeField] private float stepFill = 0f;

    [SerializeField] private Transform content;

    public bool startFill;

    public Action OnFillComplete;

    public void SetMaxTime(float _maxTime) 
    {
        maxTime = _maxTime;
    }

    private void Update()
    {
        if (!startFill) return;

        Fill(Time.deltaTime);
    }

    public void Reset()
    {
        radialImage.fillAmount = 0;
        startFill = false;
        stepFill = 0f;

        content.gameObject.SetActive(startFill);
    }

    public void StartFill()
    {
        startFill = true;

        content.gameObject.SetActive(startFill);
    }

    public void Fill(float time)
    {
        stepFill += time;

        float step = stepFill / maxTime;

        if (radialImage.fillAmount == 1.0f)
        {
            //Debug.Log("Radial Filled");
            OnFillComplete?.Invoke();
        }
        else
        {
            radialImage.fillAmount = step;
        }
    }

}

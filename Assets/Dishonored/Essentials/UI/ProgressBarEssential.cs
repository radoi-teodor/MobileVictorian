using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ProgressBarEssential : MonoBehaviour {

    public Image fillImage; // image we want to fill
    public Image cursorImage; // image we want to follow progress
    public float value, max; // the min and max values

    [Space]
    public bool fadeWhenFilled = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (fillImage) // if image is not null
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, value/max, Time.deltaTime * 3); // propotion rule
            // we interpolate to make it smooth
        }

        if (cursorImage)
        {
            cursorImage.rectTransform.anchorMin = new Vector2(cursorImage.rectTransform.anchorMin.x, value/max);
            cursorImage.rectTransform.anchorMax = new Vector2(cursorImage.rectTransform.anchorMax.x, value/max);
            cursorImage.rectTransform.anchoredPosition = Vector2.Lerp(cursorImage.rectTransform.anchoredPosition, new Vector2(0,0), Time.deltaTime);

            if (fadeWhenFilled)
            {
                if(value/max < .98f)
                {
                    cursorImage.color = Color.Lerp(cursorImage.color, new Color(cursorImage.color.r, cursorImage.color.g, cursorImage.color.b,1), Time.deltaTime*2);
                }
                else
                {
                    cursorImage.color = Color.Lerp(cursorImage.color, new Color(cursorImage.color.r, cursorImage.color.g, cursorImage.color.b, 0), Time.deltaTime * 2);
                }
            }
            else
            {
                if(cursorImage.color.a != 1)
                {
                    cursorImage.color = new Color(cursorImage.color.r, cursorImage.color.g, cursorImage.color.b,1);
                }
            }
        }

	}

    private void LateUpdate()
    {
        value = Mathf.Clamp(value, 0, max); // we clam the value, so it can stay below the max
    }
}

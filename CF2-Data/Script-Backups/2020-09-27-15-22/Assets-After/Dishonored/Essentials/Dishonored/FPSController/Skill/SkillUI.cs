using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{

    [HideInInspector]
    public Vector2 anchoredPos;

    [HideInInspector]
    public string powerName;

    RectTransform rt;
    Image img;

    // Use this for initialization
    void Awake()
    {
        rt = GetComponent<RectTransform>();
        img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        rt.anchoredPosition = anchoredPos;

        if (ControlFreak2.CF2Input.GetMouseButtonUp(2))
        {
            img.color = new Color(1f, 1f, 1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (ControlFreak2.CF2Input.GetMouseButton(2))
        {
            if (collision.tag == "CursorUI")
            {
                img.color = new Color(.5f,.5f,.5f);
                SendMessageUpwards("ChangeSkill", powerName);
                SendMessageUpwards("ChangeSkillUI", powerName);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "CursorUI")
        {
            img.color = new Color(1f, 1f, 1f);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class PanelNavigator : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;

    private readonly Stack<GameObject> history = new Stack<GameObject>();
    private GameObject current;

    private void Start()
    {
        current = startPanel;
        current.SetActive(true);
    }

    public void Show(GameObject panel)
    {
        if (current != null)
        {
            current.SetActive(false);
            history.Push(current);
        }

        current = panel;
        current.SetActive(true);
    }

    public void Back()
    {
        if (history.Count == 0) return;

        current.SetActive(false);
        current = history.Pop();
        current.SetActive(true);
    }
}
using UnityEngine;

public class GridPosition : MonoBehaviour
{
    [SerializeField] private int X;
    [SerializeField] private int Y;

    private void OnMouseDown()
    {
        Debug.Log("Grid Position Clicked: " + X + ", " + Y);
    }
}

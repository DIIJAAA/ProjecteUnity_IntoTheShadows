
using UnityEngine;

public class MazeCellObject : MonoBehaviour
{
    [SerializeField] private GameObject topWall;
    [SerializeField] private GameObject bottomWall;
    [SerializeField] private GameObject leftWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject floor;

    public void Init(bool top, bool bottom, bool right, bool left)
    {
        topWall.SetActive(top);
        bottomWall.SetActive(bottom);
        rightWall.SetActive(right);
        leftWall.SetActive(left);
        
        if (floor != null) AdjustTextureTiling(floor, 1f, 1f);
        if (top && topWall != null) AdjustTextureTiling(topWall, 1f, 1f);
        if (bottom && bottomWall != null) AdjustTextureTiling(bottomWall, 1f, 1f);
        if (right && rightWall != null) AdjustTextureTiling(rightWall, 1f, 1f);
        if (left && leftWall != null) AdjustTextureTiling(leftWall, 1f, 1f);
    }
    
    private void AdjustTextureTiling(GameObject obj, float tilingX, float tilingY)
    {
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material != null)
        {
            Material mat = renderer.material;
            mat.mainTextureScale = new Vector2(tilingX, tilingY);
        }
    }
}
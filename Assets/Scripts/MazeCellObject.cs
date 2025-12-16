using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MazeCellObject : MonoBehaviour
{
    [SerializeField] GameObject topWall;
    [SerializeField] GameObject bottomWall;
    [SerializeField] GameObject leftWall;
    [SerializeField] GameObject rightWall;
    [SerializeField] GameObject floor;

    public void Init(bool top, bool bottom, bool right, bool left)
    {
        topWall.SetActive(top);
        bottomWall.SetActive(bottom);
        rightWall.SetActive(right);
        leftWall.SetActive(left);
        
        // Ajusta el tiling correctament
        if (floor != null) AdjustTextureTiling(floor, 1f, 1f);
        if (top && topWall != null) AdjustTextureTiling(topWall, 1f, 1f);
        if (bottom && bottomWall != null) AdjustTextureTiling(bottomWall, 1f, 1f);
        if (right && rightWall != null) AdjustTextureTiling(rightWall, 1f, 1f);
        if (left && leftWall != null) AdjustTextureTiling(leftWall, 1f, 1f);
    }
    
    void AdjustTextureTiling(GameObject obj, float tilingX, float tilingY)
    {
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material != null)
        {
            // Crea una instància del material per evitar modificar l'original
            Material mat = renderer.material;
            mat.mainTextureScale = new Vector2(tilingX, tilingY);
        }
    }
}
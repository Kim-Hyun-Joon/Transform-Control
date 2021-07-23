using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    private static TileManager instance;

    public static TileManager Instance {
        get {
            if (instance == null) instance = FindObjectOfType<TileManager>();
            
            return instance;
        }
    }


    public GameObject defaultTile;
    public Vector2 defaultTileSize;
    public int defaultFloorSize;
    public Plane groundPlane;

    public GameObject ArrowTile;

    private void Awake() {


        groundPlane = new Plane(Vector3.up, Vector3.zero);

        CreateFloor(Vector3.zero, defaultTile, defaultTileSize, defaultFloorSize);  //바닥 생성

    }

    public void CreateFloor(Vector3 position, GameObject tile, Vector2 tileSize, int floorSize) {
        //
        if (tile == null) return;                   //타일 모양 안 정해지면 패스
        GameObject quad = new GameObject("Quad");
        quad.transform.SetParent(transform);        //Quad 안에 Floor 싹다 집어넣을 예정

        for (int z = 0; z < floorSize; z++) {
            for (int x = 0; x < floorSize; x++) {
                GameObject t = GameObject.Instantiate(tile);
                t.name = tile.name;
                Vector3 pos = position + new Vector3(x * tileSize.x, 0f, z * tileSize.y);
                t.transform.position = pos;
                t.transform.SetParent(quad.transform);
            }
        }
    }
    
    public GameObject FindTileFrom(Transform hitTile) {       //누른 타일

        int depth = 0;
        Transform currentT = hitTile;
        while (currentT) {
            if (currentT == transform) break; //TileManager까지 올라감
            depth++;                            //깊이 추출
            currentT = currentT.parent;
        }

        if (currentT == null) return null;

        currentT = hitTile;
        while (currentT && depth > 2) //TileManager/Quad/Floor 의 순이니까
        {
            depth--;
            currentT = currentT.parent;
        }

        if (currentT == null) return null;
        return currentT.gameObject;
    }
    /*
    public GameObject[] FindNeighborTiles(GameObject hitTile) {

        List<GameObject> tiles = new List<GameObject>();

    }
    */

}

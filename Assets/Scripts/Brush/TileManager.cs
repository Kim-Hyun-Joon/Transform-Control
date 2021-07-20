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

        CreateDefault(defaultTile, defaultTileSize, defaultFloorSize);

    }

    public void CreateDefault(GameObject tile, Vector2 tileSize, int floorSize) {
        
        CreateQuad(Vector3.zero, tile, tileSize, floorSize);
    }

    public void CreateQuad(Vector3 position, GameObject tile, Vector2 tileSize, int floorSize) {
        //
        if (defaultTile == null) return ;
        GameObject quad = new GameObject("Quad");
        quad.transform.SetParent(transform);

        for (int z = 0; z < floorSize; z++) {
            for (int x = 0; x < floorSize; x++) {
                GameObject t = GameObject.Instantiate(defaultTile);
                t.name = defaultTile.name;
                Vector3 pos = position + new Vector3(x * tileSize.x, 0f, z * tileSize.y);
                t.transform.position = pos;
                t.transform.SetParent(quad.transform);
            }
        }
    }

    
    public GameObject FindTileFrom(Transform which) {
        // in case the raycasting found a child gameObj on a tile, find the top most GameObj of the tile
        int depth = 0;
        Transform currentT = which;
        while (currentT) {
            if (currentT == transform) break; // found TileMap
            depth++;
            currentT = currentT.parent;
        }

        if (currentT == null) return null;

        currentT = which;
        while (currentT && depth > 2) // expected: TileMap/Quad/Floor
        {
            depth--;
            currentT = currentT.parent;
        }

        if (currentT == null) return null;
        return currentT.gameObject;
    }
    
}

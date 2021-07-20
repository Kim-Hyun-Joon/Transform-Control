using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brush : MonoBehaviour
{
    public void OnEnable() {
        Debug.Log("aa");
    }
    Plane getPlane;
    float brushSize;
    TileManager tile;
    public GameObject brushTilePrefab;
    int layerMask;


    private void Start() {
        tile = TileManager.Instance;
        getPlane = tile.groundPlane;
        layerMask = 1 << LayerMask.NameToLayer("Floor");  // Floor 레이어만 충돌 체크함
    }

    // Update is called once per frame
    void Update()
    {
        if (!TileManager.Instance) return;

        if (Input.GetMouseButtonDown(0)) {
            ReplaceTilesUnderMouse();       //좌클릭시 타일을 바꾸는데
        }

        UpdateBrushMarker();
    }
    //------------------------------------- Edit Tiles ----------------------------------------//



    GameObject GetTileUnderMouse() {
        //
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, float.PositiveInfinity)) {
            return tile.FindTileFrom(hit.transform);
        }

        return null;
    }


    bool RayCastFromAbove(out RaycastHit hit, Vector3 pos) {
        Ray hitRay = new Ray(pos + (Vector3.up * 100f), -Vector3.up);
        return Physics.Raycast(hitRay, out hit, float.PositiveInfinity, layerMask);
    }



    GameObject[] GetTilesUnderMouse() {
        //
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);        //카메라에서 마우스 좌표로 레이를 쏨   
        if (!getPlane.Raycast(ray, out float dist)) return null;         //바닥에 마우스가 있는지 정도는 확인하고

        List<GameObject> tiles = new List<GameObject>();
        if (brushSize <= 1) {
            GameObject tile = GetTileUnderMouse();
            if (tile) tiles.Add(tile);
            return tiles.ToArray();
        }


        Vector3 center = ray.GetPoint(dist);
        float dx = tile.defaultTileSize.x;
        float dz = tile.defaultTileSize.y;

        Vector3 leftBottom = center - new Vector3(brushSize * 0.5f * dx, 0f, brushSize * 0.5f * dz);        //leftBottom을 지정해서 타일 위치를 정할거같은 느낌

        Vector3 zDir = Vector3.forward * dz;
        Vector3 xDir = Vector3.right * dx;


        for (int z = 0; z < brushSize; z++) {
            for (int x = 0; x < brushSize; x++) {
                Vector3 posOnGround = leftBottom + z * zDir + x * xDir;

                if (RayCastFromAbove(out RaycastHit hit, posOnGround)) {
                    GameObject go = tile.FindTileFrom(hit.transform);        //타일 구성이 TileMap/Quad/Floor의 구성임
                    if (go != null && !tiles.Contains(go)) {
                        tiles.Add(go);
                    }
                }// RayCast
            }// for x
        }// for z


        return tiles.ToArray();         //배열로?
    }


    void ReplaceTilesUnderMouse() {
        GameObject[] oldTiles = GetTilesUnderMouse();           //올드 타일에 마우스로 지정된 타일을 가져옴
        if (oldTiles == null) return;
        if (brushTilePrefab == null) {          //브러쉬는 존재해야하고
            Debug.Log("Please set 'Brush Tile Prefab'");
            return;
        }

        for (int i = 0; i < oldTiles.Length; i++) {
            GameObject oldTile = oldTiles[i];
            if (oldTile.name == brushTilePrefab.name)// same thing, don't create anew tile, but adept rotation
            {       //브러쉬랑 타일맵의 이름을 비교해서 설치된건지 아닌건지를 판단
                //oldTile.transform.rotation = Quaternion.Euler(0f, curTileYAngle, 0f);
                continue;
            }
            //이 밑으로는 브러쉬랑 타일맵의 이름이 다른 경우임
            //즉 색칠 안된 곳임
            GameObject newTile = GameObject.Instantiate(brushTilePrefab);   //브러쉬 모양으로 타일을 만든다음
            newTile.name = brushTilePrefab.name;
            newTile.transform.SetParent(oldTile.transform.parent);          //새로 만든 타일을 자식으로 넣고
            newTile.transform.SetSiblingIndex(oldTile.transform.GetSiblingIndex());//새로 만든 타일의 인덱스를 이전 타일의 인덱스로 설정
            newTile.transform.position = oldTile.transform.position;        //새로 만든 타일 위치는 이전 타일 위치로
            //newTile.transform.rotation = Quaternion.Euler(0f, curTileYAngle, 0f);//회전은 update()에서 정해준 public 변수를 사용

            oldTile.transform.SetParent(null);//삭제도 순서가 있는거 같음
            GameObject.Destroy(oldTile);        //Destroy 해도 1프레임 뒤에 scene에서 사라지기 때문에 우선 부모 자식관계를 끊어주고 삭제 진행
        }

    }

    //------------------------------------- Marker --------------------------------------------//
    private void UpdateBrushMarker() {
        // disable all:
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        //일단 다 숨기고

        GameObject[] tiles = GetTilesUnderMouse();
        if (tiles == null) return;
        if (tiles.Length == 0) return;

        // create additional markers in case there are not enough

        while (transform.childCount < tiles.Length) {
            GameObject m = GameObject.Instantiate(transform.GetChild(0).gameObject);
            m.transform.SetParent(transform);
            m.transform.localScale = transform.GetChild(0).localScale;
            m.SetActive(false);
        }

        // set Marker to pos of tile and enable. Assume Pivot is in the center for all.
        for (int i = 0; i < tiles.Length; i++) {
            RaycastHit hit;
            if (RayCastFromAbove(out hit, tiles[i].transform.position)) {
                GameObject marker = transform.GetChild(i).gameObject;
                marker.SetActive(true);
                Vector3 pos = hit.point;
                pos.y += 0.05f;
                marker.transform.position = pos;
            }

        }

    }

}

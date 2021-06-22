using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;


public class TransformControl : MonoBehaviour {


    class TransformData {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        Matrix4x4 matrix;
        public TransformData(Vector3 p, Quaternion r, Vector3 s) {
            position = p;
            rotation = r;
            scale = s;

            matrix = Matrix4x4.TRS(p, r, s);
        }
        public TransformData(Transform tr) : this(tr.position, tr.rotation, tr.localScale) { }
    }
    public enum TransformMode {
        None,
        Translate,
        Rotate,
        Scale
    };
    public enum TransformDirection {
        None,
        Xright,
        Xleft,
        Y,  //윗방향
        Zforward,
        Zback,
        XZ
    };
    protected const string SHADER = "Hidden/Internal-Colored";
    protected const float THRESHOLD = 10f;
    protected const float HANDLER_SIZE = 0.2f;
    protected Material Getmaterial() {
        if (_material == null) {
            var shader = Shader.Find(SHADER);
            if (shader == null) Debug.LogErrorFormat("Shader not found : {0}", SHADER);
            _material = new Material(shader);
        }
        return _material;
    }

    public TransformMode mode = TransformMode.Translate;

    Color[] colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, };

    Dictionary<TransformDirection, Vector3> axes = new Dictionary<TransformDirection, Vector3>() {
            { TransformDirection.None, Vector3.zero },
            { TransformDirection.Xright, Vector3.right },
            { TransformDirection.Xleft, Vector3.left },
            { TransformDirection.Y, Vector3.up },
            { TransformDirection.Zforward, Vector3.forward },
            { TransformDirection.Zback, Vector3.back },
            { TransformDirection.XZ, Vector3.right + Vector3.forward },
        };
    Matrix4x4[] matrices = new Matrix4x4[] {
            Matrix4x4.TRS(Vector3.right, Quaternion.AngleAxis(90f, Vector3.back), Vector3.one),
            Matrix4x4.TRS(Vector3.forward, Quaternion.AngleAxis(90f, Vector3.right), Vector3.one),
            Matrix4x4.TRS(Vector3.left, Quaternion.AngleAxis(90f, Vector3.forward), Vector3.one),
            Matrix4x4.TRS(Vector3.back, Quaternion.AngleAxis(90f, Vector3.left), Vector3.one),
        };
    Material _material;

    Vector3 start;
    bool dragging;
    TransformData prev;

    Mesh cone;
    Mesh cube;

    TransformDirection selected = TransformDirection.None;

    #region Circumference

    const int SPHERE_RESOLUTION = 32;
    List<Vector3> circumY;    //윗방향

    #endregion

    Transform[] nodeDirections = new Transform[4];

    void Awake() {
        mode = (TransformMode)UIManager.Instance.GetMode();

        cone = CreateCone(5, 0.1f, HANDLER_SIZE);
        cube = CreateCube(HANDLER_SIZE);

        GetCircumference(SPHERE_RESOLUTION,
            out circumY);

        for(int i = 1; i < transform.childCount; i++) {
            nodeDirections[i-1] = transform.GetChild(i);
        }
    }
    // Usage: Call Control() method in Update() loop 
    void Update() {
        Control();
    }
    public void Control() {
        if (Input.GetMouseButtonDown(0)) {
            dragging = true;
            start = Input.mousePosition;
            prev = new TransformData(transform);
            Pick(start);
        } else if (Input.GetMouseButtonUp(0)) {
            dragging = false;
            hasPreMousePos = false; //
            //transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(0, 90f * Mathf.Round(transform.GetChild(0).rotation.eulerAngles.y / 90), 0));
            transform.rotation = Quaternion.Euler(new Vector3(0, 90f * Mathf.Round(transform.rotation.eulerAngles.y / 90), 0));
            selected = TransformDirection.None;
        }
        if (dragging) {
            Drag();
        }
    }
    public bool Pick(Vector3 mouse) {
        selected = TransformDirection.None;

        switch (mode) {
            case TransformMode.Translate:
            case TransformMode.Scale:
                return PickOrthogonal(mouse);
            case TransformMode.Rotate:
                return PickSphere(mouse);
        }

        return false;
    }
    Matrix4x4 GetTranform() {
        float scale = 1f;
        return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one * scale);
        //return Matrix4x4.TRS(transform.GetChild(0).position, transform.rotation, Vector3.one * scale);
    }
    bool PickOrthogonal(Vector3 mouse) {
        var cam = Camera.main;

        var matrix = GetTranform();

        var origin = cam.WorldToScreenPoint(matrix.MultiplyPoint(Vector3.zero)).xy();
        var right = cam.WorldToScreenPoint(matrix.MultiplyPoint(Vector3.right)).xy() - origin;
        var rightHead = cam.WorldToScreenPoint(matrix.MultiplyPoint(Vector3.right * (1f + HANDLER_SIZE))).xy() - origin;
        var forward = cam.WorldToScreenPoint(matrix.MultiplyPoint(Vector3.forward)).xy() - origin;
        var forwardHead = cam.WorldToScreenPoint(matrix.MultiplyPoint(Vector3.forward * (1f + HANDLER_SIZE))).xy() - origin;

        var xz = cam.WorldToScreenPoint(matrix.MultiplyPoint(Vector3.right + Vector3.forward)).xy() - origin;
        var xzHead = cam.WorldToScreenPoint(matrix.MultiplyPoint((Vector3.forward + Vector3.right) * (0.5f + HANDLER_SIZE))).xy() - origin;

        var v = mouse.xy() - origin;
        var vl = v.magnitude;

        // Add THRESHOLD to each magnitude to ignore a direction.

        var xRight = v.Orth(right).magnitude;
        if (Vector2.Dot(v, right) <= -float.Epsilon || vl > rightHead.magnitude) {
            xRight += THRESHOLD;
        }
        var xLeft = v.Orth(-right).magnitude;
        if (Vector2.Dot(v, -right) <= -float.Epsilon || vl < -rightHead.magnitude) {
            xLeft += THRESHOLD;
        }
        //

        var zForward = v.Orth(forward).magnitude;
        if (Vector2.Dot(v, forward) <= -float.Epsilon || vl > forwardHead.magnitude) {
            zForward += THRESHOLD;
        }
        var zBack = v.Orth(-forward).magnitude;
        if (Vector2.Dot(v, -forward) <= -float.Epsilon || vl < -forwardHead.magnitude) {
            zBack += THRESHOLD;
        }
        //

        var xzl = v.Orth(xz).magnitude;
        if (Vector2.Dot(v, xz) <= -float.Epsilon || vl > xzHead.magnitude) {
            xzl += THRESHOLD;
        }

        if (xRight < zForward && xRight < THRESHOLD) {
            selected = TransformDirection.Xright;
        } else if (xLeft < zForward && xLeft < THRESHOLD) {
            selected = TransformDirection.Xleft;
        } else if (zForward < xRight && zForward < THRESHOLD) {
            selected = TransformDirection.Zforward;
        } else if (zBack < xRight && zBack < THRESHOLD) {
            selected = TransformDirection.Zback;
        } else if(xzl < xRight && xzl < zForward && xzl < THRESHOLD) {
            selected = TransformDirection.XZ;
        }


        return selected != TransformDirection.None;
    }
    bool PickSphere(Vector3 mouse) {
        var cam = Camera.main;

        var matrix = GetTranform();

        var v = mouse.xy();
        var y = circumY.Select(p => cam.WorldToScreenPoint(matrix.MultiplyPoint(p)).xy()).ToList();

        float xl, yl, zl;
        xl = yl = zl = float.MaxValue;
        for (int i = 0; i < SPHERE_RESOLUTION; i++) {
            yl = Mathf.Min(yl, (v - y[i]).magnitude);
        }

        if (yl < xl && yl < zl && yl < THRESHOLD) {
            selected = TransformDirection.Y;
        }

        return selected != TransformDirection.None;
    }
    void GetCircumference(int resolution,
        out List<Vector3> y) {
        y = new List<Vector3>();

        var pi2 = Mathf.PI * 2f;
        for (int i = 0; i < resolution; i++) {
            var r = (float)i / resolution * pi2;
            y.Add(new Vector3(Mathf.Cos(r), 0f, Mathf.Sin(r)));
        }
    }
    bool GetStartProj(out Vector3 proj) {
        proj = default(Vector3);

        var plane = new Plane((Camera.main.transform.position - prev.position).normalized, prev.position);
        var ray = Camera.main.ScreenPointToRay(start);
        if (plane.Raycast(ray, out float distance)) {
            var point = ray.GetPoint(distance);             //ray를 따라 distance만큼 이동한 지점(교차지점), 마우스로 찍은 지점
            var axis = prev.rotation * axes[selected];
            var dir = point - prev.position;
            proj = Vector3.Project(dir, axis.normalized);
            return true;
        }
        return false;
    }
    float GetStartDistance() {
        if (GetStartProj(out Vector3 proj)) {
            return proj.magnitude;
        }
        return 0f;
    }
    void Drag() {
        switch (mode) {
            case TransformMode.Translate:
                Translate();
                break;
            case TransformMode.Rotate:
                Rotate();
                break;
            case TransformMode.Scale:
                Scale();
                break;
        }
    }
    void Translate() {
        if (selected == TransformDirection.None) return;

        var plane = new Plane((Camera.main.transform.position - prev.position).normalized, prev.position);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance)) {
            var point = ray.GetPoint(distance);
            var axisX = prev.rotation * axes[selected == TransformDirection.XZ || selected == TransformDirection.Xright ? TransformDirection.Xright : TransformDirection.None];
            var axisZ = prev.rotation * axes[selected == TransformDirection.XZ || selected == TransformDirection.Zforward ? TransformDirection.Zforward : TransformDirection.None];
            var dir = point - prev.position;
            var projX = Vector3.Project(dir, axisX.normalized);
            var projZ = Vector3.Project(dir, axisZ.normalized);

            if (GetStartProj(out Vector3 start)) {
                var offsetX = (new Vector3(start.x, 0, 0)).magnitude;       //찍은 지점
                var offsetZ = (new Vector3(0, 0, start.z)).magnitude;
                var curX = projX.magnitude;
                var curZ = projZ.magnitude;
                if (Vector3.Dot(start, projX) >= 0f) {
                    projX = ((curX - offsetX) * projX.normalized).Round() * 0.5f;
                } else {
                    projX = ((curX + offsetX) * projX.normalized).Round() * 0.5f;
                }
                if (Vector3.Dot(start, projZ) >= 0f) {
                    projZ = ((curZ - offsetZ) * projZ.normalized).Round() * 0.5f;
                } else {
                    projZ = ((curZ + offsetZ) * projZ.normalized).Round() * 0.5f;
                }
            }
            transform.position = prev.position + projX + projZ;
        }
    }
    [Range(-180f, 180f)]
    public float angle;

    public bool hasPreMousePos = false;
    public Vector3 preMousePos;
    void Rotate() {
        if (selected == TransformDirection.None) return;

        var rotateAxis = axes[selected];
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (hasPreMousePos) {

            var dirVec = new Vector3(mousePos.x - transform.GetChild(0).position.x, 0f, mousePos.z - transform.GetChild(0).position.z);
            //var dirVec = new Vector3(mousePos.x - transform.position.x, 0f, mousePos.z - transform.position.z);
            var preVec = new Vector3(preMousePos.x - transform.GetChild(0).position.x, 0f, preMousePos.z - transform.GetChild(0).position.z);
            //var preVec = new Vector3(preMousePos.x - transform.position.x, 0f, preMousePos.z - transform.position.z);

            //90도 회전을 위한 장치
            //angle += SignedAngle(preVec, dirVec, Vector3.up);

            //if (Mathf.Abs(angle) > 90f) {
            //    transform.GetChild(0).rotation = transform.GetChild(0).rotation * Quaternion.AngleAxis(90f * Mathf.Sign(angle), rotateAxis);
            //    angle = 0;
            //}
            //

            //부드러운 회전을 위해서는 이부분 주석 해제
            angle = SignedAngle(preVec, dirVec, Vector3.up);
            //transform.GetChild(0).rotation = transform.GetChild(0).rotation * Quaternion.AngleAxis(angle, rotateAxis);
            transform.rotation = transform.rotation * Quaternion.AngleAxis(angle, rotateAxis);
        }
        preMousePos = mousePos;
        hasPreMousePos = true;
    }
    public static float SignedAngle(Vector3 from, Vector3 to, Vector3 normal) {
        // angle in [0,180]
        float angle = Vector3.Angle(from, to);
        float sign = Mathf.Sign(Vector3.Dot(normal, Vector3.Cross(from, to)));
        return angle * sign;
    }
    void Scale() {
        if (selected == TransformDirection.None) return;

        var plane = new Plane((Camera.main.transform.position - transform.position).normalized, prev.position);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance)) {
            var point = ray.GetPoint(distance);
            var axis = prev.rotation * axes[selected];
            var dir = point - prev.position;
            var proj = Vector3.Project(dir, axis.normalized);
            var offset = GetStartDistance();

            var mag = 0f;
            if (proj.magnitude < offset) {
                mag = 1f - (offset - proj.magnitude) / offset;
            } else {
                mag = proj.magnitude / offset;
            }

            var scale = transform.localScale;
            switch (selected) {
                case TransformDirection.Xright:
                    scale.x = Mathf.Ceil(prev.scale.x * mag);
                    break;
                case TransformDirection.Xleft:
                    scale.x = Mathf.Ceil(prev.scale.x * mag);
                    break;
                case TransformDirection.Zforward:
                    scale.z = Mathf.Ceil(prev.scale.z * mag);
                    break;
                case TransformDirection.Zback:
                    scale.z = Mathf.Ceil(prev.scale.z * mag);
                    break;

            }
            transform.localScale = scale;
        }

    }
    void OnRenderObject() {
        if (mode == TransformMode.None) return;

        GL.PushMatrix();

        GL.MultMatrix(GetTranform());

        switch (mode) {
            case TransformMode.Translate:
                DrawTranslate();
                break;

            case TransformMode.Rotate:
                DrawRotate();
                break;

            case TransformMode.Scale:
                DrawScale();
                break;
        }

        GL.PopMatrix();
    }
    void DrawLine(Vector3 start, Vector3 end, Color color) {
        GL.Begin(GL.LINES);
        GL.Color(color);
        GL.Vertex(start);
        GL.Vertex(end);
        GL.End();
    }
    void DrawQuad(Vector3 start, Vector3 end, Color color) {
        GL.Begin(GL.QUADS);
        GL.Color(color);

        GL.Vertex(start);
        GL.Vertex3(start.x, 0, end.z * 0.4f);
        GL.Vertex3(0.4f, 0, end.z * 0.4f);
        GL.Vertex3(0.4f, 0, 0);

        GL.End();
    }
    void DrawMesh(Mesh mesh, Matrix4x4 m, Color color) {
        GL.Begin(GL.TRIANGLES);
        GL.Color(color);

        var vertices = mesh.vertices;
        for (int i = 0, n = vertices.Length; i < n; i++) {
            vertices[i] = m.MultiplyPoint(vertices[i]);
        }

        var triangles = mesh.triangles;
        for (int i = 0, n = triangles.Length; i < n; i += 3) {
            int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
            GL.Vertex(vertices[a]);
            GL.Vertex(vertices[b]);
            GL.Vertex(vertices[c]);
        }

        GL.End();
    }
    void DrawTranslate() {
        Getmaterial().SetInt("_ZTest", (int)CompareFunction.Always);
        Getmaterial().SetPass(0);

        // x axis
        var color = selected == TransformDirection.Xright ? colors[3] : colors[0];
        DrawLine(Vector3.zero, Vector3.right, color);
        DrawMesh(cone, matrices[0], color);

        // z axis
        color = selected == TransformDirection.Zforward ? colors[3] : colors[2];
        DrawLine(Vector3.zero, Vector3.forward, color);
        DrawMesh(cone, matrices[1], color);     //y축을 지움으로 index 한칸 땡김

        color = selected == TransformDirection.XZ ? colors[3] : colors[1];
        DrawQuad(Vector3.zero, Vector3.forward, color);
    }
    void DrawRotate() {
        Getmaterial().SetInt("_ZTest", (int)CompareFunction.Always);
        Getmaterial().SetPass(0);

        GL.Begin(GL.LINES);
        var color = selected == TransformDirection.Y ? colors[3] : colors[1];
        GL.Color(color);
        Getmaterial().SetPass(0);
        for (int i = 0; i < SPHERE_RESOLUTION; i++) {
            var cur = circumY[i];
            var next = circumY[(i + 1) % SPHERE_RESOLUTION];
            GL.Vertex(cur);
            GL.Vertex(next);
        }
        GL.End();
    }
    void DrawScale() {
        Getmaterial().SetInt("_ZTest", (int)CompareFunction.Always);
        Getmaterial().SetPass(0);

        var right = Matrix4x4.TRS(Vector3.right * transform.localScale.x * 0.5f, Quaternion.AngleAxis(90f, Vector3.back), Vector3.one);
        var left = Matrix4x4.TRS(Vector3.left * transform.localScale.x * 0.5f, Quaternion.AngleAxis(90f, Vector3.forward), Vector3.one);
        var forward = Matrix4x4.TRS(Vector3.forward * transform.localScale.z * 0.5f, Quaternion.AngleAxis(90f, Vector3.right), Vector3.one);
        var back = Matrix4x4.TRS(Vector3.back * transform.localScale.z * 0.5f, Quaternion.AngleAxis(90f, Vector3.left), Vector3.one);

        // x axis
        var color = selected == TransformDirection.Xright ? colors[3] : colors[1];
        //DrawLine(new Vector3(-transform.localScale.x*0.5f,0, -transform.localScale.z * 0.5f), Vector3.right, color);
        DrawMesh(cube, right, color);
        
        color = selected == TransformDirection.Xleft ? colors[3] : colors[1];
        //DrawLine(Vector3.zero, Vector3.left, color);
        DrawMesh(cube, left, color);


        // z axis
        color = selected == TransformDirection.Zforward ? colors[3] : colors[2];
        //DrawLine(Vector3.zero, Vector3.forward, color);
        DrawMesh(cube,forward, color);

        color = selected == TransformDirection.Zback ? colors[3] : colors[2];
        //DrawLine(Vector3.zero, Vector3.back, color);
        DrawMesh(cube, back, color);
        
    }
    //Translate Mode에서는 화살표, Scale Mode에서는 육면체
    #region Mesh
    Mesh CreateCone(int subdivisions, float radius, float height) {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[subdivisions + 2];
        int[] triangles = new int[(subdivisions * 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0, n = subdivisions - 1; i < subdivisions; i++) {
            float ratio = (float)i / n;
            float r = ratio * (Mathf.PI * 2f);
            float x = Mathf.Cos(r) * radius;
            float z = Mathf.Sin(r) * radius;
            vertices[i + 1] = new Vector3(x, 0f, z);
        }
        vertices[subdivisions + 1] = new Vector3(0f, height, 0f);

        // construct bottom
        for (int i = 0, n = subdivisions - 1; i < n; i++) {
            int offset = i * 3;
            triangles[offset] = 0;
            triangles[offset + 1] = i + 1;
            triangles[offset + 2] = i + 2;
        }

        // construct sides
        int bottomOffset = subdivisions * 3;
        for (int i = 0, n = subdivisions - 1; i < n; i++) {
            int offset = i * 3 + bottomOffset;
            triangles[offset] = i + 1;
            triangles[offset + 1] = subdivisions + 1;
            triangles[offset + 2] = i + 2;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        return mesh;
    }
    Mesh CreateCube(float size) {
        var mesh = new Mesh();

        var hsize = size * 0.5f;
        mesh.vertices = new Vector3[] {
                new Vector3 (-hsize, -hsize, -hsize),
                new Vector3 ( hsize, -hsize, -hsize),
                new Vector3 ( hsize,  hsize, -hsize),
                new Vector3 (-hsize,  hsize, -hsize),
                new Vector3 (-hsize,  hsize,  hsize),
                new Vector3 ( hsize,  hsize,  hsize),
                new Vector3 ( hsize, -hsize,  hsize),
                new Vector3 (-hsize, -hsize,  hsize),
            };

        mesh.triangles = new int[] {
                0, 2, 1, //face front
				0, 3, 2,
                2, 3, 4, //face top
				2, 4, 5,
                1, 2, 5, //face right
				1, 5, 6,
                0, 7, 4, //face left
				0, 4, 3,
                5, 4, 7, //face back
				5, 7, 6,
                0, 6, 7, //face bottom
				0, 1, 6
            };

        return mesh;
    }

    #endregion

}
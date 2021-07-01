using UnityEngine;


public static class VectorExtension {

    public static Vector2 xy(this Vector3 v) {
        return new Vector2(v.x, v.y);
    }
    public static Vector2 xz(this Vector3 v) {
        return new Vector2(v.x, v.z);
    }
    // orthogonal vector v2 to v1
    public static Vector2 Orth(this Vector2 from, Vector2 to) {
        return from.Project(to) - from;
    }

    // projection vector v2 to v1
    public static Vector2 Project(this Vector2 from, Vector2 to) {
        return to * (Vector2.Dot(to, from) / Mathf.Pow(to.magnitude, 2f));
    }

    public static Vector2 Perp(this Vector2 v) {
        return new Vector2(v.y, -v.x);
    }

    public static Vector3 Round(this Vector3 v) {
        return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
    }
    public static Vector3 Ceil(this Vector3 v) {
        return new Vector3(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z));
    }

    public static Vector3 IngredientMultiply(this Vector3 a, Vector3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector3 LeastScale(this Vector3 v) {
        return new Vector3(Mathf.Clamp(v.x, 1f, float.MaxValue), v.y, Mathf.Clamp(v.z, 1f, float.MaxValue));
    }
}




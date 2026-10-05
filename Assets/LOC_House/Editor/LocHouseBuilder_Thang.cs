using UnityEngine;
using UnityEditor;

// [1/10] hoàn thiện cầu thang: trần dốc dưới vế (che răng cưa), len chân tường theo bậc, tay vịn sát tường, trụ chiếu nghỉ.
public static partial class LocHouseBuilder
{
    static Material GoThang => M("M_ChanThang", "#5E4230");

    // tayVin: 0 không · -1 tay vịn gỗ sát tường phía x0 · +1 phía x1. tuong: len chân ở phía tường (trái/phải).
    static void HoanThienThang(string n, float x0, float x1, float zs, int dir, float baseY, float rise, int risers, float run,
                               bool tuongTrai, bool tuongPhai, int tayVin)
    {
        int steps = risers - 1;
        var g = new GameObject(n + "_HoanThien").transform; g.SetParent(cur, false);
        var old = cur; cur = g;

        // trần dốc: tấm đặc giữa đường qua 2 góc sau-dưới của các bậc và đường qua góc sau-trên → lấp hết răng cưa gầm thang
        if (rise > 0 && steps >= 2)
        {
            var a = new Vector3((x0 + x1) / 2, baseY - 0.12f, zs + dir * run);
            var b = new Vector3((x0 + x1) / 2, baseY + rise * (steps - 1) - 0.12f, zs + dir * run * steps);
            var d = b - a; var rot = Quaternion.LookRotation(d, Vector3.up);
            float vt = rise + 0.12f, cos = d.normalized.z != 0 ? Mathf.Abs(new Vector3(0, d.y, d.z).normalized.z) : 1f, t = vt * cos;
            var s = GameObject.CreatePrimitive(PrimitiveType.Cube); s.name = "GamThang";
            Object.DestroyImmediate(s.GetComponent<Collider>());
            s.transform.SetParent(cur, false);
            s.transform.rotation = rot;
            s.transform.position = (a + b) / 2 + rot * Vector3.up * (t / 2 - 0.02f);   // nhô xuống 2 cm dưới góc bậc: không trùng mặt với đáy bậc
            s.transform.localScale = new Vector3(x1 - x0 - 0.01f, t, d.magnitude);   // [1/10] hụt 5 mm mỗi bên: hông tấm không trùng mặt hông bậc (gây nháy)
            s.GetComponent<MeshFilter>().sharedMesh = WorldUVBox(s.transform.position, s.transform.localScale, 1f, 1f);   // cùng granito với bậc
            s.GetComponent<Renderer>().sharedMaterial = Granito;
            GameObjectUtility.SetStaticEditorFlags(s, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
        }

        // len chân tường theo từng bậc (cao 11 cm, dày 2 cm)
        for (int i = 0; i < steps; i++)
        {
            float top = baseY + rise * (i + 1), za = zs + dir * run * i, zb = zs + dir * run * (i + 1);
            float z0 = Mathf.Min(za, zb), z1 = Mathf.Max(za, zb);
            if (tuongTrai) Box($"{n}_ChanT{i + 1:00}", x0, x0 + 0.02f, top, top + 0.11f, z0, z1, GoThang, false);
            if (tuongPhai) Box($"{n}_ChanP{i + 1:00}", x1 - 0.02f, x1, top, top + 0.11f, z0, z1, GoThang, false);
        }

        // tay vịn gỗ sát tường, cao 0,90 so với mũi bậc, 3 gối đỡ
        if (tayVin != 0)
        {
            float x = tayVin < 0 ? x0 + 0.075f : x1 - 0.075f;
            float zA = zs, yA = baseY + rise + 0.9f, zB = zs + dir * run * steps, yB = baseY + rise * risers + 0.9f;
            Rail(n + "_TayVinTuong", x, zA, yA, zB, yB);
            for (int k = 0; k < 3; k++)
            {
                float f = (k + 0.5f) / 3f, zz = Mathf.Lerp(zA, zB, f), yy = Mathf.Lerp(yA, yB, f);
                float xw = tayVin < 0 ? x0 : x1 - 0.05f;
                Box($"{n}_GoiDo{k}", xw, xw + 0.05f, yy - 0.09f, yy - 0.03f, zz - 0.02f, zz + 0.02f, Sat, false);
            }
        }
        cur = old;
    }

    // trụ lan can ở góc chiếu nghỉ (cao 1,0 so với mặt chiếu nghỉ) + cột đỡ góc chiếu nghỉ từ sàn
    static void TruChieuNghi(string n, float x, float z, float landY, float floorY, float zCot)
    {
        Box(n + "_Tru", x - 0.045f, x + 0.045f, landY, landY + 1.0f, z - 0.045f, z + 0.045f, GoThang, true);
        Box(n + "_Mu", x - 0.06f, x + 0.06f, landY + 1.0f, landY + 1.04f, z - 0.06f, z + 0.06f, GoThang, false);
        Box(n + "_Cot", x - 0.08f, x + 0.02f, floorY, landY - 0.15f, zCot - 0.10f, zCot, Tuong, true);
    }
}

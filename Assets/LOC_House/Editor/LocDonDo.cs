using System.Linq;
using UnityEditor;
using UnityEngine;

// [6/10 tối] Dọn đồ theo góp ý của người chơi — sửa thẳng scene hiện tại (không dựng lại nhà). Idempotent.
// Menu: LOC → Dọn đồ theo góp ý 06-10
public static class LocDonDo
{
    [MenuItem("LOC/Dọn đồ theo góp ý 06-10")]
    public static void Run()
    {
        int n = 0;
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToArray())
        {
            if (!t) continue;
            string nm = t.name; var p = t.position; string par = t.parent ? t.parent.name : "";
            bool xoa =
                nm.StartsWith("CuaPhongNhim_") ||
                (nm.StartsWith("ChauCayKieng_LaTo") && par == "PhongKhach" && Mathf.Abs(p.x - 0.45f) < 0.3f && Mathf.Abs(p.z - 0.95f) < 0.3f) ||
                (nm.StartsWith("GheDau_Go") && par == "PhongKhach" && Mathf.Abs(p.x - 5.25f) < 0.3f && Mathf.Abs(p.z - 4.6f) < 0.3f);
            if (!xoa) continue;
            Debug.Log("[LOC] Xoá: " + t.name + " @ " + p);
            Undo.DestroyObjectImmediate(t.gameObject); n++;
        }
        // [6/10 tối] dời / dựng lại
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string nm = t.name, par = t.parent ? t.parent.name : ""; var p = t.position;
            if (nm.StartsWith("GheDau_Go") && par == "PhongTho" && Mathf.Abs(p.x - 6.4f) < 0.4f) { t.position = new Vector3(7.15f, p.y, 3.5f); n++; }
            if (nm.StartsWith("DoTuDauGiuong_Bo") && par == "PhongBoMe" && p.z < 1.1f) { t.position = new Vector3(6.7f, p.y, 1.25f); n++; }
            if (par == "BanHang" && nm == "CanBanHang") { t.position = new Vector3(-0.75f, p.y, -1.05f); n++; }
            if (par == "BanHang" && nm == "CuonDayDien") { t.position = new Vector3(-1.9f, p.y, 1.55f); n++; }
            if (par == "BanHang" && nm.StartsWith("SM_PlasticChairStack_4")) { t.position = new Vector3(-4.1f, p.y, 0.35f); n++; }
        }
        if (!GameObject.Find("CuaNhim (mở)"))
        {
            Transform ph = null; foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (t.name == "PhongNhim") ph = t;
            Material mat = null; foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (r.transform.parent && r.transform.parent.name.StartsWith("CuaKhoi")) mat = r.sharedMaterial;
            if (ph)
            {
                var g = new GameObject("CuaNhim (mở)"); g.transform.SetParent(ph, false);
                g.transform.SetPositionAndRotation(new Vector3(3.15f, 3.6f, 9.85f), Quaternion.Euler(0, 180, 0));
                var c = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(c.GetComponent<Collider>());
                c.name = "Canh"; c.transform.SetParent(g.transform, false);
                c.transform.localPosition = new Vector3(0.44f, 1.025f, 0); c.transform.localScale = new Vector3(0.88f, 2.05f, 0.04f);
                if (mat) c.GetComponent<Renderer>().sharedMaterial = mat;
                var d = g.AddComponent<LocDoor>(); d.ten = "cửa phòng Nhím"; d.dungSanLaMo = true; d.goc = -90f;
                var bc = g.AddComponent<BoxCollider>(); bc.center = new Vector3(0.44f, 1.025f, 0); bc.size = new Vector3(0.88f, 2.05f, 0.06f);
                Undo.RegisterCreatedObjectUndo(g, "Cửa Nhím"); n++;
            }
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"[LOC] Dọn đồ xong: xoá {n} vật. Nhớ Ctrl+S.");
    }
}

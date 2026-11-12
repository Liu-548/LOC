# Xương rút gọn (Generic, ~44 xương) dựng từ khớp của bộ xương default MakeHuman + gộp trọng số.
import bpy, numpy as np, re
from mathutils import Vector, Matrix, Quaternion

FINGERS = {1: "Thumb", 2: "Index", 3: "Middle", 4: "Ring", 5: "Pinky"}

def map_bone(b):
    """tên xương MH → tên xương LỘC"""
    s = "_L" if b.endswith(".L") else "_R" if b.endswith(".R") else ""
    n = b[:-2] if s else b
    if n in ("root", "pelvis", "spine05"): return "Hips"
    if n in ("spine04", "spine03"): return "Spine"
    if n in ("spine02", "spine01", "breast", "special01"): return "Chest"
    if n.startswith("neck"): return "Neck"
    if n in ("clavicle", "shoulder01"): return "Shoulder" + s
    if n.startswith("upperarm"): return "UpperArm" + s
    if n.startswith("lowerarm"): return "LowerArm" + s
    if n in ("wrist",) or n.startswith("metacarpal"): return "Hand" + s
    m = re.match(r"finger(\d)-(\d)", n)
    if m: return f"{FINGERS[int(m[1])]}{1 if m[2] == '1' else 2}{s}"
    if n.startswith("upperleg"): return "UpperLeg" + s
    if n.startswith("lowerleg"): return "LowerLeg" + s
    if n == "foot": return "Foot" + s
    if n.startswith("toe"): return "Toes" + s
    if n == "eye": return "Eye" + s
    return "Head"   # head, jaw, mặt, lưỡi

def bone_defs(J):
    """J(name) → toạ độ khớp. Trả về [(tên, cha, head, tail)]"""
    d = [("Hips", None, J("root____head") * Vector((1, 0, 1)) + Vector((0, J("spine04____head").y, 0)), J("spine04____head")),
         ("Spine", "Hips", J("spine04____head"), J("spine02____head")),
         ("Chest", "Spine", J("spine02____head"), J("neck01____head")),
         ("Neck", "Chest", J("neck01____head"), J("head____head")),
         ("Head", "Neck", J("head____head"), J("head____tail"))]
    for s, S in (("_L", ".L"), ("_R", ".R")):
        d += [("Eye" + s, "Head", J(f"eye{S}____head"), J(f"eye{S}____head") + (J(f"eye{S}____tail") - J(f"eye{S}____head")).normalized() * 0.03),
              ("Shoulder" + s, "Chest", J(f"clavicle{S}____head"), J(f"upperarm01{S}____head")),
              ("UpperArm" + s, "Shoulder" + s, J(f"upperarm01{S}____head"), J(f"lowerarm01{S}____head")),
              ("LowerArm" + s, "UpperArm" + s, J(f"lowerarm01{S}____head"), J(f"wrist{S}____head")),
              ("Hand" + s, "LowerArm" + s, J(f"wrist{S}____head"), J(f"finger3-1{S}____head"))]
        for k, F in FINGERS.items():
            d += [(f"{F}1{s}", "Hand" + s, J(f"finger{k}-1{S}____head"), J(f"finger{k}-2{S}____head")),
                  (f"{F}2{s}", f"{F}1{s}", J(f"finger{k}-2{S}____head"), J(f"finger{k}-3{S}____tail"))]
        d += [("UpperLeg" + s, "Hips", J(f"upperleg01{S}____head"), J(f"lowerleg01{S}____head")),
              ("LowerLeg" + s, "UpperLeg" + s, J(f"lowerleg01{S}____head"), J(f"foot{S}____head")),
              ("Foot" + s, "LowerLeg" + s, J(f"foot{S}____head"), J(f"toe3-1{S}____head")),
              ("Toes" + s, "Foot" + s, J(f"toe3-1{S}____head"), J(f"toe3-3{S}____tail"))]
    return d

def make_armature(defs, palm_n):
    """palm_n: {'_L': Vector, '_R': Vector} — pháp tuyến mu bàn tay, để roll bàn tay/ngón cho trục Z = mu bàn tay"""
    ad = bpy.data.armatures.new("NhimRig"); ob = bpy.data.objects.new("NhimRig", ad)
    bpy.context.scene.collection.objects.link(ob); bpy.context.view_layer.objects.active = ob
    bpy.ops.object.mode_set(mode="EDIT")
    for n, p, h, t in defs:
        e = ad.edit_bones.new(n); e.head = h; e.tail = t
        if p: e.parent = ad.edit_bones[p]; e.use_connect = False
        side = "_L" if n.endswith("_L") else "_R" if n.endswith("_R") else ""
        if side and (n.startswith(("Hand", "Thumb", "Index", "Middle", "Ring", "Pinky"))):
            e.align_roll(palm_n[side])
        elif n.startswith(("Eye",)):
            e.align_roll(Vector((0, 0, 1)))
        else:
            e.align_roll(Vector((0, -1, 0)) if abs(Vector(t - h).normalized().y) < 0.9 else Vector((0, 0, 1)))
    bpy.ops.object.mode_set(mode="OBJECT")
    return ob

def skin(obj, arm, weights):
    """weights: {bone: (idx array, w array)} cho đỉnh của obj"""
    for b, (ix, w) in weights.items():
        vg = obj.vertex_groups.get(b) or obj.vertex_groups.new(name=b)
        for i, ww in zip(ix, w):
            if ww > 1e-4: vg.add([int(i)], float(ww), "REPLACE")
    m = obj.modifiers.new("Armature", "ARMATURE"); m.object = arm
    obj.parent = arm

def mh_weights(mhw, remap, nverts):
    """gộp trọng số MH theo map_bone; remap: chỉ số base mesh → chỉ số đỉnh obj (dict). Chuẩn hoá tổng = 1."""
    acc = {}
    for b, lst in mhw.items():
        t = map_bone(b)
        a = acc.setdefault(t, np.zeros(nverts))
        for vi, w in lst:
            j = remap.get(vi)
            if j is not None: a[j] += w
    tot = sum(acc.values()); tot[tot == 0] = 1
    return {b: (np.nonzero(a)[0], (a / tot)[np.nonzero(a)[0]]) for b, a in acc.items() if a.any()}

# ───── đặt tư thế bằng hướng xương (trong "khung thân" đứng thẳng), rồi nhân với hướng toàn cục G
def aim_matrix(rest3, rest_y, d):
    """xoay tối thiểu đưa trục Y nghỉ → d"""
    q = Vector(rest_y).normalized().rotation_difference(Vector(d).normalized())
    return q.to_matrix() @ rest3

class Poser:
    def __init__(self, arm):
        self.arm = arm; self.pb = arm.pose.bones
        self.rest = {b.name: b.bone.matrix_local.copy() for b in self.pb}

    def reset(self):
        for b in self.pb: b.matrix_basis = Matrix()
        bpy.context.view_layer.update()

    def apply(self, spec, G=Matrix(), hips_loc=None):
        """spec: {bone: dict(d=hướng Y (khung thân), twist=độ quanh Y, rot=Matrix3 tuyệt đối, local=(trục, độ))}
        Thứ tự cha → con. G: Matrix4 hướng toàn cục áp lên cả khung thân (vd nằm nghiêng)."""
        self.reset()
        G3 = G.to_3x3()
        order = [b.name for b in self.arm.data.bones]   # bones theo thứ tự cha trước
        for n in order:
            pb = self.pb[n]; s = spec.get(n, {})
            bpy.context.view_layer.update()
            cur = pb.matrix.copy()
            R = self.rest[n].to_3x3()
            if n == "Hips" or "d" in s or "rot" in s or "twist" in s:
                if "rot" in s: body3 = s["rot"]
                else:
                    body3 = aim_matrix(R, R.col[1], s["d"]) if "d" in s else (self._body_rot(n) if n != "Hips" else R)
                    if "fa" in s and "d" in s:   # xoắn quanh trục xương để xương con (cẳng tay) chỉ đúng hướng fa
                        ch = self.arm.data.bones[n].children[0]
                        yc = (body3 @ R.inverted() @ ch.matrix_local.to_3x3()).col[1]
                        dd = Vector(s["d"]).normalized(); a = (yc - dd * yc.dot(dd)).normalized(); b = Vector(s["fa"]); b = (b - dd * b.dot(dd)).normalized()
                        ang = a.angle(b, 0.0); 
                        if a.cross(b).dot(dd) < 0: ang = -ang
                        body3 = Quaternion(dd, ang).to_matrix() @ body3
                if "twist" in s:
                    ax = body3.col[1].normalized()
                    body3 = Quaternion(ax, np.radians(s["twist"])).to_matrix() @ body3
                M = (G3 @ body3).to_4x4()
                if n == "Hips":
                    loc = G @ (hips_loc if hips_loc is not None else self.rest[n].translation)
                    M.translation = loc
                else:
                    M.translation = cur.translation
                pb.matrix = M
            if "local" in s:
                bpy.context.view_layer.update()
                for ax, deg in s["local"]:
                    pb.matrix_basis = pb.matrix_basis @ Matrix.Rotation(np.radians(deg), 4, ax)
        bpy.context.view_layer.update()

    def _body_rot(self, n):
        """giữ nguyên hướng nghỉ tương đối với cha (khi không chỉ định) — trả về None để không đụng"""
        return self.rest[n].to_3x3()

    def key(self, frame, bones=None):
        for b in self.pb:
            if bones and b.name not in bones: continue
            b.rotation_mode = "QUATERNION"
            b.keyframe_insert("rotation_quaternion", frame=frame)
            if b.name == "Hips": b.keyframe_insert("location", frame=frame)

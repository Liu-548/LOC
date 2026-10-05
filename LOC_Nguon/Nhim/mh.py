# Đọc base mesh MakeHuman (hm08, CC0) + targets.npz (CC0) → lưới bé gái 7 tuổi.
# Toạ độ MH: Y lên, đơn vị dm, mặt trước +Z. Hàm to_blender đổi sang Blender (Z lên, mét, mặt trước −Y).
import numpy as np, json, os
D = os.path.join(os.path.dirname(os.path.abspath(__file__)), "mh")

def load_obj(path=f"{D}/base.obj"):
    V, VT, F = [], [], []
    g = None
    for line in open(path):
        if line.startswith("v "): V.append([float(x) for x in line.split()[1:4]])
        elif line.startswith("vt "): VT.append([float(x) for x in line.split()[1:3]])
        elif line.startswith("g "): g = line.split()[1]
        elif line.startswith("f "):
            vs, ts = [], []
            for tok in line.split()[1:]:
                p = tok.split("/"); vs.append(int(p[0]) - 1); ts.append(int(p[1]) - 1 if len(p) > 1 and p[1] else -1)
            F.append((g, vs, ts))
    return np.array(V), np.array(VT), F

_npz = None
def target(name):
    global _npz
    if _npz is None: _npz = np.load(f"{D}/makehuman/data/targets.npz")
    k = f"targets/{name}"
    return _npz[k + ".index"].astype(int), _npz[k + ".vector"].astype(float) * 1e-3

def morph(V, weights):
    V = V.copy()
    for name, w in weights.items():
        if abs(w) < 1e-6: continue
        i, d = target(name); np.add.at(V, i, d * w)
    return V

def macro(gender=0.0, age_years=7, race={"asian": 1.0}, muscle=0.5, weight=0.5):
    """Trọng số target theo đúng công thức MacroModifier của MakeHuman."""
    age = (age_years - 1) / 48 if age_years < 25 else 0.5 + (age_years - 25) / 130
    if age < 0.5:
        old = 0; baby = max(0, 1 - age * 5.333); young = max(0, (age - 0.1875) * 3.2); child = max(0, min(1, 5.333 * age) - young)
    else:
        child = baby = 0; old = max(0, age * 2 - 1); young = 1 - old
    A = {"baby": baby, "child": child, "young": young, "old": old}
    G = {"female": 1 - gender, "male": gender}
    M = {"minmuscle": max(0, 1 - muscle * 2), "averagemuscle": 1 - abs(muscle - 0.5) * 2, "maxmuscle": max(0, muscle * 2 - 1)}
    W = {"minweight": max(0, 1 - weight * 2), "averageweight": 1 - abs(weight - 0.5) * 2, "maxweight": max(0, weight * 2 - 1)}
    out = {}
    for r, rv in race.items():
        for g, gv in G.items():
            for a, av in A.items():
                out[f"macrodetails/{r}-{g}-{a}"] = rv * gv * av
    for g, gv in G.items():
        for a, av in A.items():
            for m, mv in M.items():
                for w, wv in W.items():
                    out[f"macrodetails/universal-{g}-{a}-{m}-{w}"] = gv * av * mv * wv
    return out

def to_blender(P):
    P = np.asarray(P) * 0.1
    return np.stack([P[..., 0], -P[..., 2], P[..., 1]], -1)

def load_skel(path=f"{D}/default.mhskel"):
    return json.load(open(path))

def load_weights(path=f"{D}/default_weights.mhw"):
    return json.load(open(path))["weights"]   # {bone: [[vidx, w], ...]}

def joint_pos(skel, V, ref):
    """ref = {'strategy':'MEAN','vertex_indices':[...]} hoặc 'CUBE'/'VERTEX'."""
    s = ref.get("strategy", "MEAN")
    if s == "VERTEX": return V[ref["vertex_index"]]
    if s == "CUBE":
        return V[ref["cube_name"]] if False else None
    return V[ref["vertex_indices"]].mean(0)

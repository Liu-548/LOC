# NHÓM 1 · Bàn thờ vong phòng khách (GĐ3 prompt 04): dựng lại bàn + khăn trắng phủ (nếp gấp không đều, vết nhàu mép trước),
# phần còn lại (ảnh thờ, bát hương, chén, đèn dầu, vòng hoa…) nâng tự động giữ nguyên bố cục + node code tham chiếu.
from lib import *
from chung import *
import json, os
GIU = json.load(open(os.path.join(os.path.dirname(__file__), 'giu_node.json')))
VL_GIU = ("Bang_Chu_Vong_Hoa", "M_CT_Neon", "M_CocTrong", "M_ThuocVi")

def _ban_khan(c):
    a, b = c.bb("BanThoVong")
    ma, mb = c.bb("Khan_MatBan")            # mặt khăn gốc = mặt đặt đồ thờ
    W, D = mb.x - ma.x, mb.y - ma.y
    cx, cy = (ma.x + mb.x) / 2, (ma.y + mb.y) / 2
    top = mb.z - 0.002
    go = mat("go", "#7A6244", r=0.6, ten="go_ban_tho_vong")
    khan = mat("vai", "#E8E4DA", r=0.9, ten="khan_trang_nga", hai_mat=True)
    # bàn: mặt + bốn chân trụ vuông (chỉ lộ dưới mép khăn)
    box((W - 0.08, D - 0.08, 0.04), (cx, cy, top - 0.03), go, b=0.004)
    for sx in (-1, 1):
        for sy in (-1, 1):
            chan_thop(cx + sx * (W / 2 - 0.08), cy + sy * (D / 2 - 0.07), a.z, top - 0.05 - a.z, 0.05, 0.045, go)
    # khăn: mặt phẳng + bốn vạt rủ tới cách sàn 0,18 (mặt trước), nếp gấp không đều, một nếp sâu
    ru = top - (a.z + 0.18)
    nx, ny = 80, 40
    g = grid(W + 2 * ru, D + 2 * ru, nx, ny, (0, 0, 0), khan)
    nep = [(-0.42, 0.012), (-0.08, 0.026), (0.25, 0.01), (0.47, 0.014)]   # (vị trí theo bề ngang, độ sâu)
    def f(v):
        x, y = v.x, v.y
        ox = max(0, abs(x) - W / 2); oy = max(0, abs(y) - D / 2); o = max(ox, oy)
        if o <= 0:
            z = top + 0.001
            if y < -D / 2 + 0.08 and abs(x + 0.1) < 0.18: z += 0.004 * math.sin((x + 0.1) * 40) * (1 - abs(y + D / 2) / 0.08)   # vết nhàu chỗ tì tay
            return Vector((cx + x, cy + y, z))
        k = min(1, o / ru)
        bulge = 0.012 * math.sin(k * math.pi / 2)
        px = x if ox <= 0 else math.copysign(W / 2 + 0.006 + bulge, x)
        py = y if oy <= 0 else math.copysign(D / 2 + 0.006 + bulge, y)
        # nếp dọc trên vạt trước/sau
        if oy > 0 and ox <= 0:
            dz = sum(dep * math.exp(-((x / (W / 2) - p) * 9) ** 2) for p, dep in nep) * k
            py += math.copysign(dz, y)
        if ox > 0 and oy <= 0:
            py += 0.006 * math.sin(y * 30) * k
        z = top + 0.001 - o
        # hai góc trước rủ khác nhau
        if ox > 0 and oy > 0: z += (0.04 if x < 0 else 0.015) * k
        return Vector((cx + px, cy + py, z))
    deform(g, f); smooth(g, 70); solid(g, 0.002, -1)
    return []

def ban_tho_vong(c):
    out = nang_tu_goc(c, giu_node=GIU.get(c.ten, []), ten_vl_giu=VL_GIU,
                      bo=("Khan_Phu", "Khan_MatBan", "Khan_GocGapLoi", "Than_Ban_Go", "Bac_Ke"))
    _ban_khan(c)
    return out

CT = {"BanThoVong_Dem1": ban_tho_vong, "BanThoVong_Dem2": ban_tho_vong, "BanThoVong_Dem3": ban_tho_vong}

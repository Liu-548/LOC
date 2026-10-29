# Nâng cấp tự động cho mọi món chưa có công thức dựng tay: giữ bố cục gốc, vát/bo mịn, vật liệu PBR theo lớp.
import json, os
from lib import *
GIU = json.load(open(os.path.join(os.path.dirname(__file__), 'giu_node.json')))
NHOM = json.load(open('/home/claude/nhom_final.json'))
VL_GIU = ("Bang_Chu_Vong_Hoa", "M_CT_Neon", "M_CocTrong", "M_ThuocVi")

def _f(ten):
    def f(c):
        return nang_tu_goc(c, giu_node=GIU.get(ten, []), ten_vl_giu=VL_GIU)
    return f

CT = {n: _f(n) for g in ("1", "2", "3") for n in NHOM[g]}

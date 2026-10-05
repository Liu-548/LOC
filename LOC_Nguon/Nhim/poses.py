# Tư thế của Nhím (khung thân: trái +X, trước −Y, lên +Z). G = hướng toàn cục.
# Gốc đối tượng: Nam_*/Ngoi_DauGap = tâm mặt đệm giường (trục dài giường theo X, đầu giường +X, phía phòng +Y);
#                Ngoi_VayTay = sàn dưới tâm mặt ghế; Dung_DauGap = sàn chỗ đứng.
import numpy as np
from mathutils import Matrix, Vector

def M3(cols):
    return Matrix([[cols[0][i], cols[1][i], cols[2][i]] for i in range(3)])

# nằm nghiêng phải: đầu → +X, bên phải → xuống (−Z), mặt → +Y (ra phía phòng)
G_NAM = M3([(0, 0, 1), (0, -1, 0), (1, 0, 0)]).to_4x4()
G_DUNG = Matrix()                                   # đứng/ngồi: mặt −Y
def G_yaw(deg): return Matrix.Rotation(np.radians(deg), 4, "Z")

TAY_TRAI_BOT = {"UpperArm_L": {"d": (0.10, -0.42, -0.90)}}   # khuỷu trái luôn 90° (đã nằm trong tư thế nghỉ)

NAM = {   # nằm nghiêng, hơi co, tay phải gần cằm giữ mép chăn
    "Spine": {"d": (0.0, -0.18, 1)}, "Chest": {"d": (0.0, -0.22, 1)},
    "Neck": {"d": (-0.22, -0.28, 1)}, "Head": {"d": (-0.28, -0.22, 1)},
    "Shoulder_R": {"d": (-1, -0.25, 0.05)}, "Shoulder_L": {"d": (1, -0.2, 0.0)},
    "UpperArm_R": {"d": (-0.15, -0.95, -0.1)}, "LowerArm_R": {"d": (0.05, 0.45, 0.89)}, "Hand_R": {"local": [("X", -15)]},
    "UpperArm_L": {"d": (0.10, -0.30, -0.95), "fa": (-0.75, -0.55, -0.35)},   # cẳng tay bột vắt ngang bụng, tựa xuống nệm
    "UpperLeg_L": {"d": (0.06, -0.78, -0.62)}, "LowerLeg_L": {"d": (0.0, 0.42, -0.9)}, "Foot_L": {"d": (0.0, -0.35, -0.94)},
    "UpperLeg_R": {"d": (-0.04, -0.62, -0.78)}, "LowerLeg_R": {"d": (0.0, 0.55, -0.83)}, "Foot_R": {"d": (0.0, -0.3, -0.95)},
}
CURL_R = {"Index1_R": 30, "Index2_R": 40, "Middle1_R": 35, "Middle2_R": 45, "Ring1_R": 40, "Ring2_R": 45, "Pinky1_R": 45, "Pinky2_R": 45, "Thumb2_R": 8}

def merge(*ds):
    out = {}
    for d in ds:
        for k, v in d.items():
            out[k] = {**out.get(k, {}), **v}
    return out

def curl(dd, scale=1.0):
    return {k: {"local": [("X", -v * scale)]} for k, v in dd.items()}   # âm = gập về phía lòng bàn tay (trục Z của xương tay = mu bàn tay)

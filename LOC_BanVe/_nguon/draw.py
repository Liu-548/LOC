# -*- coding: utf-8 -*-
# Bản vẽ bố cục nhà LỘC — đọc data.json (trích từ LOC_NhaLoc.unity) → PDF nhiều trang
import json, os, math, collections, sys, datetime
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from reportlab.pdfgen import canvas
from reportlab.lib.units import mm
from reportlab.lib.colors import Color, HexColor, white, black
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from vi_names import vi

HOME = os.path.expanduser('~')
DATA = json.load(open(os.path.join(HOME, 'work', 'data.json')))
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HOME, 'mnt/GameKinhDi/LOC_BanVe/LOC_BanVe_BoCuc_NhaLoc.pdf')
ONLY = sys.argv[2].split(',') if len(sys.argv) > 2 else None   # chỉ vẽ một số trang (để thử)

FD = '/usr/share/fonts/truetype/dejavu/'
pdfmetrics.registerFont(TTFont('F', FD + 'DejaVuSansCondensed.ttf'))
pdfmetrics.registerFont(TTFont('FB', FD + 'DejaVuSansCondensed-Bold.ttf'))
pdfmetrics.registerFont(TTFont('FM', FD + 'DejaVuSansMono.ttf'))

A3W, A3H = 297 * mm, 420 * mm          # portrait
UNITS, STRUCT, DECALS, LIGHTS = DATA['units'], DATA['structure'], DATA['decals'], DATA['lights']

LV = {'H': dict(name='HẦM', base=-2.3, lab='Hầm (−2,30)'), 'T1': dict(name='TẦNG 1', base=0.0, lab='Tầng 1 (±0,00)'),
      'T2': dict(name='TẦNG 2', base=3.4, lab='Tầng 2 (+3,40)'), 'T3': dict(name='TẦNG 3', base=6.6, lab='Tầng 3 (+6,60)')}
ROOM = {  # mã → (tên, tầng, kích thước thông thuỷ ghi trong LocHouseBuilder)
    'ST': ('Sân trước + cổng + rạp tang', 'T1'), 'TB': ('Tiệm — khu bán hàng', 'T1'), 'TK': ('Tiệm — kho', 'T1'),
    'PK': ('Phòng khách + bàn thờ vong', 'T1'), 'SS': ('Sảnh sau + chân thang + cửa hầm', 'T1'), 'BA': ('Bếp + chỗ ăn', 'T1'),
    'GT': ('Giếng trời', 'T1'), 'KS': ('Khối sau: kho, WC, bể giặt', 'T1'), 'H': ('Hầm', 'H'),
    'BM': ('Phòng bố mẹ + WC + ban công', 'T2'), 'S2': ('Sảnh, thang, hành lang tầng 2', 'T2'), 'LV': ('Góc làm việc', 'T2'),
    'NH': ('Phòng Nhím', 'T2'), 'KH': ('Phòng Khôi', 'T2'),
    'SP': ('Sân phơi', 'T3'), 'PT': ('Phòng thờ gia tiên', 'T3'), 'GK': ('Sảnh + góc kho mở', 'T3')}

CAT = collections.OrderedDict([
    ('san', ('Đồ đặt trên sàn', '#3B6EA8', '#1F4E79', 0.30, None)),
    ('tren', ('Đồ đặt trên bàn / kệ / giường', '#1F9E89', '#0B6B5A', 0.45, None)),
    ('treo', ('Treo tường / treo trần', '#E08A1E', '#B5650A', 0.25, (2, 1.5))),
    ('dien', ('Điện – đèn – quạt', '#E6B800', '#9A7B00', 0.40, None)),
    ('cua', ('Cửa, cổng, cửa sổ, rèm', '#4E9A3B', '#2E6B1F', 0.40, None)),
    ('ketcau', ('Kết cấu phụ / cố định', '#8E5EC2', '#5A3A85', 0.28, None))])
NIGHTCOL = {1: '#D35400', 2: '#C2185B', 3: '#5E35B1'}


def hexc(h, a=1.0):
    c = HexColor(h)
    return Color(c.red, c.green, c.blue, alpha=a)


def fm(v, n=2):
    return f'{v:.{n}f}'.replace('.', ',').replace('-', '−')


def rot(x, z, yaw):
    t = math.radians(yaw); c, s = math.cos(t), math.sin(t)
    return x * c + z * s, -x * s + z * c


def category(u):
    n = u['name']; base = LV[u['level']]['base']
    if u['kieu'] == 3 or n.startswith(('Cua', 'Khung', 'Canh', 'SongSat', 'BauCuaSo', 'OThoang', 'Rem_', 'ThanhRem', 'CongSat', 'Kinh')):
        return 'cua'
    if n.startswith(('Den', 'Bong', 'Quat', 'OCam', 'BangDien', 'Chuong', 'HopCongTo', 'DongHoNuoc')) and 'Ban' not in n[:6]:
        return 'dien'
    if n.startswith(('LanCan', 'GoBanCong', 'GiengTroi', 'MuiBac', 'NenSan', 'TruDauThang', 'OngNuocMua', 'MayBom', 'TranGiat', 'HoaTran', 'TuongBua', 'Decal', 'BonNuocMai', 'CotPhoi', 'DayPhoi', 'KepPhoi', 'BienSoNha')):
        return 'ketcau'
    if u['kieu'] == 1 or u['y0'] >= base + 1.5:
        return 'treo'
    if u['y0'] >= base + 0.25:
        return 'tren'
    return 'san'


# ───────── chuẩn bị đơn vị (unit)
for u in UNITS:
    u['cat'] = category(u)
    a = u['yaw'] if u['yaw'] is not None else 0
    pts = [p for pt in u['parts'] for p in pt['poly']]
    cx, cz = u['center']
    loc = [rot(p[0] - cx, p[1] - cz, -a) for p in pts]
    u['w'] = max(p[0] for p in loc) - min(p[0] for p in loc)
    u['d'] = max(p[1] for p in loc) - min(p[1] for p in loc)
    u['h'] = u['y1'] - u['y0']
    u['area'] = u['w'] * u['d']
    u['label'] = vi(u['name'])
    u['nightlab'] = {1: 'Đ1', 2: 'Đ2', 3: 'Đ3'}.get(u['night'], '')
    if u['name'].endswith('_Dem1') or u['name'].endswith('_Dem2') or u['name'].endswith('_Dem3'):
        pass

ORDER = list(CAT.keys())
ROOMUNITS = collections.defaultdict(list)
for u in UNITS:
    ROOMUNITS[u['room']].append(u)
for r, lst in ROOMUNITS.items():
    lst.sort(key=lambda u: (ORDER.index(u['cat']), u['night'] or 0, round(u['center'][1], 1), u['center'][0]))
    for i, u in enumerate(lst, 1):
        u['code'] = f'{r}-{i:02d}'
        u['num'] = f'{i:02d}'

# ───────── phân loại kết cấu
def sclass(s):
    n = s['name']; p = s['path'].split('/'); top = p[1] if len(p) > 1 else ''
    sx, sy, sz = s['size']
    if top == 'KhuPho': return 'pho'
    if top == 'CongTac': return 'congtac'
    if n in ('TayVin',): return 'rail'
    if n in ('Song', 'ThanhTren', 'ThanhDuoi'): return 'railx'
    if '_Bac' in n and n.startswith('Thang'): return 'step'
    if n.startswith('Kinh'): return 'glass'
    if sy <= 0.35 and sx * sz > 1.0: return 'slab'
    if min(sx, sz) <= 0.55: return 'wall'
    return 'other'


for s in STRUCT:
    s['cls'] = sclass(s)
STR_BY = collections.defaultdict(list)
for s in STRUCT:
    STR_BY[s['cls']].append(s)

# cụm công tắc
SW = collections.OrderedDict()
for s in STR_BY['congtac']:
    g = s['path'].split('/')[2]
    SW.setdefault(g, []).append(s)
SWITCH = []
for g, v in SW.items():
    xs = [p[0] for s in v for p in s['poly']]; zs = [p[1] for s in v for p in s['poly']]
    x0, x1, z0, z1 = min(xs), max(xs), min(zs), max(zs)
    y0 = min(s['y0'] for s in v); y1 = max(s['y1'] for s in v)
    lvl = 'T3' if y0 > 6.5 else 'T2' if y0 > 3.3 else 'T1'
    SWITCH.append(dict(name=g, x0=x0, x1=x1, z0=z0, z1=z1, y0=y0, y1=y1, level=lvl, cx=(x0 + x1) / 2, cz=(z0 + z1) / 2,
                       axis='X' if (x1 - x0) < (z1 - z0) else 'Z', n=len(v)))
SWITCH.sort(key=lambda a: (a['level'], a['cz'], a['cx']))
for i, a in enumerate(SWITCH, 1):
    a['code'] = f'CT{i:02d}'

# đèn
LIT = []
for l in LIGHTS:
    if l['name'] in ('TrangDem', 'Den_Pho'): continue
    x, y, z = l['pos']
    lvl = 'H' if y < -0.3 else 'T1' if y < 3.3 else 'T2' if y < 6.5 else 'T3'
    if l['name'].startswith('Den_BongDayToc_Ham'): lvl = 'H'
    if l['name'] in ('Den_Rap_Trai', 'Den_Rap_Phai'): lvl = 'T1'
    if l['name'] in ('Den_NenDien_GiaTien', 'Den_PhongTho_1', 'Den_PhongTho_2', 'Den_Sanh_T3', 'Den_SanPhoi'): lvl = 'T3'
    LIT.append(dict(name=l['name'], x=x, y=y, z=z, level=lvl, color=l['color'], range=l['range'], inten=l['intensity']))
LIT.sort(key=lambda a: (a['level'], a['z'], a['x']))
for i, a in enumerate(LIT, 1):
    a['code'] = f'L{i:02d}'

# ───────── bố cục trang
PAGES = []            # danh sách (title, drawfn)
TOC = []


def halo_text(c, px, py, t, font, size, col, lw):
    c.saveState()
    tt = c.beginText(px, py); tt.setFont(font, size); tt.setTextRenderMode(1)
    c.setLineWidth(lw); c.setStrokeColor(white); tt.textOut(t); c.drawText(tt)
    c.restoreState()   # Tr 1 là trạng thái đồ hoạ, phải khôi phục trước khi tô chữ
    tt = c.beginText(px, py); tt.setFont(font, size)
    tt.setFillColor(hexc(col)); tt.textOut(t); c.drawText(tt)


class PV:
    def __init__(s, c, win, area, den=None):
        s.c = c; s.x0, s.z0, s.x1, s.z1 = win
        ax, ay, aw, ah = [v * mm for v in area]
        if den is None:
            for den in (10, 15, 20, 25, 30, 40, 50, 60, 75, 100, 125, 150, 200, 250):
                if (s.x1 - s.x0) * 1000 / den * mm <= aw and (s.z1 - s.z0) * 1000 / den * mm <= ah: break
        s.den = den; s.k = 1000.0 / den * mm
        ww, hh = (s.x1 - s.x0) * s.k, (s.z1 - s.z0) * s.k
        s.ox = ax + (aw - ww) / 2; s.oy = ay + (ah - hh) / 2; s.ww = ww; s.hh = hh

    def X(s, x): return s.ox + (x - s.x0) * s.k
    def Y(s, z): return s.oy + (z - s.z0) * s.k

    def clip(s):
        p = s.c.beginPath(); p.rect(s.ox, s.oy, s.ww, s.hh); s.c.clipPath(p, stroke=0, fill=0)

    def poly(s, pts, fill=None, stroke=None, lw=0.3, alpha=1.0, dash=None, close=True):
        c = s.c; p = c.beginPath()
        p.moveTo(s.X(pts[0][0]), s.Y(pts[0][1]))
        for q in pts[1:]: p.lineTo(s.X(q[0]), s.Y(q[1]))
        if close: p.close()
        if fill: c.setFillColor(hexc(fill, alpha))
        if stroke:
            c.setStrokeColor(hexc(stroke)); c.setLineWidth(lw)
        c.setDash(*(dash if dash else ((),)) if False else ()) if False else None
        c.setDash(list(dash) if dash else [])
        c.drawPath(p, stroke=1 if stroke else 0, fill=1 if fill else 0)
        c.setDash([])

    def line(s, a, b, col='#000000', lw=0.3, dash=None):
        c = s.c; c.setStrokeColor(hexc(col)); c.setLineWidth(lw); c.setDash(list(dash) if dash else [])
        c.line(s.X(a[0]), s.Y(a[1]), s.X(b[0]), s.Y(b[1])); c.setDash([])

    def text(s, x, z, t, size=5, font='F', col='#000000', anchor='c', halo=True, dx=0, dy=0):
        c = s.c; px, py = s.X(x) + dx, s.Y(z) + dy
        c.setFont(font, size)
        w = pdfmetrics.stringWidth(t, font, size)
        if anchor == 'c': px -= w / 2
        elif anchor == 'r': px -= w
        if halo: halo_text(c, px, py - size * 0.35, t, font, size, col, size * 0.3)
        else:
            tt = c.beginText(px, py - size * 0.35); tt.setFont(font, size); tt.setFillColor(hexc(col)); tt.textOut(t); c.drawText(tt)


def rect_pts(x0, z0, x1, z1):
    return [(x0, z0), (x1, z0), (x1, z1), (x0, z1)]


def draw_axes(pv, step=1.0, lab=True, fs=4):
    c = pv.c
    # lưới 1 m
    x = math.ceil(pv.x0 / step) * step
    while x <= pv.x1 + 1e-6:
        pv.line((x, pv.z0), (x, pv.z1), '#BBBBBB' if abs(x - round(x / 5) * 5) > 1e-6 else '#8F8F8F', 0.12 if abs(x - round(x / 5) * 5) > 1e-6 else 0.25)
        x += step
    z = math.ceil(pv.z0 / step) * step
    while z <= pv.z1 + 1e-6:
        pv.line((pv.x0, z), (pv.x1, z), '#BBBBBB' if abs(z - round(z / 5) * 5) > 1e-6 else '#8F8F8F', 0.12 if abs(z - round(z / 5) * 5) > 1e-6 else 0.25)
        z += step


def draw_frame(pv, step=1.0, fs=4.2):
    c = pv.c
    c.saveState()
    c.setStrokeColor(black); c.setLineWidth(0.6)
    c.rect(pv.ox, pv.oy, pv.ww, pv.hh, stroke=1, fill=0)
    c.setFont('F', fs); c.setFillColor(black)
    x = math.ceil(pv.x0 / step) * step
    while x <= pv.x1 + 1e-6:
        c.line(pv.X(x), pv.oy, pv.X(x), pv.oy - 1.2 * mm); c.line(pv.X(x), pv.oy + pv.hh, pv.X(x), pv.oy + pv.hh + 1.2 * mm)
        t = fm(x, 0) if abs(step - round(step)) < 1e-6 else fm(x, 1)
        c.drawCentredString(pv.X(x), pv.oy - 4.2 * mm, t); c.drawCentredString(pv.X(x), pv.oy + pv.hh + 2 * mm, t)
        x += step
    z = math.ceil(pv.z0 / step) * step
    while z <= pv.z1 + 1e-6:
        c.line(pv.ox, pv.Y(z), pv.ox - 1.2 * mm, pv.Y(z)); c.line(pv.ox + pv.ww, pv.Y(z), pv.ox + pv.ww + 1.2 * mm, pv.Y(z))
        t = fm(z, 0) if abs(step - round(step)) < 1e-6 else fm(z, 1)
        c.drawRightString(pv.ox - 1.6 * mm, pv.Y(z) - 1.3, t); c.drawString(pv.ox + pv.ww + 1.6 * mm, pv.Y(z) - 1.3, t)
        z += step
    # mũi tên hướng
    c.setFont('FB', 5.5)
    c.drawString(pv.ox, pv.oy - 8.5 * mm, 'X →  (đứng ngoài đường nhìn vào: 0 = mặt trong tường trái)')
    c.drawRightString(pv.ox + pv.ww, pv.oy - 8.5 * mm, '↓ phía ĐƯỜNG    ↑ vào trong nhà (Z)')
    c.restoreState()


def scalebar(pv, x_mm, y_mm):
    c = pv.c; c.saveState()
    L = 2.0 if pv.den >= 50 else 1.0
    w = L * pv.k
    c.setLineWidth(0.5); c.setStrokeColor(black); c.setFillColor(black)
    c.rect(x_mm * mm, y_mm * mm, w / 2, 1.4 * mm, stroke=1, fill=1)
    c.setFillColor(white); c.rect(x_mm * mm + w / 2, y_mm * mm, w / 2, 1.4 * mm, stroke=1, fill=1)
    c.setFillColor(black); c.setFont('F', 5)
    c.drawString(x_mm * mm, y_mm * mm + 2 * mm, '0'); c.drawString(x_mm * mm + w - 4, y_mm * mm + 2 * mm, f'{fm(L, 0)} m')
    c.drawString(x_mm * mm + w + 3 * mm, y_mm * mm + 0.2 * mm, f'Tỉ lệ 1:{pv.den}')
    c.restoreState()


def draw_structure(pv, lvl, site=False):
    c = pv.c; base = LV[lvl]['base']; hc = base + 1.5; hs = base + 0.5
    c.saveState(); pv.clip()
    # mặt đường + sàn
    if site:
        for s in STR_BY['other'] + STR_BY['slab']:
            if s['name'] == 'DuongPho':
                pv.poly(s['poly'], fill='#6E6E72', alpha=0.55)
    for s in STR_BY['slab']:
        top = s['y1']
        if base - 0.2 <= top <= base + 0.06 and s['name'] != 'DuongPho':
            pv.poly(s['poly'], fill='#E9E2D0' if lvl != 'H' else '#D9D2C2', alpha=1)
    # bậc thang
    for s in STR_BY['step']:
        n = s['name']
        if lvl == 'H' and n.startswith('ThangHam'): show, dash = True, None
        elif lvl == 'T1' and n.startswith('ThangHam'): show, dash = True, (1.2, 1)
        elif lvl == 'T1' and n.startswith('Thang_T1'): show, dash = True, None
        elif lvl == 'T2' and n.startswith('Thang_T2'): show, dash = True, None
        elif lvl == 'T2' and n.startswith('Thang_T1'): show, dash = False, None
        else: show, dash = False, None
        if show:
            above = (s['y1'] > hc) and not n.startswith('ThangHam')
            pv.poly(s['poly'], fill='#D8D8D8', stroke='#555555', lw=0.25, dash=(1.2, 1) if (above or dash) else None)
    # tường
    if site:
        for s in STR_BY['pho']:
            if s['y0'] < 3.0 and s['size'][1] > 0.3:
                pv.poly(s['poly'], fill='#C9C4BA', stroke='#8A857B', lw=0.2)
    for s in STR_BY['wall'] + STR_BY['other']:
        if s['name'] == 'DuongPho' or s['cls'] == 'other' and s['size'][1] < 0.1: continue
        if s['y0'] <= hc <= s['y1']:
            pv.poly(s['poly'], fill='#2B2B2B', stroke='#000000', lw=0.25)
        elif s['y0'] <= hs <= s['y1'] and s['y1'] <= hc and s['y1'] - s['y0'] > 0.05:
            pv.poly(s['poly'], fill='#9C9C9C', stroke='#555555', lw=0.2)
    for s in STR_BY['glass']:
        if s['y0'] <= hc <= s['y1'] or s['y0'] <= hs <= s['y1']:
            pv.poly(s['poly'], fill='#7FD1E8', stroke='#2A8FB0', lw=0.25)
    for s in STR_BY['rail']:
        if base <= s['y0'] <= base + 3.3:
            pv.poly(s['poly'], fill='#555555', stroke='#333333', lw=0.15)
    c.restoreState()


def label_rooms(pv, lvl):
    # tên phòng chữ lớn mờ ở giữa các vùng định nghĩa
    R = {'T1': [('TIỆM — BÁN HÀNG', -2.3, -1.6), ('TIỆM — KHO', -2.3, 4.6), ('SÂN TRƯỚC', 3.8, -2.9), ('PHÒNG KHÁCH', 4.2, 3.6), ('SẢNH SAU + THANG', 4.6, 9.3),
                ('BẾP + ĂN', 3.8, 13.7), ('GIẾNG TRỜI', 3.8, 17.8), ('KHO', 1.8, 22.0), ('WC', 6.1, 20.5), ('GIẶT', 5.7, 23.4)],
         'T2': [('PHÒNG BỐ MẸ', 3.0, 2.0), ('BAN CÔNG', 3.8, -0.7), ('SẢNH / THANG', 0.95, 6.6), ('GÓC LÀM VIỆC', 5.4, 6.8), ('PHÒNG NHÍM', 5.4, 10.4), ('PHÒNG KHÔI', 5.4, 14.4)],
         'T3': [('SÂN PHƠI', 3.8, 1.5), ('PHÒNG THỜ GIA TIÊN', 3.8, 4.7), ('SẢNH + GÓC KHO', 4.0, 8.6)], 'H': [('HẦM', 5.0, 10.5)]}
    for t, x, z in R.get(lvl, []):
        if not (pv.x0 < x < pv.x1 and pv.z0 < z < pv.z1): continue
        pv.text(x, z, t, size=7 if pv.den >= 60 else 9, font='FB', col='#9A9A9A', halo=False)


def draw_units(pv, units, labeled, fullcode=False, minarea=0.0, dim_ids=()):
    c = pv.c; c.saveState(); pv.clip()
    placed = []
    # vẽ: đồ nhỏ trên cùng
    for u in sorted(units, key=lambda u: -u['area']):
        name, stroke_col, stroke_a, dash = CAT[u['cat']][0], CAT[u['cat']][2], CAT[u['cat']][3], CAT[u['cat']][4]
        dim = u['id'] in dim_ids
        for p in u['parts']:
            if dim:
                pv.poly(p['poly'], fill='#BBBBBB', stroke='#999999', lw=0.15, alpha=0.35)
            else:
                pv.poly(p['poly'], fill=CAT[u['cat']][1], stroke=CAT[u['cat']][2], lw=0.25, alpha=CAT[u['cat']][3], dash=dash)
        if u['night'] and not dim:
            pts = [q for p in u['parts'] for q in p['poly']]
            x0, x1 = min(q[0] for q in pts), max(q[0] for q in pts); z0, z1 = min(q[1] for q in pts), max(q[1] for q in pts)
            pv.poly(rect_pts(x0, z0, x1, z1), stroke=NIGHTCOL[u['night']], lw=0.5, dash=(1.6, 1.2))
    # mũi tên hướng mặt trước
    for u in units:
        if u['id'] in dim_ids or u['area'] < 0.12 or u['cat'] in ('cua', 'dien', 'ketcau'): continue
        yaw = u['yaw'] or 0
        fx, fz = math.sin(math.radians(yaw)), math.cos(math.radians(yaw))
        cx, cz = u['center']
        half = u['d'] / 2
        # điểm giữa cạnh trước (theo hướng mặt trước)
        bx, bz = cx + fx * half, cz + fz * half
        L = min(0.12, max(0.05, u['d'] * 0.25))
        a = (bx - fx * L, bz - fz * L); tip = (bx, bz)
        nx, nz = -fz, fx
        pv.poly([tip, (a[0] + nx * L * 0.6, a[1] + nz * L * 0.6), (a[0] - nx * L * 0.6, a[1] - nz * L * 0.6)], fill='#222222', alpha=0.8)
    # nhãn
    lab = [u for u in units if u['id'] in labeled]
    lab.sort(key=lambda u: u['area'])
    fs = 4.3 if pv.den <= 50 else 3.8
    for u in lab:
        t = u['code'] if fullcode else u['num']
        if u['night']: t += '·' + u['nightlab']
        cx, cz = u['center']
        X0, Y0 = pv.X(cx), pv.Y(cz)
        w = pdfmetrics.stringWidth(t, 'FB', fs); h = fs * 0.9
        best = None
        for r in (0, 2.2, 3.4, 4.6, 6.0, 7.6, 9.4):
            for ang in range(0, 360, 30) if r else (0,):
                px = X0 + r * mm * math.cos(math.radians(ang)); py = Y0 + r * mm * math.sin(math.radians(ang))
                rc = (px - w / 2 - 0.4, py - h / 2 - 0.3, px + w / 2 + 0.4, py + h / 2 + 0.3)
                if rc[0] < pv.ox or rc[2] > pv.ox + pv.ww or rc[1] < pv.oy or rc[3] > pv.oy + pv.hh: continue
                if any(not (rc[2] < q[0] or rc[0] > q[2] or rc[3] < q[1] or rc[1] > q[3]) for q in placed): continue
                best = (px, py, r, rc); break
            if best: break
        if not best:
            px, py = X0, Y0; best = (px, py, 0, (px - w / 2, py - h / 2, px + w / 2, py + h / 2))
        px, py, r, rc = best
        placed.append(rc)
        if r > 0:
            c.setStrokeColor(hexc('#555555')); c.setLineWidth(0.15); c.line(X0, Y0, px, py)
            c.setFillColor(hexc('#555555')); c.circle(X0, Y0, 0.35, stroke=0, fill=1)
        halo_text(c, px - w / 2, py - fs * 0.33, t, 'FB', fs, NIGHTCOL[u['night']] if u['night'] else '#111111', fs * 0.32)
    c.restoreState()


def draw_switches_lights(pv, lvl, rooms=None, tag=True):
    c = pv.c; c.saveState(); pv.clip()
    for a in SWITCH:
        if a['level'] != lvl: continue
        if rooms and not (pv.x0 - 0.5 <= a['cx'] <= pv.x1 + 0.5 and pv.z0 - 0.5 <= a['cz'] <= pv.z1 + 0.5): continue
        pv.poly(rect_pts(a['cx'] - 0.06, a['cz'] - 0.06, a['cx'] + 0.06, a['cz'] + 0.06), fill='#FFFFFF', stroke='#C0392B', lw=0.5)
        pv.line((a['cx'] - 0.06, a['cz'] - 0.06), (a['cx'] + 0.06, a['cz'] + 0.06), '#C0392B', 0.35)
        pv.line((a['cx'] - 0.06, a['cz'] + 0.06), (a['cx'] + 0.06, a['cz'] - 0.06), '#C0392B', 0.35)
        if tag: pv.text(a['cx'], a['cz'], a['code'], size=3.6, font='FB', col='#C0392B', dy=2.4)
    for a in LIT:
        if a['level'] != lvl: continue
        if not (pv.x0 - 0.5 <= a['x'] <= pv.x1 + 0.5 and pv.z0 - 0.5 <= a['z'] <= pv.z1 + 0.5): continue
        X, Y = pv.X(a['x']), pv.Y(a['z'])
        c.setFillColor(hexc('#FFE066', 0.9)); c.setStrokeColor(hexc('#B8860B')); c.setLineWidth(0.35)
        c.circle(X, Y, 0.9 * mm, stroke=1, fill=1)
        for k in range(8):
            ang = k * math.pi / 4
            c.line(X + 1.1 * mm * math.cos(ang), Y + 1.1 * mm * math.sin(ang), X + 1.7 * mm * math.cos(ang), Y + 1.7 * mm * math.sin(ang))
        if tag:
            halo_text(c, X + 2 * mm, Y - 1.2, a['code'], 'FB', 3.6, '#8A6500', 1.0)
    c.restoreState()


def header(c, title, sub, pageno, total, portrait=False, tocid=None):
    W = A3W if portrait else A3H; H = A3H if portrait else A3W
    c.saveState()
    c.setFillColor(hexc('#1E2A3A')); c.rect(0, H - 13 * mm, W, 13 * mm, stroke=0, fill=1)
    c.setFillColor(white); c.setFont('FB', 11); c.drawString(10 * mm, H - 8.6 * mm, 'LỘC — BẢN VẼ BỐ CỤC NHÀ')
    c.setFont('FB', 11); c.drawString(78 * mm, H - 8.6 * mm, title)
    c.setFont('F', 7.5); c.drawRightString(W - 10 * mm, H - 8.8 * mm, f'Trang {pageno}/{total}')
    if sub:
        c.setFillColor(hexc('#333333')); c.setFont('F', 7.2); c.drawString(10 * mm, H - 18 * mm, sub)
    c.setFillColor(hexc('#555555')); c.setFont('F', 5.3)
    c.drawString(10 * mm, 4.5 * mm, 'Nguồn: scene Assets/Scenes/LOC_NhaLoc.unity (dựng 30/09/2026 17:09) — toạ độ Unity thế giới, đơn vị mét. X: ngang · Z: chiều sâu · Y: cao độ (sàn T1 = 0).')
    c.drawRightString(W - 10 * mm, 4.5 * mm, 'Mã vật = mã phòng + số thứ tự (PK-07…). Đ1/Đ2/Đ3 = vật chỉ có ở Đêm 1/2/3. Kích thước = khung bao của mesh thật (.glb).')
    c.restoreState()


def legend(c, x_mm, y_mm, w_mm=120):
    # chú giải nhỏ (ngang)
    c.saveState(); x = x_mm * mm; y = y_mm * mm
    c.setFont('FB', 6); c.setFillColor(black); c.drawString(x, y + 10 * mm, 'CHÚ GIẢI')
    cx = x; cy = y + 5 * mm
    items = [(k, v) for k, v in CAT.items()]
    for i, (k, v) in enumerate(items):
        xx = x + (i % 3) * 42 * mm; yy = y + 5 * mm - (i // 3) * 4.8 * mm
        c.setFillColor(hexc(v[1], v[3] + 0.15)); c.setStrokeColor(hexc(v[2])); c.setLineWidth(0.5)
        if v[4]: c.setDash(list(v[4]))
        c.rect(xx, yy - 0.3 * mm, 6 * mm, 3 * mm, stroke=1, fill=1); c.setDash([])
        c.setFillColor(black); c.setFont('F', 5.6); c.drawString(xx + 7.5 * mm, yy + 0.7 * mm, v[0])
    yy = y - 5.5 * mm
    c.setFillColor(hexc('#2B2B2B')); c.rect(x, yy, 6 * mm, 3 * mm, stroke=0, fill=1); c.setFillColor(black); c.drawString(x + 7.5 * mm, yy + 0.7 * mm, 'Tường (cắt ở 1,5 m)')
    c.setFillColor(hexc('#9C9C9C')); c.rect(x + 42 * mm, yy, 6 * mm, 3 * mm, stroke=0, fill=1); c.setFillColor(black); c.drawString(x + 49.5 * mm, yy + 0.7 * mm, 'Bậu / tường thấp')
    c.setFillColor(hexc('#7FD1E8')); c.rect(x + 84 * mm, yy, 6 * mm, 3 * mm, stroke=0, fill=1); c.setFillColor(black); c.drawString(x + 91.5 * mm, yy + 0.7 * mm, 'Kính')
    yy -= 4.8 * mm
    c.setStrokeColor(hexc('#C0392B')); c.setLineWidth(0.5); c.rect(x + 1 * mm, yy, 3 * mm, 3 * mm, stroke=1, fill=0); c.line(x + 1 * mm, yy, x + 4 * mm, yy + 3 * mm); c.line(x + 1 * mm, yy + 3 * mm, x + 4 * mm, yy)
    c.setFillColor(black); c.drawString(x + 7.5 * mm, yy + 0.7 * mm, 'Công tắc / ổ cắm (CT..)')
    c.setFillColor(hexc('#FFE066')); c.setStrokeColor(hexc('#B8860B')); c.circle(x + 45 * mm, yy + 1.5 * mm, 1 * mm, stroke=1, fill=1)
    c.setFillColor(black); c.drawString(x + 49.5 * mm, yy + 0.7 * mm, 'Nguồn sáng (L..)')
    c.setFillColor(black); c.setFont('F', 5.6); c.drawString(x + 84 * mm, yy + 0.7 * mm, '▲ mặt trước của vật')
    yy -= 4.8 * mm
    for i, (n, col) in enumerate(((1, NIGHTCOL[1]), (2, NIGHTCOL[2]), (3, NIGHTCOL[3]))):
        c.setStrokeColor(hexc(col)); c.setLineWidth(0.6); c.setDash([1.6, 1.2]); c.rect(x + i * 42 * mm + 1 * mm, yy, 5 * mm, 3 * mm, stroke=1, fill=0); c.setDash([])
        c.setFillColor(black); c.drawString(x + i * 42 * mm + 7.5 * mm, yy + 0.7 * mm, f'Đ{n}: chỉ có ở Đêm {n}')
    c.restoreState()


# ───────── bảng
ROWH = 7.0 * mm


def fmt_row(u):
    hd = {0: 'trong', 180: 'ra đường', 90: '→ phải', -90: '← trái', -180: 'ra đường', 270: '← trái'}
    yaw = u['yaw'] or 0
    yy = round(yaw)
    hdn = hd.get(yy, f'{yy}°')
    return dict(code=u['code'], name=u['label'], un=u['name'], night=u['nightlab'], x=fm(u['center'][0]), z=fm(u['center'][1]),
                size=f"{fm(u['w'])}×{fm(u['d'])}×{fm(u['h'])}", y=f"{fm(u['y0'])} → {fm(u['y1'])}", hd=hdn, cat=u['cat'])


def table(c, x_mm, y_top_mm, w_mm, rows, start, nmax, heads=True):
    """vẽ rows[start:start+nmax]; trả về chỉ số kế tiếp"""
    x = x_mm * mm; y = y_top_mm * mm; w = w_mm * mm
    cols = [('Mã', 13), ('Tên (tên trong Unity)', 0), ('Đêm', 7), ('Tâm X', 10.5), ('Tâm Z', 10.5), ('Rộng×Sâu×Cao (m)', 27), ('Y đáy → đỉnh', 22), ('Hướng', 13)]
    fixed = sum(cw for _, cw in cols) * mm
    cols[1] = (cols[1][0], (w - fixed) / mm)
    xs = [x]
    for _, cw in cols: xs.append(xs[-1] + cw * mm)
    c.saveState()
    if heads:
        c.setFillColor(hexc('#1E2A3A')); c.rect(x, y - 6 * mm, w, 6 * mm, stroke=0, fill=1)
        c.setFillColor(white); c.setFont('FB', 5.6)
        for i, (t, _) in enumerate(cols):
            if i in (3, 4): c.drawRightString(xs[i + 1] - 1 * mm, y - 4 * mm, t)
            else: c.drawString(xs[i] + 1 * mm, y - 4 * mm, t)
        y -= 6 * mm
    i = start; n = 0; lastcat = rows[start - 1]['cat'] if start > 0 else None
    while i < len(rows) and n < nmax:
        r = rows[i]
        if r['cat'] != lastcat:
            if n + 1 >= nmax and n > 0: break
            c.setFillColor(hexc(CAT[r['cat']][1], 0.25)); c.rect(x, y - 4.6 * mm, w, 4.6 * mm, stroke=0, fill=1)
            c.setFillColor(hexc(CAT[r['cat']][2])); c.setFont('FB', 5.8); c.drawString(x + 1 * mm, y - 3.2 * mm, CAT[r['cat']][0].upper())
            y -= 4.6 * mm; lastcat = r['cat']; n += 0.65
        if n >= nmax: break
        if int(n) % 2 == 0:
            c.setFillColor(hexc('#F4F6F8')); c.rect(x, y - ROWH, w, ROWH, stroke=0, fill=1)
        c.setFillColor(hexc(CAT[r['cat']][2])); c.setFont('FB', 5.8); c.drawString(xs[0] + 1 * mm, y - 3.4 * mm, r['code'])
        c.setFillColor(black); c.setFont('F', 5.8)
        nm = r['name']
        while pdfmetrics.stringWidth(nm, 'F', 5.8) > (xs[2] - xs[1] - 1.5 * mm) and len(nm) > 4: nm = nm[:-2]
        c.drawString(xs[1] + 1 * mm, y - 3.0 * mm, nm if nm == r['name'] else nm + '…')
        c.setFillColor(hexc('#777777')); c.setFont('FM', 4.2); c.drawString(xs[1] + 1 * mm, y - 5.9 * mm, r['un'][:60])
        if r['night']:
            c.setFillColor(hexc(NIGHTCOL[int(r['night'][1])])); c.setFont('FB', 5.8); c.drawString(xs[2] + 1 * mm, y - 3.4 * mm, r['night'])
        c.setFillColor(black); c.setFont('F', 5.8)
        c.drawRightString(xs[4] - 1 * mm, y - 3.4 * mm, r['x']); c.drawRightString(xs[5] - 1 * mm, y - 3.4 * mm, r['z'])
        c.drawString(xs[5] + 1 * mm, y - 3.4 * mm, r['size']); c.drawString(xs[6] + 1 * mm, y - 3.4 * mm, r['y']); c.drawString(xs[7] + 1 * mm, y - 3.4 * mm, r['hd'])
        c.setStrokeColor(hexc('#DDDDDD')); c.setLineWidth(0.2); c.line(x, y - ROWH, x + w, y - ROWH)
        y -= ROWH; i += 1; n += 1
    c.restoreState()
    return i


# ───────── định nghĩa trang phòng
RP = [  # (khoá, tiêu đề, tầng, cửa sổ nhìn (x0,z0,x1,z1), các mã phòng đánh số)
    ('ST', 'Sân trước — cổng, rạp tang, bàn phúng viếng (Đêm 1 / 2 / 3)', 'T1', (-0.5, -5.9, 8.3, 0.4), ['ST']),
    ('TIEM', 'Tiệm vật liệu — khu bán hàng + kho', 'T1', (-4.7, -5.8, 0.15, 7.15), ['TB', 'TK']),
    ('PK', 'Phòng khách + bàn thờ vong', 'T1', (-0.35, -0.4, 8.0, 7.6), ['PK']),
    ('SS', 'Sảnh sau + chân cầu thang + cửa hầm', 'T1', (-0.35, 7.3, 8.0, 11.1), ['SS']),
    ('BA', 'Bếp + chỗ ăn', 'T1', (-0.35, 10.9, 8.0, 16.6), ['BA']),
    ('KS', 'Giếng trời + khối sau (kho, WC, bể giặt)', 'T1', (-0.35, 16.3, 8.0, 24.95), ['GT', 'KS']),
    ('H', 'Hầm', 'H', (0.6, 5.5, 8.0, 14.0), ['H']),
    ('BM', 'Phòng bố mẹ + WC khép kín + ban công', 'T2', (-0.4, -1.45, 8.0, 5.65), ['BM']),
    ('S2', 'Sảnh, cầu thang, hành lang tầng 2', 'T2', (-0.35, 5.3, 3.4, 16.7), ['S2']),
    ('LV', 'Góc làm việc (tầng 2)', 'T2', (2.95, 5.3, 7.95, 8.65), ['LV']),
    ('NH', 'Phòng Nhím', 'T2', (2.95, 8.3, 7.95, 12.6), ['NH']),
    ('KH', 'Phòng Khôi', 'T2', (2.95, 12.3, 7.95, 16.6), ['KH']),
    ('SPT', 'Sân phơi + phòng thờ gia tiên (tầng 3)', 'T3', (-0.4, -0.4, 8.0, 6.65), ['SP', 'PT']),
    ('GK', 'Sảnh + góc kho mở (tầng 3)', 'T3', (-0.4, 6.3, 8.0, 11.0), ['GK']),
]


def room_rows(codes):
    lst = []
    for r in codes: lst += ROOMUNITS.get(r, [])
    lst.sort(key=lambda u: (ORDER.index(u['cat']), [codes.index(u['room'])][0], u['night'] or 0, round(u['center'][1], 1), u['center'][0]))
    return lst


def build_pages():
    pages = []
    # 0 bìa
    pages.append(dict(kind='cover', title='Bìa + mục lục', portrait=True))
    pages.append(dict(kind='numbers', title='Số chốt: kích thước, cao độ, phòng', portrait=True))
    pages.append(dict(kind='site', title='Mặt bằng tổng thể — phố, sân trước, tiệm, nhà (tầng 1)', portrait=True))
    for lvl, title in (('T1', 'Mặt bằng tầng 1 — toàn bộ (tiệm + sân trước + nhà)'), ('H', 'Mặt bằng hầm'), ('T2', 'Mặt bằng tầng 2 — toàn bộ'), ('T3', 'Mặt bằng tầng 3 — toàn bộ')):
        pages.append(dict(kind='floor', lvl=lvl, title=title, portrait=True))
    for key, title, lvl, win, codes in RP:
        rows = room_rows(codes)
        first = 36
        pages.append(dict(kind='room', key=key, title=title, lvl=lvl, win=win, codes=codes, rows=rows, start=0, nmax=first, portrait=False))
        done = first;
        # số hàng thực tế nhiều hơn do hàng tiêu đề nhóm: tính lại khi vẽ → dùng vòng tới khi hết
        pages[-1]['cont'] = True
    return pages


# ───────── vẽ từng loại trang
def page_room(c, pg, pageno, total, cache):
    key, lvl, win, codes, rows = pg['key'], pg['lvl'], pg['win'], pg['codes'], pg['rows']
    header(c, pg['title'], f"{LV[lvl]['lab']} · {len(rows)} vật trong {', '.join(codes)}", pageno, total)
    H = A3W
    pv = PV(c, win, (16, 48, 250, 220))
    draw_axes(pv)
    draw_structure(pv, lvl)
    label_rooms(pv, lvl)
    codeset = set(codes)
    all_in = [u for u in UNITS if u['level'] == lvl and not (u['bbox'][2] < win[0] or u['bbox'][0] > win[2] or u['bbox'][3] < win[1] or u['bbox'][1] > win[3])]
    mine = [u for u in all_in if u['room'] in codeset]
    others = [u for u in all_in if u['room'] not in codeset]
    dim_ids = {u['id'] for u in others}
    draw_units(pv, others + mine, {u['id'] for u in mine}, fullcode=len(codes) > 1, dim_ids=dim_ids)
    draw_switches_lights(pv, lvl, rooms=True)
    draw_frame(pv)
    scalebar(pv, 16, 22)
    legend(c, 100, 28)
    # bảng
    y_top = H - 16 * mm
    nxt = table(c, 272, (H - 16 * mm) / mm, 138, [fmt_row(u) for u in rows], 0, 35)
    cache[key] = nxt
    return nxt


def page_table_cont(c, pg, pageno, total, start):
    rows = [fmt_row(u) for u in pg['rows']]
    H = A3W
    header(c, pg['title'] + ' — bảng vật (tiếp)', f"Vật thứ {start + 1} → {len(rows)} / {len(rows)}", pageno, total)
    n1 = table(c, 10, (H - 16) / mm * mm / mm if False else (H / mm - 18), 190, rows, start, 34)
    if n1 < len(rows):
        table(c, 215, (H / mm - 18), 190, rows, n1, 34)
    return


def plan_page(c, pg, pageno, total):
    lvl = pg['lvl']
    if pg['kind'] == 'site':
        header(c, pg['title'], 'Mặt bằng tầng 1 kèm khu phố xung quanh · chỉ vẽ kết cấu, đường, nhà hàng xóm và khung chiếm chỗ của vật trong nhà', pageno, total, portrait=True)
        win = (-13.5, -13.6, 17.0, 26.0)
        pv = PV(c, win, (18, 34, 262, 345), den=None)
        draw_axes(pv, step=2.0)
        draw_structure(pv, 'T1', site=True)
        draw_units(pv, [u for u in UNITS if u['level'] == 'T1'], set(), dim_ids=())
        for l in LIGHTS:
            if l['name'] == 'Den_Pho':
                X, Y = pv.X(l['pos'][0]), pv.Y(l['pos'][2])
                if pv.ox <= X <= pv.ox + pv.ww and pv.oy <= Y <= pv.oy + pv.hh:
                    c.setFillColor(hexc('#FFB060')); c.circle(X, Y, 1.0 * mm, stroke=0, fill=1)
        pv.text(0.0 + 3.8, -9.0, 'ĐƯỜNG PHỐ', size=10, font='FB', col='#FFFFFF', halo=False)
        pv.text(-2.3, -2.6, 'TIỆM', size=8, font='FB', col='#666666', halo=False); pv.text(3.8, -3.0, 'SÂN TRƯỚC', size=8, font='FB', col='#666666', halo=False)
        pv.text(3.8, 12.0, 'NHÀ LỘC', size=12, font='FB', col='#888888', halo=False)
        draw_frame(pv, step=2.0)
        scalebar(pv, 18, 18)
        return
    if pg['kind'] == 'floor':
        wins = {'T1': (-5.0, -6.2, 8.5, 25.4), 'H': (0.0, 4.8, 8.4, 14.6), 'T2': (-0.7, -1.7, 8.3, 17.2), 'T3': (-0.7, -0.7, 8.3, 11.4)}
        win = wins[lvl]
        sub = f"{LV[lvl]['lab']} · mỗi vật vẽ bằng khung bao thật; nhãn = mã vật (tra bảng ở các trang phòng) — chỉ gắn nhãn cho vật ≥ 0,25 m²"
        header(c, pg['title'], sub, pageno, total, portrait=True)
        pv = PV(c, win, (18, 50, 262, 330))
        draw_axes(pv); draw_structure(pv, lvl); label_rooms(pv, lvl)
        us = [u for u in UNITS if u['level'] == lvl]
        lab = {u['id'] for u in us if u['area'] >= 0.25 and u['cat'] in ('san', 'tren', 'cua')}
        draw_units(pv, us, lab, fullcode=True)
        draw_switches_lights(pv, lvl, rooms=True, tag=True)
        draw_frame(pv); scalebar(pv, 18, 34); legend(c, 110, 29)
        return


def page_cover(c, pg, pageno, total, toc):
    W, H = A3W, A3H
    header(c, 'Bìa & mục lục', '', pageno, total, portrait=True)
    c.saveState()
    c.setFillColor(hexc('#1E2A3A')); c.setFont('FB', 34); c.drawString(22 * mm, H - 55 * mm, 'LỘC')
    c.setFont('FB', 16); c.drawString(22 * mm, H - 66 * mm, 'Bản vẽ chi tiết bố cục từng vật trong nhà')
    c.setFont('F', 9); c.setFillColor(hexc('#444444'))
    lines = ['Dự án game kinh dị 3D — môn Phát triển game nâng cao · căn nhà phố 8 × 25 m + tiệm tách riêng bên trái.',
             f'Toàn bộ số liệu đọc trực tiếp từ scene Assets/Scenes/LOC_NhaLoc.unity (bản dựng 30/09/2026 17:09, sau rà soát chạm mặt/chồng lấn).',
             f'Gồm {len(UNITS)} vật (đã tính cả biến thể Đêm 1/2/3), {len(SWITCH)} cụm công tắc/ổ cắm, {len(LIT)} nguồn sáng, {len(DECALS)} decal, hơn {len(STRUCT)} khối kết cấu.',
             'Mỗi vật có: mã, tên Việt + tên Unity, toạ độ tâm (X, Z), khung bao Rộng×Sâu×Cao, cao độ đáy → đỉnh (Y), hướng mặt trước.',
             'Toạ độ dán thẳng được vào Unity: X = ngang (0 = mặt trong tường trái, đứng ngoài đường nhìn vào) · Z = chiều sâu (0 = mặt trong tường mặt tiền) · Y = cao độ (sàn tầng 1 = 0).']
    y = H - 82 * mm
    for ln in lines:
        c.drawString(22 * mm, y, ln); y -= 5.6 * mm
    c.setFillColor(hexc('#1E2A3A')); c.setFont('FB', 11); c.drawString(22 * mm, y - 6 * mm, 'MỤC LỤC')
    y -= 14 * mm
    c.setFont('F', 9.5)
    for (t, pn) in toc:
        c.setFillColor(black); c.drawString(26 * mm, y, t)
        c.setFillColor(hexc('#666666')); c.drawRightString(W - 26 * mm, y, str(pn))
        c.setStrokeColor(hexc('#CCCCCC')); c.setLineWidth(0.2); c.setDash([0.6, 1.2]);
        tw = pdfmetrics.stringWidth(t, 'F', 9.5)
        c.line(26 * mm + tw + 2 * mm, y + 0.8, W - 26 * mm - 8 * mm, y + 0.8); c.setDash([])
        y -= 6.0 * mm
    c.setFillColor(black)
    legend(c, 22, 35)
    c.restoreState()


def page_numbers(c, pg, pageno, total):
    W, H = A3W, A3H
    header(c, pg['title'], 'Các số này là số thật trong scene — đối chiếu được với LocHouseBuilder.cs', pageno, total, portrait=True)
    c.saveState()
    y = H - 30 * mm
    def sect(t):
        nonlocal y
        c.setFillColor(hexc('#1E2A3A')); c.setFont('FB', 10); c.drawString(14 * mm, y, t); y -= 6 * mm
    def rowt(a, b, bold=False):
        nonlocal y
        c.setFillColor(black); c.setFont('FB' if bold else 'F', 8); c.drawString(16 * mm, y, a); c.setFont('F', 8); c.drawString(92 * mm, y, b); y -= 4.6 * mm
    sect('Cao độ (Y, mét)')
    for a, b in (('Sàn hầm', '−2,30 · trần hầm 2,10 (mặt dưới sàn T1 ở −0,20)'), ('Sân trước / hiên', '−0,15 (một bậc 0,15 lên sàn tiệm & nhà)'), ('Sàn tầng 1', '±0,00 · trần 3,20 · sàn dày 0,20'),
                 ('Sàn tầng 2', '+3,40 · trần 3,00'), ('Sàn tầng 3', '+6,60 · trần 3,00'), ('Mặt mái', '+9,80 (giếng trời 1,90 × … trên ô thang, lợp tấm nhựa)'), ('Tầm mắt người chơi', '1,65 m (cao 1,65 m)')):
        rowt(a, b)
    y -= 3 * mm
    sect('Vỏ nhà')
    for a, b in (('Bề ngang thông thuỷ nhà', '7,60 m (X 0 → 7,60); tường bao 0,20 hai bên'), ('Chiều sâu nhà (khối xây)', 'Z −0,20 → 24,80 (≈ 25 m); khối sau kết thúc ở Z 24,60'),
                 ('Sân trước', 'X −0,20 → 7,80 · Z −5,40 → −0,20; cổng sắt ở Z −5,25; cao độ −0,15'), ('Tiệm (tách riêng, bên trái)', 'X −4,40 → −0,20 · Z −5,40 → 7,00; vách kho ở Z 1,80–1,90; mái tôn +3,60'),
                 ('Tường ngăn trong', '0,10 m (T2 và T3); tường bao T1 0,20'), ('Cửa sắt xếp mặt tiền tiệm', 'lỗ 3,60 × 2,40 (X −4,10 → −0,50)')):
        rowt(a, b)
    y -= 3 * mm
    sect('Phòng theo từng tầng (khoảng Z, theo LocHouseBuilder.cs)')
    T = [('Tầng 1 — Phòng khách', 'Z 0 → 7,40 · 7,60 × 7,40'), ('Tầng 1 — Sảnh sau + chân thang', 'Z 7,50 → 10,90 · thang chữ U sát tường trái (X 0 → 1,90)'), ('Tầng 1 — Bếp + ăn', 'Z 11,00 → 16,40 · 7,60 × 5,40'),
         ('Tầng 1 — Giếng trời', 'Z 16,50 → 19,10 · lộ trời, sàn −0,05'), ('Tầng 1 — Khối sau', 'Z 19,20 → 24,60 · kho (X 0 → 3,60), WC (X 4,70 → 7,60 · Z 19,20 → 21,80), bể giặt (Z 21,90 → 24,60)'),
         ('Tầng 2 — Ban công', 'Z −1,20 → −0,20 (nhô 1,0 m), lan can 1,0–1,1 m'), ('Tầng 2 — Phòng bố mẹ', 'Z 0 → 5,40 · 7,60 × 5,40 · WC khép kín góc phải (X 5,80 → 7,60 · Z 3,60 → 5,40)'),
         ('Tầng 2 — Hành lang + thang', 'X 1,90 → 3,10 hành lang; thang X 0 → 1,90; Z 5,50 → 16,40'), ('Tầng 2 — Góc làm việc', 'X 3,20 → 7,60 · Z 5,50 → 8,40 (ô mở không cánh Z 6,00 → 7,20)'),
         ('Tầng 2 — Phòng Nhím', 'X 3,20 → 7,60 · Z 8,50 → 12,40 · 4,40 × 3,90 · cửa Z 8,95 → 9,85'), ('Tầng 2 — Phòng Khôi', 'X 3,20 → 7,60 · Z 12,50 → 16,40 · 4,40 × 3,90 · cửa Z 13,40 → 14,20'),
         ('Tầng 3 — Sân phơi', 'Z 0 → 3,00 · 7,60 × 3,00 (cửa ra sân phơi Z 3,00)'), ('Tầng 3 — Phòng thờ', 'Z 3,10 → 6,40 · 7,60 × 3,30'), ('Tầng 3 — Sảnh + góc kho mở', 'Z 6,50 → 10,80 (ô thang X 1,90 → 7,60; góc kho không cửa)')]
    for a, b in T: rowt(a, b)
    y -= 3 * mm
    sect('Cầu thang (bậc lấy thẳng từ scene)')
    for a, b in (('Thang chính T1 → T2', 'vế 1: X 1,00 → 1,90 từ Z 7,60, 10 bậc cao 0,17 · chiếu nghỉ +1,70 (Z 9,85 → 10,85) · vế 2: X 0 → 0,90 đi ra Z 9,85'),
                 ('Thang chính T2 → T3', 'vế 1 từ Z 7,60, 9 bậc cao 0,178 · chiếu nghỉ +1,60 so với sàn T2 · vế 2 đi ra Z 9,60'), ('Bậc thang', 'mặt bậc sâu 0,25 · rộng vế 0,90 · bậc granito'),
                 ('Thang hầm', 'dưới vế 2 của thang chính; dốc hơn thang chính (0,19 × 0,24) — cố ý, xem kịch bản')):
        rowt(a, b)
    y -= 3 * mm
    sect('Bảng số vật theo phòng')
    cnt = collections.Counter(u['room'] for u in UNITS)
    x = 16 * mm; yy = y
    c.setFont('F', 8)
    i = 0
    for r, (nm, lv) in ROOM.items():
        col = i % 2; row = i // 2
        c.setFillColor(black); c.drawString(16 * mm + col * 130 * mm, y - row * 4.6 * mm, f'{r} — {nm}')
        c.drawRightString(16 * mm + col * 130 * mm + 112 * mm, y - row * 4.6 * mm, f'{cnt.get(r, 0)} vật')
        i += 1
    c.restoreState()


# ───────── phụ lục
def page_appendix(c, pg, pageno, total, kind, start=0):
    H = A3W
    if kind == 'switch':
        header(c, 'Phụ lục A — công tắc & ổ cắm (14 cụm)', 'Mỗi cụm = mặt nhựa + phím + đèn neon + biểu tượng (LocCongTac) · tâm cụm đo ở mặt tường', pageno, total)
        heads = ['Mã', 'Cụm (tên trong scene)', 'Tầng', 'Tâm X', 'Tâm Z', 'Y đáy → đỉnh', 'Gắn trên tường', 'Số khối']
        cw = [14, 70, 26, 22, 22, 40, 70, 18]
        rows = []
        for a in SWITCH:
            rows.append([a['code'], a['name'], LV[a['level']]['lab'], fm(a['cx']), fm(a['cz']), f"{fm(a['y0'])} → {fm(a['y1'])}",
                         'vuông góc trục X (tường trái/phải)' if a['axis'] == 'X' else 'vuông góc trục Z (tường trước/sau)', str(a['n'])])
        simple_table(c, 12, H / mm - 22, heads, cw, rows, [3, 4])
        y0 = 175
        c.setFont('F', 7.5); c.setFillColor(black)
        c.drawString(12 * mm, y0 * mm, 'Ghi chú: toàn bộ công tắc đặt ở tâm cao 1,45 m (1,33 → 1,57) — vừa tầm mắt 1,65 m, theo yêu cầu "công tắc phải rõ ràng, vừa tầm mắt" (29/9). CongTac_Ham ở tường phải hầm (X 7,56–7,60).')
    elif kind == 'light':
        header(c, 'Phụ lục B — nguồn sáng (L01…)', 'Đèn điểm Unity (Point Light) · màu, tầm chiếu và cường độ như trong scene (chưa tính ánh sáng bake)', pageno, total)
        heads = ['Mã', 'Tên trong scene', 'Tầng', 'X', 'Z', 'Y (cao)', 'Màu', 'Tầm (m)', 'Cường độ']
        cw = [14, 76, 28, 20, 20, 20, 24, 24, 24]
        rows = []
        for a in LIT:
            col = '#%02X%02X%02X' % tuple(int(round(v * 255)) for v in (a['color'] or (1, 1, 1)))
            rows.append([a['code'], a['name'], LV[a['level']]['lab'], fm(a['x']), fm(a['z']), fm(a['y']), col, fm(a['range'], 1), fm(a['inten'], 2)])
        simple_table(c, 12, H / mm - 22, heads, cw, rows, [3, 4, 5, 7, 8])
    elif kind == 'decal':
        header(c, 'Phụ lục C — decal dán bề mặt (vết ẩm, dấu tay, chữ sơn…)', f'{len(DECALS)} decal · toạ độ tâm · kích thước w×h (mét) · "Đỉnh" = cao độ tâm', pageno, total)
        heads = ['STT', 'Tên decal', 'Tầng', 'Tâm X', 'Tâm Z', 'Cao độ Y', 'Rộng × Cao (m)', 'Nhóm trong scene']
        cw = [10, 66, 26, 20, 20, 20, 30, 120]
        rows = []
        for i, s in enumerate(sorted(DECALS, key=lambda s: (s['pos'][1] > 6.5, s['pos'][1] > 3.3, s['pos'][1] < -0.3, s['pos'][2], s['pos'][0])), 1):
            y = s['pos'][1]; lvl = 'H' if y < -0.3 else 'T1' if y < 3.3 else 'T2' if y < 6.5 else 'T3'
            sx, sy, sz = s['size']
            rows.append([str(i), s['name'].replace('Decal_Decal_', '').replace('Decal_', ''), LV[lvl]['lab'], fm(s['pos'][0]), fm(s['pos'][2]), fm(y), f"{fm(max(sx, sz) if sz > 0.05 else sx)} × {fm(sy if sz <= 0.05 else sz)}", '/'.join(s['path'].split('/')[1:-1])])
        simple_table(c, 12, H / mm - 22, heads, cw, rows, [3, 4, 5], maxrows=start and 34 or 34, start=start)


def simple_table(c, x_mm, y_mm, heads, cw, rows, right=(), maxrows=34, start=0):
    c.saveState()
    x = x_mm * mm; y = y_mm * mm
    xs = [x]
    for w in cw: xs.append(xs[-1] + w * mm)
    tw = xs[-1] - x
    c.setFillColor(hexc('#1E2A3A')); c.rect(x, y - 6 * mm, tw, 6 * mm, stroke=0, fill=1)
    c.setFillColor(white); c.setFont('FB', 6.5)
    for i, h in enumerate(heads):
        if i in right: c.drawRightString(xs[i + 1] - 1.5 * mm, y - 4.1 * mm, h)
        else: c.drawString(xs[i] + 1.5 * mm, y - 4.1 * mm, h)
    y -= 6 * mm
    for ri, r in enumerate(rows[start:start + maxrows]):
        if ri % 2 == 0: c.setFillColor(hexc('#F4F6F8')); c.rect(x, y - 5.4 * mm, tw, 5.4 * mm, stroke=0, fill=1)
        c.setFillColor(black); c.setFont('F', 6.6)
        for i, v in enumerate(r):
            if i in right: c.drawRightString(xs[i + 1] - 1.5 * mm, y - 3.8 * mm, v)
            else: c.drawString(xs[i] + 1.5 * mm, y - 3.8 * mm, v)
        y -= 5.4 * mm
    c.restoreState()


# ───────── chạy
def main():
    # kế hoạch trang
    plan = []   # (title, kind, args)
    plan.append(('cover', 'Bìa & mục lục', {}))
    plan.append(('numbers', 'Số chốt: cao độ, vỏ nhà, phòng, thang', {}))
    plan.append(('site', 'Mặt bằng tổng thể (phố + sân + tiệm + nhà, tầng 1)', {}))
    for lvl, t in (('T1', 'Mặt bằng tầng 1 — toàn bộ'), ('H', 'Mặt bằng hầm'), ('T2', 'Mặt bằng tầng 2 — toàn bộ'), ('T3', 'Mặt bằng tầng 3 — toàn bộ')):
        plan.append(('floor', t, dict(lvl=lvl)))
    for key, title, lvl, win, codes in RP:
        rows = room_rows(codes)
        plan.append(('room', title, dict(key=key, lvl=lvl, win=win, codes=codes, rows=rows, portrait=False)))
        # số hàng đầu tiên 35; phần còn lại 2 cột × 34
        rem_start = None
        plan.append(('roomcont', title, dict(key=key, rows=rows, codes=codes, _cont=True)))
    plan.append(('app_switch', 'Phụ lục A — công tắc & ổ cắm', {}))
    plan.append(('app_light', 'Phụ lục B — nguồn sáng', {}))
    nd = len(DECALS);
    for i in range(0, nd, 34):
        plan.append(('app_decal', 'Phụ lục C — decal bề mặt' + (' (tiếp)' if i else ''), dict(start=i)))
    return plan


def run():
    plan = main()
    # tính số trang room: roomcont có thể không cần
    # 1) tính trước số hàng mỗi trang phòng
    pages = []
    for kind, title, a in plan:
        if kind == 'roomcont':
            rows = [fmt_row(u) for u in a['rows']]
            # đếm số hàng đã dùng ở trang đầu
            used = simulate_rows(rows, 0, 35)
            if used >= len(rows): continue
            pos = used
            while pos < len(rows):
                pages.append(('roomcont', title, dict(a, start=pos)))
                pos = simulate_rows(rows, pos, 34)
                if pos < len(rows):
                    pos = simulate_rows(rows, pos, 34)
            continue
        pages.append((kind, title, a))
    total = len(pages)
    # TOC
    toc = []; seen = set()
    for i, (kind, title, a) in enumerate(pages, 1):
        if kind in ('roomcont',): continue
        if kind == 'app_decal' and a.get('start', 0) > 0: continue
        toc.append((title, i))
    c = canvas.Canvas(OUT, pagesize=(A3W, A3H))
    c.setTitle('LỘC — Bản vẽ bố cục nhà'); c.setAuthor('Claude (Cowork)')
    cache = {}
    for i, (kind, title, a) in enumerate(pages, 1):
        if ONLY and not (kind in ONLY or str(i) in ONLY or (kind == 'room' and a.get('key') in ONLY)):
            continue
        portrait = kind in ('cover', 'numbers', 'site', 'floor')
        c.setPageSize((A3W, A3H) if portrait else (A3H, A3W))
        if kind == 'cover': page_cover(c, a, i, total, toc)
        elif kind == 'numbers': page_numbers(c, dict(title=title), i, total)
        elif kind in ('site', 'floor'): plan_page(c, dict(kind=kind, lvl=a.get('lvl'), title=title), i, total)
        elif kind == 'room': page_room(c, dict(title=title, **a), i, total, cache)
        elif kind == 'roomcont': page_table_cont(c, dict(title=title, rows=a['rows']), i, total, a['start'])
        elif kind == 'app_switch': page_appendix(c, None, i, total, 'switch')
        elif kind == 'app_light': page_appendix(c, None, i, total, 'light')
        elif kind == 'app_decal': page_appendix(c, None, i, total, 'decal', a.get('start', 0))
        c.showPage()
    c.save()
    print('pages', total, '->', OUT)


def simulate_rows(rows, start, nmax):
    i = start; n = 0; lastcat = rows[start - 1]['cat'] if start > 0 else None
    while i < len(rows) and n < nmax:
        r = rows[i]
        if r['cat'] != lastcat:
            if n + 1 >= nmax and n > 0: break
            lastcat = r['cat']; n += 0.65
        if n >= nmax: break
        i += 1; n += 1
    return i


if __name__ == '__main__':
    run()

# Sinh bản .md từ cùng dữ liệu với draw.py (import draw → không vẽ PDF vì main có guard)
import sys, os
sys.argv = ['x']
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import draw as D
mm = D.mm

class Fake:                       # bắt các lệnh drawString của page_numbers để lấy lại bảng số chốt
    def __init__(s): s.t = []
    def __getattr__(s, k):
        if k == 'drawString': return lambda x, y, t: s.t.append((round(x / mm), t))
        return lambda *a, **k: None
f = Fake(); D.page_numbers(f, dict(title='x'), 2, 30)
num, cur = [], None
for x, t in f.t:
    if x == 14: cur = [t, []]; num.append(cur)
    elif x == 16 and cur and not t.startswith(('ST ', 'TB ')) and ' — ' in t and cur[0].startswith('Bảng'): pass
    elif x == 16 and cur and not cur[0].startswith('Bảng'): cur[1].append([t, ''])
    elif x == 92 and cur and cur[1]: cur[1][-1][1] = t
esc = lambda s: str(s).replace('|', '\\|')
def tab(heads, rows):
    o = ['| ' + ' | '.join(heads) + ' |', '|' + '|'.join('---' for _ in heads) + '|']
    o += ['| ' + ' | '.join(esc(c) for c in r) + ' |' for r in rows]
    return '\n'.join(o) + '\n'

L = ['# LỘC — Bản vẽ chi tiết bố cục từng vật trong nhà (bản chữ)', '',
     f'Nguồn: scene `Assets/Scenes/LOC_NhaLoc.unity` (bản dựng 30/09/2026 17:09). Gồm **{len(D.UNITS)} vật** (đã tính cả biến thể Đêm 1/2/3), {len(D.SWITCH)} cụm công tắc/ổ cắm, {len(D.LIT)} nguồn sáng, {len(D.DECALS)} decal. Bản này là phiên bản chữ của file `LOC_BanVe_BoCuc_NhaLoc.pdf`; **mặt bằng vẽ xem trong PDF**, số liệu ở đây khớp từng dòng với bảng trong PDF.', '',
     '## Cách đọc toạ độ', '',
     '- **X** = chiều ngang: 0 = mặt trong tường trái (đứng ngoài đường nhìn vào), tăng sang phải.',
     '- **Z** = chiều sâu: 0 = mặt trong tường trước, tăng vào trong nhà; âm = phía đường (sân trước, tiệm).',
     '- **Y** = cao độ tuyệt đối (sàn tầng 1 = 0). Hầm −2,30 · sân trước −0,15 · T2 +3,40 · T3 +6,60.',
     '- **Tâm X / Tâm Z** = tâm khung bao của vật; **Rộng×Sâu×Cao** = kích thước khung bao thật của mesh (mét, theo trục riêng của vật); **Y đáy → đỉnh** = cao độ chân và đỉnh vật.',
     '- **Hướng** = hướng mặt trước của vật: `trong` = quay vào nhà, `ra đường` = quay ra phố, `→ phải` / `← trái` = nhìn từ ngoài đường vào; số độ = góc xoay khác.',
     '- **Đêm**: Đ1/Đ2/Đ3 = vật chỉ có ở Đêm 1/2/3; để trống = có mọi đêm.', '',
     '## Số chốt', '']
for h, rows in num:
    L += [f'### {h}', '', tab(['Hạng mục', 'Giá trị'], rows)]
import collections
cnt = collections.Counter(u['room'] for u in D.UNITS)
L += ['### Bảng số vật theo phòng', '', tab(['Mã', 'Phòng', 'Số vật'], [[r, n, cnt.get(r, 0)] for r, (n, lv) in D.ROOM.items()])]
L += ['## Mục lục phòng', '']
L += [f'- [{t}](#{k.lower()})  — {D.LV[lvl]["lab"]}' for k, t, lvl, win, codes in D.RP]
L += ['']
for k, t, lvl, win, codes in D.RP:
    rows = D.room_rows(codes)
    L += [f'## {t}', '', f'<a id="{k.lower()}"></a>{D.LV[lvl]["lab"]} · phòng {", ".join(codes)} · {len(rows)} vật', '']
    for cat in D.ORDER:
        rs = [D.fmt_row(u) for u in rows if u['cat'] == cat]
        if not rs: continue
        L += [f'### {D.CAT[cat][0]} ({len(rs)})', '',
              tab(['Mã', 'Tên', 'Tên Unity', 'Đêm', 'Tâm X', 'Tâm Z', 'Rộng×Sâu×Cao', 'Y đáy → đỉnh', 'Hướng'],
                  [[r['code'], r['name'], '`' + r['un'] + '`', r['night'] or '', r['x'], r['z'], r['size'], r['y'], r['hd']] for r in rs])]
fm = D.fm
L += ['## Phụ lục A — công tắc & ổ cắm', '', 'Mỗi cụm = mặt nhựa + phím + đèn neon + biểu tượng (LocCongTac). Tất cả đặt ở tâm cao 1,45 m (1,33 → 1,57).', '',
      tab(['Mã', 'Cụm', 'Tầng', 'Tâm X', 'Tâm Z', 'Y đáy → đỉnh', 'Gắn trên tường', 'Số khối'],
          [[a['code'], a['name'], D.LV[a['level']]['lab'], fm(a['cx']), fm(a['cz']), f"{fm(a['y0'])} → {fm(a['y1'])}", 'vuông góc trục X (tường trái/phải)' if a['axis'] == 'X' else 'vuông góc trục Z (tường trước/sau)', a['n']] for a in D.SWITCH])]
L += ['## Phụ lục B — nguồn sáng', '', 'Point Light của Unity, giá trị như trong scene (chưa tính ánh sáng bake).', '',
      tab(['Mã', 'Tên', 'Tầng', 'X', 'Z', 'Y', 'Màu', 'Tầm (m)', 'Cường độ'],
          [[a['code'], a['name'], D.LV[a['level']]['lab'], fm(a['x']), fm(a['z']), fm(a['y']), '#%02X%02X%02X' % tuple(int(round(v * 255)) for v in (a['color'] or (1, 1, 1))), fm(a['range'], 1), fm(a['inten'], 2)] for a in D.LIT])]
rows = []
for i, s in enumerate(sorted(D.DECALS, key=lambda s: (s['pos'][1] > 6.5, s['pos'][1] > 3.3, s['pos'][1] < -0.3, s['pos'][2], s['pos'][0])), 1):
    y = s['pos'][1]; lvl = 'H' if y < -0.3 else 'T1' if y < 3.3 else 'T2' if y < 6.5 else 'T3'
    sx, sy, sz = s['size']
    rows.append([i, s['name'].replace('Decal_Decal_', '').replace('Decal_', ''), D.LV[lvl]['lab'], fm(s['pos'][0]), fm(s['pos'][2]), fm(y),
                 f"{fm(max(sx, sz) if sz > 0.05 else sx)} × {fm(sy if sz <= 0.05 else sz)}", '/'.join(s['path'].split('/')[1:-1])])
L += ['## Phụ lục C — decal dán bề mặt', '', tab(['STT', 'Tên decal', 'Tầng', 'Tâm X', 'Tâm Z', 'Cao độ Y', 'Rộng × Cao (m)', 'Nhóm trong scene'], rows)]
out = os.path.expanduser('~/mnt/GameKinhDi/LOC_BanVe/LOC_BanVe_BoCuc_NhaLoc.md')
open(out, 'w', encoding='utf-8').write('\n'.join(L))
print(len(L), 'dòng ->', out)

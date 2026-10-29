# Ảnh hoa văn cho món có họa tiết in/dệt (nhúng vào .glb). Tạo một lần, lưu img/<tên>.png
import os, math, random
from PIL import Image, ImageDraw, ImageFilter
D = os.path.join(os.path.dirname(__file__), "img"); os.makedirs(D, exist_ok=True)

def _hex(h): h = h.lstrip('#'); return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))

def vai_hoa(ten="vai_hoa_dogach", nen="#9E5A45", hoa=("#C98C6A", "#B87A5C"), la="#6F7A5A", n=512, so=9, seed=7):
    p = f"{D}/{ten}.png"
    if os.path.exists(p): return p
    r = random.Random(seed); im = Image.new("RGB", (n, n), _hex(nen)); d = ImageDraw.Draw(im)
    for i in range(so):
        for j in range(so):
            cx = (i + 0.5 + (j % 2) * 0.5) * n / so + r.uniform(-6, 6); cy = (j + 0.5) * n / so + r.uniform(-6, 6)
            for dx in (-n, 0, n):
                for dy in (-n, 0, n):
                    x, y = cx + dx, cy + dy
                    for k in range(2):
                        a = r.uniform(0, 6.28); lx, ly = x + 14 * math.cos(a), y + 14 * math.sin(a)
                        d.ellipse((lx - 9, ly - 4, lx + 9, ly + 4), fill=_hex(la))
                    c = _hex(hoa[(i + j) % len(hoa)])
                    for k in range(5):
                        a = k * 1.2566 + i; px, py = x + 7 * math.cos(a), y + 7 * math.sin(a)
                        d.ellipse((px - 6, py - 6, px + 6, py + 6), fill=c)
                    d.ellipse((x - 3, y - 3, x + 3, y + 3), fill=(110, 60, 40))
    # sợi dệt + bạc màu
    px = im.load()
    for yy in range(n):
        for xx in range(n):
            t = 1 + 0.06 * ((xx % 3 == 0) - (yy % 3 == 0)) + r.uniform(-0.03, 0.03)
            q = px[xx, yy]; px[xx, yy] = tuple(max(0, min(255, int(c * t))) for c in q)
    im.save(p); return p

def ren(ten="ren_trang", mau="#E4DFD0", n=512):
    """Khăn ren móc: RGBA, lỗ trong suốt (alpha cutout)."""
    p = f"{D}/{ten}.png"
    if os.path.exists(p): return p
    im = Image.new("RGBA", (n, n), (0, 0, 0, 0)); d = ImageDraw.Draw(im); c = _hex(mau) + (255,)
    s = n // 8
    for i in range(9):
        for j in range(9):
            x, y = i * s, j * s
            d.ellipse((x - s * 0.42, y - s * 0.42, x + s * 0.42, y + s * 0.42), outline=c, width=5)
            d.ellipse((x - s * 0.15, y - s * 0.15, x + s * 0.15, y + s * 0.15), fill=c)
            for k in range(8):
                a = k * math.pi / 4; d.line((x + s * 0.15 * math.cos(a), y + s * 0.15 * math.sin(a), x + s * 0.42 * math.cos(a), y + s * 0.42 * math.sin(a)), fill=c, width=3)
            d.line((x + s * 0.42, y, x + s * 0.58, y), fill=c, width=4); d.line((x, y + s * 0.42, x, y + s * 0.58), fill=c, width=4)
    im.save(p); return p

def ke_o(ten="nilon_ke_o", a="#B8483A", b="#EDE8DC", n=512, o=8):
    p = f"{D}/{ten}.png"
    if os.path.exists(p): return p
    A, B = _hex(a), _hex(b); im = Image.new("RGB", (n, n)); px = im.load(); s = n // o
    for y in range(n):
        for x in range(n):
            u, v = (x // s) % 2, (y // s) % 2
            if u and v: c = A
            elif u or v: c = tuple((A[k] + B[k]) // 2 for k in range(3))
            else: c = B
            px[x, y] = c
    im.save(p); return p

def luoi(ten="luoi_loa", nen="#2A2A28", n=256, buoc=4):
    p = f"{D}/{ten}.png"
    if os.path.exists(p): return p
    im = Image.new("RGB", (n, n), _hex(nen)); d = ImageDraw.Draw(im)
    for i in range(0, n, buoc):
        d.line((i, 0, i, n), fill=(58, 58, 55)); d.line((0, i, n, i), fill=(50, 50, 48))
    im.save(p); return p

def chu(ten, dong, w=512, h=256, nen="#F1ECDD", mau="#222222", co=40, canh="giua", font=None):
    """Nhãn/biển chữ Quốc ngữ đơn giản."""
    from PIL import ImageFont
    p = f"{D}/{ten}.png"
    if os.path.exists(p): return p
    im = Image.new("RGB", (w, h), _hex(nen)); d = ImageDraw.Draw(im)
    f = None
    for fp in ["/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"]:
        if os.path.exists(fp): f = fp; break
    y = (h - len(dong) * co * 1.25) / 2
    for line in dong:
        sz = co if not isinstance(line, tuple) else line[1]; txt = line if not isinstance(line, tuple) else line[0]
        ft = ImageFont.truetype(f, sz) if f else ImageFont.load_default()
        tw = d.textlength(txt, font=ft)
        d.text(((w - tw) / 2 if canh == "giua" else 20, y), txt, font=ft, fill=_hex(mau)); y += sz * 1.25
    im.save(p); return p

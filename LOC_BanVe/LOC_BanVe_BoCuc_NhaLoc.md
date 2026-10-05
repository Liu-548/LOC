# LỘC — Bản vẽ chi tiết bố cục từng vật trong nhà (bản chữ)

Nguồn: scene `Assets/Scenes/LOC_NhaLoc.unity` (bản dựng 30/09/2026 17:09). Gồm **522 vật** (đã tính cả biến thể Đêm 1/2/3), 14 cụm công tắc/ổ cắm, 29 nguồn sáng, 64 decal. Bản này là phiên bản chữ của file `LOC_BanVe_BoCuc_NhaLoc.pdf`; **mặt bằng vẽ xem trong PDF**, số liệu ở đây khớp từng dòng với bảng trong PDF.

## Cách đọc toạ độ

- **X** = chiều ngang: 0 = mặt trong tường trái (đứng ngoài đường nhìn vào), tăng sang phải.
- **Z** = chiều sâu: 0 = mặt trong tường trước, tăng vào trong nhà; âm = phía đường (sân trước, tiệm).
- **Y** = cao độ tuyệt đối (sàn tầng 1 = 0). Hầm −2,30 · sân trước −0,15 · T2 +3,40 · T3 +6,60.
- **Tâm X / Tâm Z** = tâm khung bao của vật; **Rộng×Sâu×Cao** = kích thước khung bao thật của mesh (mét, theo trục riêng của vật); **Y đáy → đỉnh** = cao độ chân và đỉnh vật.
- **Hướng** = hướng mặt trước của vật: `trong` = quay vào nhà, `ra đường` = quay ra phố, `→ phải` / `← trái` = nhìn từ ngoài đường vào; số độ = góc xoay khác.
- **Đêm**: Đ1/Đ2/Đ3 = vật chỉ có ở Đêm 1/2/3; để trống = có mọi đêm.

## Số chốt

### Cao độ (Y, mét)

| Hạng mục | Giá trị |
|---|---|
| Sàn hầm | −2,30 · trần hầm 2,10 (mặt dưới sàn T1 ở −0,20) |
| Sân trước / hiên | −0,15 (một bậc 0,15 lên sàn tiệm & nhà) |
| Sàn tầng 1 | ±0,00 · trần 3,20 · sàn dày 0,20 |
| Sàn tầng 2 | +3,40 · trần 3,00 |
| Sàn tầng 3 | +6,60 · trần 3,00 |
| Mặt mái | +9,80 (giếng trời 1,90 × … trên ô thang, lợp tấm nhựa) |
| Tầm mắt người chơi | 1,65 m (cao 1,65 m) |

### Vỏ nhà

| Hạng mục | Giá trị |
|---|---|
| Bề ngang thông thuỷ nhà | 7,60 m (X 0 → 7,60); tường bao 0,20 hai bên |
| Chiều sâu nhà (khối xây) | Z −0,20 → 24,80 (≈ 25 m); khối sau kết thúc ở Z 24,60 |
| Sân trước | X −0,20 → 7,80 · Z −5,40 → −0,20; cổng sắt ở Z −5,25; cao độ −0,15 |
| Tiệm (tách riêng, bên trái) | X −4,40 → −0,20 · Z −5,40 → 7,00; vách kho ở Z 1,80–1,90; mái tôn +3,60 |
| Tường ngăn trong | 0,10 m (T2 và T3); tường bao T1 0,20 |
| Cửa sắt xếp mặt tiền tiệm | lỗ 3,60 × 2,40 (X −4,10 → −0,50) |

### Phòng theo từng tầng (khoảng Z, theo LocHouseBuilder.cs)

| Hạng mục | Giá trị |
|---|---|
| Tầng 1 — Phòng khách | Z 0 → 7,40 · 7,60 × 7,40 |
| Tầng 1 — Sảnh sau + chân thang | Z 7,50 → 10,90 · thang chữ U sát tường trái (X 0 → 1,90) |
| Tầng 1 — Bếp + ăn | Z 11,00 → 16,40 · 7,60 × 5,40 |
| Tầng 1 — Giếng trời | Z 16,50 → 19,10 · lộ trời, sàn −0,05 |
| Tầng 1 — Khối sau | Z 19,20 → 24,60 · kho (X 0 → 3,60), WC (X 4,70 → 7,60 · Z 19,20 → 21,80), bể giặt (Z 21,90 → 24,60) |
| Tầng 2 — Ban công | Z −1,20 → −0,20 (nhô 1,0 m), lan can 1,0–1,1 m |
| Tầng 2 — Phòng bố mẹ | Z 0 → 5,40 · 7,60 × 5,40 · WC khép kín góc phải (X 5,80 → 7,60 · Z 3,60 → 5,40) |
| Tầng 2 — Hành lang + thang | X 1,90 → 3,10 hành lang; thang X 0 → 1,90; Z 5,50 → 16,40 |
| Tầng 2 — Góc làm việc | X 3,20 → 7,60 · Z 5,50 → 8,40 (ô mở không cánh Z 6,00 → 7,20) |
| Tầng 2 — Phòng Nhím | X 3,20 → 7,60 · Z 8,50 → 12,40 · 4,40 × 3,90 · cửa Z 8,95 → 9,85 |
| Tầng 2 — Phòng Khôi | X 3,20 → 7,60 · Z 12,50 → 16,40 · 4,40 × 3,90 · cửa Z 13,40 → 14,20 |
| Tầng 3 — Sân phơi | Z 0 → 3,00 · 7,60 × 3,00 (cửa ra sân phơi Z 3,00) |
| Tầng 3 — Phòng thờ | Z 3,10 → 6,40 · 7,60 × 3,30 |
| Tầng 3 — Sảnh + góc kho mở | Z 6,50 → 10,80 (ô thang X 1,90 → 7,60; góc kho không cửa) |

### Cầu thang (bậc lấy thẳng từ scene)

| Hạng mục | Giá trị |
|---|---|
| Thang chính T1 → T2 | vế 1: X 1,00 → 1,90 từ Z 7,60, 10 bậc cao 0,17 · chiếu nghỉ +1,70 (Z 9,85 → 10,85) · vế 2: X 0 → 0,90 đi ra Z 9,85 |
| Thang chính T2 → T3 | vế 1 từ Z 7,60, 9 bậc cao 0,178 · chiếu nghỉ +1,60 so với sàn T2 · vế 2 đi ra Z 9,60 |
| Bậc thang | mặt bậc sâu 0,25 · rộng vế 0,90 · bậc granito |
| Thang hầm | dưới vế 2 của thang chính; dốc hơn thang chính (0,19 × 0,24) — cố ý, xem kịch bản |

### Bảng số vật theo phòng

| Hạng mục | Giá trị |
|---|---|

### Bảng số vật theo phòng

| Mã | Phòng | Số vật |
|---|---|---|
| ST | Sân trước + cổng + rạp tang | 79 |
| TB | Tiệm — khu bán hàng | 16 |
| TK | Tiệm — kho | 16 |
| PK | Phòng khách + bàn thờ vong | 68 |
| SS | Sảnh sau + chân thang + cửa hầm | 27 |
| BA | Bếp + chỗ ăn | 56 |
| GT | Giếng trời | 5 |
| KS | Khối sau: kho, WC, bể giặt | 25 |
| H | Hầm | 8 |
| BM | Phòng bố mẹ + WC + ban công | 70 |
| S2 | Sảnh, thang, hành lang tầng 2 | 32 |
| LV | Góc làm việc | 9 |
| NH | Phòng Nhím | 22 |
| KH | Phòng Khôi | 25 |
| SP | Sân phơi | 16 |
| PT | Phòng thờ gia tiên | 41 |
| GK | Sảnh + góc kho mở | 7 |

## Mục lục phòng

- [Sân trước — cổng, rạp tang, bàn phúng viếng (Đêm 1 / 2 / 3)](#st)  — Tầng 1 (±0,00)
- [Tiệm vật liệu — khu bán hàng + kho](#tiem)  — Tầng 1 (±0,00)
- [Phòng khách + bàn thờ vong](#pk)  — Tầng 1 (±0,00)
- [Sảnh sau + chân cầu thang + cửa hầm](#ss)  — Tầng 1 (±0,00)
- [Bếp + chỗ ăn](#ba)  — Tầng 1 (±0,00)
- [Giếng trời + khối sau (kho, WC, bể giặt)](#ks)  — Tầng 1 (±0,00)
- [Hầm](#h)  — Hầm (−2,30)
- [Phòng bố mẹ + WC khép kín + ban công](#bm)  — Tầng 2 (+3,40)
- [Sảnh, cầu thang, hành lang tầng 2](#s2)  — Tầng 2 (+3,40)
- [Góc làm việc (tầng 2)](#lv)  — Tầng 2 (+3,40)
- [Phòng Nhím](#nh)  — Tầng 2 (+3,40)
- [Phòng Khôi](#kh)  — Tầng 2 (+3,40)
- [Sân phơi + phòng thờ gia tiên (tầng 3)](#spt)  — Tầng 3 (+6,60)
- [Sảnh + góc kho mở (tầng 3)](#gk)  — Tầng 3 (+6,60)

## Sân trước — cổng, rạp tang, bàn phúng viếng (Đêm 1 / 2 / 3)

<a id="st"></a>Tầng 1 (±0,00) · phòng ST · 79 vật

### Đồ đặt trên sàn (57)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| ST-01 | Bảng cáo phó | `SM_ObituaryBoard` |  | 5,95 | −5,62 | 0,67×0,16×0,83 | −0,15 → 0,68 | ra đường |
| ST-02 | Chồng 8 ghế nhựa | `SM_PlasticChairStack_8` |  | 0,25 | −4,87 | 0,39×0,45×1,24 | −0,15 → 1,09 | trong |
| ST-03 | Cổng rạp tang | `SM_FuneralGate` |  | 3,80 | −4,93 | 3,28×0,17×2,42 | −0,15 → 2,27 | ra đường |
| ST-04 | Chồng 4 ghế nhựa | `SM_PlasticChairStack_4` |  | 6,65 | −4,87 | 0,39×0,43×0,99 | −0,15 → 0,84 | trong |
| ST-05 | Ghế nhựa xanh | `SM_PlasticChair_Green` |  | 6,09 | −4,27 | 0,39×0,43×0,81 | −0,15 → 0,66 | -80° |
| ST-06 | Bàn nhựa trải khăn (nhận phúng viếng) | `SM_PlasticTable_TableCover` |  | 5,42 | −3,98 | 0,83×0,83×0,72 | −0,15 → 0,57 | trong |
| ST-07 | Ghế nhựa đỏ | `SM_PlasticChair_Red` |  | 6,07 | −3,75 | 0,39×0,43×0,81 | −0,15 → 0,66 | ← trái |
| ST-08 | Rạp tang (khung bạt) | `SM_FuneralTent_300` |  | 3,80 | −3,42 | 4,56×4,55×2,93 | −0,16 → 2,77 | ra đường |
| ST-09 | Thùng nước ngọt phục vụ khách | `ThungNuocNgot_Khach` |  | 7,40 | −1,30 | 0,42×0,30×0,34 | −0,15 → 0,19 | trong |
| ST-10 | Thùng rác nhựa + bao đen | `ThungRacNhua` |  | 7,45 | −0,70 | 0,52×0,56×0,94 | −0,15 → 0,79 | trong |
| ST-11 | Vòng hoa (tươi) | `VongHoa_Tuoi` | Đ1 | 0,25 | −5,66 | 1,10×0,51×1,35 | −0,15 → 1,20 | 179° |
| ST-12 | Vòng hoa (tươi) | `VongHoa_Tuoi` | Đ1 | 1,50 | −5,66 | 1,10×0,51×1,35 | −0,15 → 1,20 | -179° |
| ST-13 | Vòng hoa (tươi) | `VongHoa_Tuoi` | Đ1 | 7,25 | −5,66 | 1,10×0,51×1,35 | −0,15 → 1,20 | ra đường |
| ST-14 | Chậu đốt vàng mã (đang cháy) | `SM_BurnBasin_Burning` | Đ1 | 7,30 | −4,85 | 0,50×0,50×0,39 | 0,18 → 0,57 | trong |
| ST-15 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ1 | 1,25 | −4,10 | 0,39×0,43×0,81 | −0,15 → 0,66 | -4° |
| ST-16 | Ghế nhựa xanh | `SM_PlasticChair_Green` | Đ1 | 0,67 | −3,68 | 0,39×0,43×0,81 | −0,15 → 0,66 | 89° |
| ST-17 | Ghế nhựa xanh | `SM_PlasticChair_Green` | Đ1 | 2,00 | −3,59 | 0,39×0,43×0,81 | −0,15 → 0,66 | -92° |
| ST-18 | Bàn nhựa trải khăn (nhận phúng viếng) | `SM_PlasticTable_TableCover` | Đ1 | 1,31 | −3,43 | 0,83×0,83×0,72 | −0,15 → 0,57 | -7° |
| ST-19 | Ghế nhựa xanh | `SM_PlasticChair_Green` | Đ1 | 7,44 | −3,23 | 0,39×0,43×0,81 | −0,15 → 0,66 | -88° |
| ST-20 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ1 | 6,11 | −3,13 | 0,39×0,43×0,81 | −0,15 → 0,66 | -99° |
| ST-21 | Bàn nhựa trải khăn (nhận phúng viếng) | `SM_PlasticTable_TableCover` | Đ1 | 6,74 | −2,95 | 0,83×0,83×0,72 | −0,15 → 0,57 | -8° |
| ST-22 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ1 | 2,60 | −2,82 | 0,39×0,43×0,81 | −0,15 → 0,66 | -54° |
| ST-23 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ1 | 5,45 | −2,76 | 0,39×0,43×0,81 | −0,15 → 0,66 | 72° |
| ST-24 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ1 | 0,92 | −2,75 | 0,39×0,43×0,81 | −0,15 → 0,66 | 138° |
| ST-25 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ1 | 1,42 | −2,69 | 0,39×0,43×0,81 | −0,15 → 0,66 | -136° |
| ST-26 | Ghế nhựa xanh | `SM_PlasticChair_Green` | Đ1 | 6,54 | −2,32 | 0,39×0,43×0,81 | −0,15 → 0,66 | -92° |
| ST-27 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ1 | 7,18 | −2,25 | 0,39×0,43×0,81 | −0,15 → 0,66 | -138° |
| ST-28 | Ghế nhựa xanh | `SM_PlasticChair_Green` | Đ1 | 1,57 | −2,14 | 0,39×0,43×0,81 | −0,15 → 0,66 | 63° |
| ST-29 | Bàn nhựa trải khăn (nhận phúng viếng) | `SM_PlasticTable_TableCover` | Đ1 | 2,24 | −2,13 | 0,83×0,83×0,72 | −0,15 → 0,57 | -6° |
| ST-30 | Bàn nhựa trải khăn (nhận phúng viếng) | `SM_PlasticTable_TableCover` | Đ1 | 5,89 | −2,07 | 0,83×0,83×0,72 | −0,15 → 0,57 | -1° |
| ST-31 | Ghế nhựa xanh | `SM_PlasticChair_Green` | Đ1 | 2,87 | −1,71 | 0,39×0,43×0,81 | −0,15 → 0,66 | -110° |
| ST-32 | Ghế nhựa xanh | `SM_PlasticChair_Green` | Đ1 | 5,25 | −1,58 | 0,39×0,43×0,81 | −0,15 → 0,66 | 116° |
| ST-33 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ1 | 1,90 | −1,44 | 0,39×0,43×0,81 | −0,15 → 0,66 | 137° |
| ST-34 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ1 | 6,42 | −1,43 | 0,39×0,43×0,81 | −0,15 → 0,66 | -31° |
| ST-35 | Vòng hoa (héo) | `VongHoa_Heo` | Đ2 | 0,25 | −5,66 | 1,10×0,51×1,35 | −0,15 → 1,20 | 179° |
| ST-36 | Vòng hoa (héo) | `VongHoa_Heo` | Đ2 | 1,50 | −5,66 | 1,10×0,51×1,35 | −0,15 → 1,20 | -179° |
| ST-37 | Vòng hoa (héo) | `VongHoa_Heo` | Đ2 | 7,25 | −5,66 | 1,10×0,51×1,35 | −0,15 → 1,20 | ra đường |
| ST-38 | Chậu đốt vàng mã (tro) | `SM_BurnBasin_Ash` | Đ2 | 7,30 | −4,85 | 0,50×0,50×0,25 | −0,15 → 0,10 | trong |
| ST-39 | Chồng 8 ghế nhựa | `SM_PlasticChairStack_8` | Đ2 | 2,30 | −4,60 | 0,39×0,45×1,24 | −0,15 → 1,09 | trong |
| ST-40 | Ghế nhựa xanh | `SM_PlasticChair_Green` | Đ2 | 2,05 | −3,29 | 0,39×0,43×0,81 | −0,15 → 0,66 | -49° |
| ST-41 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ2 | 6,08 | −3,20 | 0,39×0,43×0,81 | −0,15 → 0,66 | -12° |
| ST-42 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ2 | 0,63 | −3,04 | 0,39×0,43×0,81 | −0,15 → 0,66 | 106° |
| ST-43 | Bàn nhựa trải khăn (nhận phúng viếng) | `SM_PlasticTable_TableCover` | Đ2 | 1,32 | −3,01 | 0,83×0,83×0,72 | −0,15 → 0,57 | -2° |
| ST-44 | Bàn nhựa trải khăn (nhận phúng viếng) | `SM_PlasticTable_TableCover` | Đ2 | 6,21 | −2,52 | 0,83×0,83×0,72 | −0,06 → 0,66 | 2° |
| ST-45 | Ghế nhựa xanh | `SM_PlasticChair_Green` | Đ2 | 6,93 | −2,55 | 0,39×0,43×0,81 | −0,15 → 0,66 | -53° |
| ST-46 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ2 | 1,43 | −2,29 | 0,39×0,43×0,81 | −0,15 → 0,66 | -38° |
| ST-47 | Chồng 4 ghế nhựa | `SM_PlasticChairStack_4` | Đ2 | 0,30 | −1,90 | 0,39×0,43×0,99 | −0,15 → 0,84 | trong |
| ST-48 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ2 | 5,79 | −1,83 | 0,39×0,43×0,81 | −0,15 → 0,66 | 148° |
| ST-49 | Vòng hoa (héo) | `VongHoa_Heo` | Đ3 | 1,50 | −5,66 | 1,10×0,51×1,35 | −0,15 → 1,20 | -179° |
| ST-50 | Vòng hoa (héo) | `VongHoa_Heo` | Đ3 | 7,25 | −5,66 | 1,10×0,51×1,35 | −0,15 → 1,20 | ra đường |
| ST-51 | Chậu đốt vàng mã (tro) | `SM_BurnBasin_Ash` | Đ3 | 7,30 | −4,85 | 0,50×0,50×0,25 | 0,05 → 0,30 | trong |
| ST-52 | Chồng 8 ghế nhựa | `SM_PlasticChairStack_8` | Đ3 | 2,30 | −4,60 | 0,39×0,45×1,24 | −0,15 → 1,09 | trong |
| ST-53 | Ghế nhựa xanh | `SM_PlasticChair_Green` | Đ3 | 6,26 | −3,13 | 0,39×0,43×0,81 | −0,15 → 0,66 | 2° |
| ST-54 | Bàn nhựa trải khăn (nhận phúng viếng) | `SM_PlasticTable_TableCover` | Đ3 | 6,22 | −2,46 | 0,83×0,83×0,72 | −0,09 → 0,63 | -6° |
| ST-55 | Chồng 8 ghế nhựa | `SM_PlasticChairStack_8` | Đ3 | 0,30 | −1,90 | 0,39×0,45×1,24 | −0,15 → 1,09 | trong |
| ST-56 | Chồng 4 ghế nhựa | `SM_PlasticChairStack_4` | Đ3 | 7,30 | −1,90 | 0,39×0,43×0,99 | −0,15 → 0,84 | trong |
| ST-57 | Ghế nhựa đỏ | `SM_PlasticChair_Red` | Đ3 | 6,19 | −1,74 | 0,39×0,43×0,81 | −0,15 → 0,66 | -157° |

### Đồ đặt trên bàn / kệ / giường (10)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| ST-58 | Hộp thư ở cổng | `HopThu_Cong` |  | 5,40 | −5,45 | 0,25×0,10×0,12 | 1,09 → 1,21 | ra đường |
| ST-59 | Khay phong bì phúng viếng | `SM_EnvelopeTray_day` |  | 5,42 | −3,98 | 0,28×0,20×0,06 | 0,57 → 0,63 | trong |
| ST-60 | Sổ phúng viếng | `SoPhungViengTang` |  | 5,78 | −3,90 | 0,28×0,30×0,04 | 0,57 → 0,61 | → phải |
| ST-61 | Khay trà ly thuỷ tinh | `KhayTra_LyThuyTinh` | Đ1 | 1,31 | −3,42 | 0,40×0,26×0,17 | 0,57 → 0,74 | 53° |
| ST-62 | Đĩa kẹo bánh hạt dưa | `DiaKeoBanh_HatDua` | Đ1 | 6,97 | −3,03 | 0,52×0,46×0,07 | 0,57 → 0,64 | -148° |
| ST-63 | Đĩa kẹo bánh hạt dưa | `DiaKeoBanh_HatDua` | Đ1 | 2,28 | −2,13 | 0,52×0,46×0,07 | 0,57 → 0,64 | 106° |
| ST-64 | Khay trà ly thuỷ tinh | `KhayTra_LyThuyTinh` | Đ1 | 5,89 | −2,07 | 0,40×0,26×0,17 | 0,57 → 0,74 | 159° |
| ST-65 | Khay trà ly thuỷ tinh | `KhayTra_LyThuyTinh` | Đ2 | 1,32 | −3,01 | 0,40×0,26×0,17 | 0,57 → 0,74 | -95° |
| ST-66 | Đĩa kẹo bánh hạt dưa | `DiaKeoBanh_HatDua` | Đ2 | 6,15 | −2,53 | 0,52×0,46×0,07 | 0,66 → 0,73 | -42° |
| ST-67 | Khay trà ly thuỷ tinh | `KhayTra_LyThuyTinh` | Đ3 | 6,23 | −2,46 | 0,40×0,26×0,17 | 0,70 → 0,88 | 11° |

### Điện – đèn – quạt (5)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| ST-68 | Chuông điện (nút bấm) | `ChuongDien_Nut` |  | 6,50 | −5,41 | 0,07×0,02×0,11 | 1,34 → 1,46 | ra đường |
| ST-69 | Đèn cổng chao tôn | `DenCong_ChaoTon` |  | 0,50 | −0,47 | 0,41×0,54×0,22 | 2,56 → 2,78 | ra đường |
| ST-70 | Đèn cổng chao tôn | `DenCong_ChaoTon` |  | 7,05 | −0,47 | 0,41×0,54×0,22 | 2,56 → 2,78 | ra đường |
| ST-71 | Hộp công tơ điện | `HopCongTo` |  | 7,30 | −0,28 | 0,30×0,15×0,40 | 1,50 → 1,90 | ra đường |
| ST-72 | Đồng hồ nước (dưới hộp công tơ) | `DongHoNuoc_HopCong` |  | 7,30 | −0,25 | 0,34×0,10×0,16 | 1,02 → 1,18 | ra đường |

### Cửa, cổng, cửa sổ, rèm (6)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| ST-73 | Trụ gạch cổng sắt | `CongSat_TruGach` |  | 2,20 | −5,25 | 0,31×0,31×2,10 | −0,15 → 1,95 | trong |
| ST-74 | Trụ gạch cổng sắt | `CongSat_TruGach` |  | 5,40 | −5,25 | 0,31×0,31×2,10 | −0,15 → 1,95 | trong |
| ST-75 | Trụ gạch cổng sắt | `CongSat_TruGach` |  | 6,50 | −5,25 | 0,31×0,31×2,10 | −0,15 → 1,95 | trong |
| ST-76 | Cửa ngách ở cổng | `CuaNgachCong (mở)` |  | 6,34 | −4,80 | 0,90×0,03×1,80 | −0,15 → 1,65 | ← trái |
| ST-77 | Cánh cổng sắt trái (mở) | `CanhCong_Trai (mở)` |  | 2,53 | −4,60 | 1,35×0,03×1,80 | −0,15 → 1,65 | 104° |
| ST-78 | Cánh cổng sắt phải (mở) | `CanhCong_Phai (mở)` |  | 5,07 | −4,60 | 1,35×0,03×1,80 | −0,15 → 1,65 | -104° |

### Kết cấu phụ / cố định (1)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| ST-79 | Biển số nhà tráng men | `BienSoNha` |  | 2,20 | −5,40 | 0,22×0,01×0,14 | 1,48 → 1,62 | ra đường |

## Tiệm vật liệu — khu bán hàng + kho

<a id="tiem"></a>Tầng 1 (±0,00) · phòng TB, TK · 32 vật

### Đồ đặt trên sàn (25)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| TB-01 | Tủ kính 3 tầng (tiệm) | `TuKinh` |  | −0,73 | −3,90 | 1,60×0,65×1,20 | 0,00 → 1,20 | ← trái |
| TB-02 | Kệ sắt V lỗ + đồ kệ | `KeVLo_DoKe` |  | −0,58 | −2,40 | 0,90×0,35×1,60 | 0,00 → 1,60 | ← trái |
| TB-03 | Quầy hàng + ngăn kéo (lọ keo) | `LOC_CanhQuayHang` |  | −3,30 | −2,30 | 2,20×1,39×0,96 | 0,00 → 0,96 | ← trái |
| TB-04 | Cân bàn (bán hàng) | `CanBanHang` |  | −2,68 | −0,78 | 0,32×0,32×0,45 | 0,00 → 0,45 | trong |
| TB-05 | Cụm thùng sơn | `ThungSon_Cum2` |  | −0,76 | 0,21 | 1,02×0,68×0,73 | −0,00 → 0,73 | ← trái |
| TB-06 | Chồng 4 ghế nhựa | `SM_PlasticChairStack_4` |  | −3,00 | 1,12 | 0,39×0,43×0,99 | 0,00 → 0,99 | trong |
| TB-07 | Cuộn dây điện | `CuonDayDien` |  | −2,05 | 1,30 | 0,12×0,53×0,42 | 0,00 → 0,42 | trong |
| TB-08 | Chồng bao xi măng | `BaoXiMang_Chong` |  | −0,72 | 1,26 | 0,71×0,53×0,74 | 0,00 → 0,74 | ← trái |
| TB-09 | Bàn thờ Thần Tài | `BanThoThanTai` |  | −3,88 | 1,62 | 0,47×0,37×1,08 | 0,00 → 1,08 | ra đường |
| TK-01 | Cuộn dây điện (2) | `CuonDayDien_2` |  | −3,20 | 2,30 | 0,67×0,39×0,44 | 0,00 → 0,44 | trong |
| TK-02 | Bao tải rỗng | `BaoTaiRong` |  | −1,55 | 2,39 | 0,52×0,42×0,23 | 0,00 → 0,23 | -10° |
| TK-03 | Chồng bao xi măng (2) | `BaoXiMang_Chong2` |  | −0,75 | 2,50 | 0,72×0,50×0,48 | 0,00 → 0,48 | ← trái |
| TK-04 | Bụi xi măng trên sàn | `BuiXiMang_San` |  | −2,30 | 3,00 | 1,34×0,94×0,00 | 0,00 → 0,00 | trong |
| TK-05 | Bao xi măng lẻ | `BaoXiMang_Le` |  | −1,50 | 3,20 | 0,77×0,67×0,11 | −0,00 → 0,11 | 20° |
| TK-06 | Kệ gỗ dài (kho) | `KeGoDai` |  | −3,99 | 3,40 | 2,20×0,42×1,20 | 0,00 → 1,20 | → phải |
| TK-07 | Xe rùa | `XeRua` |  | −2,30 | 4,10 | 0,66×1,86×0,84 | −0,00 → 0,84 | 30° |
| TK-08 | Bó thanh sắt | `ThanhSat_Bo` |  | −0,60 | 4,30 | 0,31×0,20×2,40 | −0,00 → 2,40 | ← trái |
| TK-09 | Cây chổi | `CayChoi` |  | −1,57 | 4,47 | 0,10×0,22×1,38 | 0,00 → 1,38 | trong |
| TK-10 | Cuộn lưới thép | `CuonLuoiThep` |  | −0,75 | 5,20 | 1,22×0,59×0,97 | −0,00 → 0,97 | ← trái |
| TK-11 | Thang nhôm | `ThangNhom` |  | −4,12 | 5,30 | 0,38×0,12×1,80 | 0,00 → 1,80 | → phải |
| TK-12 | Chồng gạch mẫu | `GachMau_Chong` |  | −3,40 | 5,55 | 0,29×0,25×0,16 | 0,00 → 0,16 | trong |
| TK-13 | Thùng gỗ | `ThungGo` |  | −2,80 | 5,70 | 0,52×0,40×0,43 | 0,00 → 0,43 | 15° |
| TK-14 | Thùng sơn | `ThungSon` |  | −0,80 | 6,40 | 0,69×0,48×0,73 | 0,00 → 0,73 | ← trái |
| TK-15 | Bó ống nước (kho) | `OngNuoc_Bo` |  | −3,90 | 6,45 | 0,53×0,34×2,00 | 0,00 → 2,00 | trong |
| TK-16 | Ống nước nằm (kho) | `OngNuoc_Nam` |  | −2,15 | 6,45 | 2,09×0,35×0,13 | 0,00 → 0,13 | trong |

### Đồ đặt trên bàn / kệ / giường (1)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| TB-10 | Sổ hoá đơn trên quầy | `SoHoaDon_Quay` |  | −3,31 | −2,98 | 0,50×0,25×0,08 | 0,95 → 1,03 | 10° |

### Treo tường / treo trần (3)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| TB-11 | Bảng giá viết tay | `BangGia` |  | −0,41 | −3,20 | 0,36×0,00×0,26 | 1,64 → 1,90 | ← trái |
| TB-12 | Lịch bloc công ty vật liệu | `LichBloc_CongTy` |  | −4,19 | −3,00 | 0,32×0,02×0,54 | 1,77 → 2,31 | → phải |
| TB-13 | Đồng hồ treo tường (tiệm) | `DongHoTreo_Tiem` |  | −0,43 | −2,50 | 0,34×0,06×0,34 | 2,18 → 2,52 | ← trái |

### Điện – đèn – quạt (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| TB-14 | Đèn tuýp máng 1,2 m | `DenTuyp_120` |  | −2,30 | −2,00 | 1,22×0,07×0,06 | 3,54 → 3,60 | → phải |
| TB-15 | Quạt bàn sắt cũ | `QuatBan_SatCu` |  | −3,30 | −1,50 | 0,39×0,27×0,55 | 0,95 → 1,50 | → phải |

### Cửa, cổng, cửa sổ, rèm (1)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| TB-16 | Cửa sắt xếp mặt tiền tiệm | `CuaSatXep_Dong` |  | −2,30 | −5,30 | 3,66×0,04×2,41 | 0,00 → 2,41 | trong |

## Phòng khách + bàn thờ vong

<a id="pk"></a>Tầng 1 (±0,00) · phòng PK · 68 vật

### Đồ đặt trên sàn (25)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| PK-01 | Kệ giày chân cầu thang | `KeGiay_ChanCau` |  | 1,20 | 0,20 | 0,80×0,34×0,62 | 0,00 → 0,62 | trong |
| PK-02 | Chổi quét + ky hốt rác | `ChoiQuet_Ky` |  | 7,30 | 0,30 | 0,44×0,27×1,30 | 0,00 → 1,30 | trong |
| PK-03 | Chậu cây kiểng lá to | `ChauCayKieng_LaTo` |  | 6,55 | 0,45 | 0,74×0,78×1,17 | 0,00 → 1,17 | trong |
| PK-04 | Thảm chùi chân (cửa) | `ThamChuiChan_Cua` |  | 3,80 | 0,80 | 0,60×0,40×0,02 | 0,00 → 0,02 | trong |
| PK-05 | Chậu cây kiểng lá to | `ChauCayKieng_LaTo` |  | 0,45 | 0,95 | 0,74×0,78×1,17 | 0,00 → 1,17 | trong |
| PK-06 | Tập giấy + bút bi | `LOC_TapGiay_ButBi` |  | 0,20 | 1,33 | 0,12×0,15×0,02 | 0,00 → 0,02 | → phải |
| PK-07 | Ghế salon đơn | `Salon_GheDon` |  | 5,40 | 1,42 | 0,62×0,62×0,90 | 0,00 → 0,90 | → phải |
| PK-08 | Bàn nhỏ đặt máy điện thoại | `LOC_BanNhoDatMay` |  | 0,22 | 1,63 | 0,54×0,44×0,72 | −0,00 → 0,72 | → phải |
| PK-09 | Bàn nước salon | `Salon_BanNuoc` |  | 6,28 | 1,95 | 1,00×0,55×0,46 | 0,00 → 0,46 | → phải |
| PK-10 | Ghế salon dài | `Salon_GheDai` |  | 7,28 | 1,95 | 1,70×0,62×0,90 | 0,00 → 0,90 | ← trái |
| PK-11 | Bình hoa gốm lớn | `BinhHoaLon_Gom` |  | 0,45 | 2,10 | 0,38×0,38×0,80 | 0,00 → 0,80 | trong |
| PK-12 | Ghế salon đơn | `Salon_GheDon` |  | 5,40 | 2,47 | 0,62×0,62×0,90 | 0,00 → 0,90 | → phải |
| PK-13 | Loa thùng | `Loa_Thung` |  | 7,45 | 3,38 | 0,20×0,25×0,34 | 0,00 → 0,34 | ← trái |
| PK-14 | Quan tài (LỘC) — đầu về phía bàn thờ | `QuanTai_LOC` |  | 2,98 | 3,70 | 2,15×2,10×1,30 | 0,00 → 1,30 | trong |
| PK-15 | Tủ TV đứng + TV, VCD, loa | `TuTV_Dung_Bo` |  | 7,34 | 4,20 | 1,22×0,52×1,90 | 0,00 → 1,90 | ← trái |
| PK-16 | Loa thùng | `Loa_Thung` |  | 7,45 | 5,02 | 0,20×0,25×0,34 | 0,00 → 0,34 | ← trái |
| PK-17 | Chiếu + mền khách ngủ lại | `ChieuMen_KhachO` |  | 0,50 | 5,55 | 0,90×0,55×0,31 | 0,15 → 0,46 | → phải |
| PK-18 | Chổi lông gà | `CayChoiLongGa` |  | 7,40 | 5,60 | 0,19×0,19×0,64 | 0,00 → 0,64 | trong |
| PK-19 | Bát gạo (đồ cúng) | `LOC_BatGao` |  | 1,15 | 6,20 | 0,12×0,12×0,08 | −0,00 → 0,08 | trong |
| PK-20 | Đống đồ cúng đám tang | `LOC_DongDoCungTang` |  | 0,53 | 6,33 | 1,95×1,07×0,52 | 0,00 → 0,52 | → phải |
| PK-21 | Bát muối (đồ cúng) | `LOC_BatMuoi` |  | 1,15 | 6,45 | 0,12×0,12×0,07 | −0,00 → 0,07 | trong |
| PK-22 | Ghế đẩu gỗ | `GheDau_Go` |  | 5,05 | 7,12 | 0,32×0,32×0,44 | 0,00 → 0,44 | 25° |
| PK-23 | Bàn thờ vong — Đêm 1 | `BanThoVong_Dem1` | Đ1 | 0,35 | 3,70 | 2,77×0,70×1,70 | 0,00 → 1,70 | → phải |
| PK-24 | Bàn thờ vong — Đêm 2 | `BanThoVong_Dem2` | Đ2 | 0,35 | 3,70 | 2,77×0,70×1,70 | 0,00 → 1,70 | → phải |
| PK-25 | Bàn thờ vong — Đêm 3 | `BanThoVong_Dem3` | Đ3 | 0,35 | 3,70 | 2,77×0,70×1,70 | 0,00 → 1,70 | → phải |

### Đồ đặt trên bàn / kệ / giường (14)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| PK-26 | Nón lá treo tường | `NonLa_TreoTuong` |  | 7,50 | 0,90 | 0,44×0,21×0,44 | 1,33 → 1,77 | ← trái |
| PK-27 | Đệm ghế đơn | `Salon_Dem_NgoiDon` |  | 5,43 | 1,42 | 0,50×0,50×0,07 | 0,42 → 0,49 | → phải |
| PK-28 | Điện thoại bàn (trọn bộ) | `LOC_DienThoai_TronBo` |  | 0,22 | 1,52 | 0,42×0,22×0,07 | 0,72 → 0,79 | → phải |
| PK-29 | Hộp khăn giấy nhựa | `HopKhanGiay_Nhua` |  | 6,32 | 1,49 | 0,24×0,12×0,14 | 0,46 → 0,60 | trong |
| PK-30 | Đệm ghế dài (phía kia) | `Salon_Dem_NgoiDai_Kia` |  | 7,25 | 1,56 | 0,78×0,56×0,07 | 0,42 → 0,49 | ← trái |
| PK-31 | Khay ấm chén | `KhayAmChen` |  | 6,28 | 1,88 | 0,54×0,40×0,30 | 0,46 → 0,76 | → phải |
| PK-32 | Báo cũ trên bàn | `BaoCu_TrenBan` |  | 7,20 | 1,95 | 0,45×0,34×0,03 | 0,42 → 0,45 | 100° |
| PK-33 | Điều khiển bọc nilon | `DieuKhien` |  | 6,20 | 2,30 | 0,06×0,19×0,03 | 0,47 → 0,50 | 75° |
| PK-34 | Đệm ghế dài (gần TV) | `Salon_Dem_NgoiDai_GanTV` |  | 7,25 | 2,34 | 0,78×0,56×0,07 | 0,42 → 0,49 | ← trái |
| PK-35 | Khay hoa quả nhựa | `KhayHoaQua_Nhua` |  | 6,28 | 2,40 | 0,48×0,46×0,12 | 0,46 → 0,58 | trong |
| PK-36 | Gạt tàn thuỷ tinh | `GatTan_ThuyTinh` |  | 6,40 | 2,40 | 0,15×0,15×0,05 | 0,53 → 0,58 | trong |
| PK-37 | Đệm ghế đơn | `Salon_Dem_NgoiDon` |  | 5,43 | 2,47 | 0,50×0,50×0,07 | 0,42 → 0,49 | → phải |
| PK-38 | Kệ gỗ treo tường | `KeGo_TreoTuong` |  | 7,50 | 3,00 | 0,80×0,20×0,67 | 1,38 → 2,05 | ← trái |
| PK-39 | Hộp băng cassette | `BangCassette_Hop` |  | 7,47 | 5,02 | 0,24×0,18×0,02 | 0,34 → 0,36 | -60° |

### Treo tường / treo trần (9)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| PK-40 | Đồng hồ quả lắc | `DongHoQuaLac` |  | 5,50 | 0,07 | 0,32×0,15×0,79 | 1,60 → 2,40 | trong |
| PK-41 | Tranh sơn thuỷ (khung) | `TranhSonThuy_Khung` |  | 0,02 | 0,85 | 0,66×0,05×0,46 | 1,55 → 2,01 | → phải |
| PK-42 | Ảnh gia đình 1997 (khung treo) | `AnhGiaDinh_Tex` |  | 7,59 | 1,95 | 0,50×0,02×0,40 | 1,60 → 2,00 | ← trái |
| PK-43 | Lọ hoa giả nhựa | `LoHoaGia_Nhua` |  | 7,30 | 3,65 | 0,30×0,28×0,80 | 1,90 → 2,70 | trong |
| PK-44 | Radio cassette 2 loa | `RadioCassette_2Loa` |  | 7,30 | 4,20 | 0,38×0,14×0,49 | 1,90 → 2,39 | ← trái |
| PK-45 | Đĩa VCD xếp chồng | `DiaVCD_XepChong` |  | 7,30 | 4,75 | 0,28×0,14×0,07 | 1,90 → 1,97 | → phải |
| PK-46 | Lịch tháng 8/2002 | `LichThang8_2002` |  | 7,59 | 5,30 | 0,30×0,01×0,47 | 1,39 → 1,85 | ← trái |
| PK-47 | Tranh thêu thuyền buồm | `TranhTheu_Tex` |  | 3,50 | 7,39 | 1,00×0,02×0,50 | 2,45 → 2,95 | ra đường |
| PK-48 | Bằng "Gia đình văn hoá" | `BangGDVH_Tex` |  | 5,30 | 7,39 | 0,42×0,02×0,30 | 1,95 → 2,25 | ra đường |

### Điện – đèn – quạt (12)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| PK-49 | Đèn tuýp máng 1,2 m | `DenTuyp_120` |  | 3,00 | 1,00 | 1,22×0,07×0,06 | 3,14 → 3,20 | trong |
| PK-50 | Đèn hắt trần (ống) | `DenHat_Ong` |  | 2,50 | 2,55 | 1,00×0,02×0,02 | 3,14 → 3,16 | trong |
| PK-51 | Đèn hắt trần (ống) | `DenHat_Ong` |  | 3,50 | 2,55 | 1,00×0,02×0,02 | 3,14 → 3,16 | trong |
| PK-52 | Quạt cây (nguyên bộ) | `QuatCay_NguyenBo` |  | 6,50 | 3,50 | 0,40×0,43×1,27 | 0,00 → 1,27 | trong |
| PK-53 | Đèn chùm 5 tay | `DenChum_5Tay` |  | 3,00 | 3,70 | 0,56×0,58×0,60 | 2,60 → 3,20 | trong |
| PK-54 | Ổ cắm kéo dài | `OCamKeoDai` |  | 0,85 | 4,35 | 0,25×0,30×0,06 | −0,00 → 0,06 | trong |
| PK-55 | Đèn hắt trần (ống) | `DenHat_Ong` |  | 2,50 | 4,85 | 1,00×0,02×0,02 | 3,14 → 3,16 | trong |
| PK-56 | Đèn hắt trần (ống) | `DenHat_Ong` |  | 3,50 | 4,85 | 1,00×0,02×0,02 | 3,14 → 3,16 | trong |
| PK-57 | Quạt trần — cánh | `QuatTran_Canh` |  | 3,07 | 6,04 | 0,56×0,17×0,01 | 3,02 → 3,04 | -105° |
| PK-58 | Quạt trần — thân | `QuatTran_Than` |  | 3,00 | 6,30 | 0,18×0,18×0,22 | 2,98 → 3,20 | trong |
| PK-59 | Quạt trần — cánh | `QuatTran_Canh` |  | 2,74 | 6,37 | 0,56×0,17×0,01 | 3,02 → 3,04 | 15° |
| PK-60 | Quạt trần — cánh | `QuatTran_Canh` |  | 3,19 | 6,49 | 0,56×0,17×0,01 | 3,02 → 3,04 | 135° |

### Cửa, cổng, cửa sổ, rèm (6)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| PK-61 | Cửa chính — cánh trái (mở ra ngoài) | `CuaChinh_Trai (mở ra ngoài)` |  | 1,71 | −0,23 | 1,41×0,18×2,60 | 0,00 → 2,60 | ra đường |
| PK-62 | Cửa chính — cánh phải (mở ra ngoài) | `CuaChinh_Phai (mở ra ngoài)` |  | 5,89 | −0,23 | 1,41×0,18×2,60 | 0,00 → 2,60 | trong |
| PK-63 | Cửa sắt hầm | `CuaSatHam (mở)` |  | 6,33 | 6,81 | 0,81×0,09×1,90 | 0,00 → 1,90 | ← trái |
| PK-64 | Khung cửa sắt hầm | `KhungCuaSatHam` |  | 6,35 | 6,80 | 0,88×0,06×1,96 | 0,00 → 1,96 | → phải |
| PK-65 | Rèm hạt nhựa | `Rem_Hat` |  | 2,56 | 7,45 | 0,30×0,15×2,10 | 0,10 → 2,20 | trong |
| PK-66 | Rèm hạt nhựa | `Rem_Hat` |  | 4,44 | 7,45 | 0,30×0,15×2,10 | 0,10 → 2,20 | trong |

### Kết cấu phụ / cố định (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| PK-67 | Trần thạch cao giật cấp | `TranGiatCap_Khung` |  | 3,00 | 3,70 | 2,40×2,80×0,15 | 3,05 → 3,20 | trong |
| PK-68 | Hoa trần (phào trang trí) | `HoaTran` |  | 3,00 | 3,70 | 0,60×0,60×0,01 | 3,19 → 3,20 | trong |

## Sảnh sau + chân cầu thang + cửa hầm

<a id="ss"></a>Tầng 1 (±0,00) · phòng SS · 27 vật

### Đồ đặt trên sàn (5)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| SS-01 | Giá dép | `GiaDep_Bo` |  | 5,25 | 7,69 | 0,90×0,39×0,56 | 0,00 → 0,56 | trong |
| SS-02 | Dép của Nhím (rơi) | `Dep_Nhim_Roi` |  | 4,62 | 7,95 | 0,09×0,11×0,02 | 0,00 → 0,02 | 35° |
| SS-03 | Dép khách | `Dep_Khach` |  | 5,30 | 8,20 | 0,29×0,37×0,05 | 0,00 → 0,05 | 8° |
| SS-04 | Xe đạp cũ của Khôi | `XeDap_Khoi` |  | 0,98 | 10,45 | 1,46×0,63×0,95 | 0,00 → 0,95 | ra đường |
| SS-05 | Chổi lau nhà + xô | `ChoiLau_Gop` |  | 6,05 | 10,60 | 0,51×0,22×1,10 | −0,00 → 1,10 | ← trái |

### Treo tường / treo trần (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| SS-06 | Bảng điện chính (cầu dao, cầu chì) | `BangDienChinh` |  | 6,26 | 8,80 | 0,30×0,07×0,45 | 1,59 → 2,05 | → phải |
| SS-07 | Móc chìa khoá (thiếu một chùm) | `MocChiaKhoa` |  | 6,29 | 9,40 | 0,30×0,03×0,10 | 1,45 → 1,55 | ← trái |

### Điện – đèn – quạt (1)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| SS-08 | Bóng đèn compact | `BongCompact` |  | 4,10 | 9,20 | 0,04×0,03×0,13 | 3,07 → 3,20 | trong |

### Kết cấu phụ / cố định (19)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| SS-09 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 7,59 | 0,90×0,02×0,02 | 0,15 → 0,17 | trong |
| SS-10 | Trụ đầu thang gỗ tiện | `TruDauThang` |  | 1,87 | 7,60 | 0,14×0,14×0,95 | 0,00 → 0,95 | trong |
| SS-11 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 7,84 | 0,90×0,02×0,02 | 0,32 → 0,34 | trong |
| SS-12 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 7,86 | 0,90×0,02×0,02 | 3,21 → 3,23 | ra đường |
| SS-13 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 8,11 | 0,90×0,02×0,02 | 3,04 → 3,06 | ra đường |
| SS-14 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 8,09 | 0,90×0,02×0,02 | 0,49 → 0,51 | trong |
| SS-15 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 8,34 | 0,90×0,02×0,02 | 0,66 → 0,68 | trong |
| SS-16 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 8,36 | 0,90×0,02×0,02 | 2,87 → 2,89 | ra đường |
| SS-17 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 8,61 | 0,90×0,02×0,02 | 2,70 → 2,72 | ra đường |
| SS-18 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 8,59 | 0,90×0,02×0,02 | 0,83 → 0,85 | trong |
| SS-19 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 8,84 | 0,90×0,02×0,02 | 1,00 → 1,02 | trong |
| SS-20 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 8,86 | 0,90×0,02×0,02 | 2,53 → 2,55 | ra đường |
| SS-21 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 9,11 | 0,90×0,02×0,02 | 2,36 → 2,38 | ra đường |
| SS-22 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 9,09 | 0,90×0,02×0,02 | 1,17 → 1,19 | trong |
| SS-23 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 9,34 | 0,90×0,02×0,02 | 1,34 → 1,36 | trong |
| SS-24 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 9,36 | 0,90×0,02×0,02 | 2,19 → 2,21 | ra đường |
| SS-25 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 9,61 | 0,90×0,02×0,02 | 2,02 → 2,04 | ra đường |
| SS-26 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 9,59 | 0,90×0,02×0,02 | 1,51 → 1,53 | trong |
| SS-27 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 9,86 | 0,90×0,02×0,02 | 1,85 → 1,87 | ra đường |

## Bếp + chỗ ăn

<a id="ba"></a>Tầng 1 (±0,00) · phòng BA · 56 vật

### Đồ đặt trên sàn (21)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| BA-01 | Tủ lạnh một cánh | `TuLanh_Bo` |  | 7,23 | 11,50 | 0,58×0,74×1,45 | 0,00 → 1,45 | ← trái |
| BA-02 | Hũ / bình ngâm rượu | `HuBinhNgamRuou` |  | 0,30 | 11,55 | 0,56×0,48×0,48 | 0,00 → 0,48 | → phải |
| BA-03 | Bát đĩa trong chạn | `Chan_BatDiaTrongChan` |  | 0,25 | 12,65 | 0,90×0,47×1,60 | −0,00 → 1,60 | → phải |
| BA-04 | Chồng (bó) tre | `ChongTre` |  | 6,95 | 13,10 | 1,90×0,86×0,48 | 0,00 → 0,48 | → phải |
| BA-05 | Rổ rá + thớt | `RoRaThit` |  | 1,30 | 13,30 | 0,96×0,41×0,32 | 0,00 → 0,32 | trong |
| BA-06 | Bó mía | `BoMia` |  | 0,12 | 13,55 | 0,11×0,11×1,10 | −0,00 → 1,10 | trong |
| BA-07 | Ghế ăn (bố) | `GheAn_bo` |  | 4,25 | 13,89 | 0,40×0,44×0,85 | 0,00 → 0,85 | trong |
| BA-08 | Rổ rau quả | `RauQua_Ro` |  | 0,55 | 14,10 | 0,38×0,38×0,17 | 0,00 → 0,17 | trong |
| BA-09 | Ghế đẩu gỗ | `GheDau_Go` |  | 1,10 | 14,15 | 0,32×0,32×0,44 | 0,00 → 0,44 | 20° |
| BA-10 | Thùng rác | `ThungRac` |  | 0,20 | 14,60 | 0,30×0,27×0,32 | 0,00 → 0,32 | → phải |
| BA-11 | Ghế ăn (mẹ) | `GheAn_me` |  | 3,34 | 14,60 | 0,40×0,44×0,85 | 0,00 → 0,85 | → phải |
| BA-12 | Bộ bàn ăn + ghế | `BanAn_Bo` |  | 4,25 | 14,60 | 1,20×0,81×0,76 | 0,00 → 0,76 | trong |
| BA-13 | Ghế ăn (Khôi) | `GheAn_khoi` |  | 5,16 | 14,60 | 0,40×0,44×0,85 | 0,00 → 0,85 | ← trái |
| BA-14 | Tủ chạn chén bát | `TuBuffet_ChenBat` |  | 7,32 | 14,95 | 1,04×0,47×1,78 | 0,00 → 1,78 | ← trái |
| BA-15 | Bệ rửa xây + chậu inox + vòi | `BeRua_Bo` |  | 0,31 | 15,30 | 0,80×0,61×0,97 | 0,00 → 0,97 | → phải |
| BA-16 | Ghế ăn (Nhím) | `GheAn_nhim` |  | 4,25 | 15,31 | 0,40×0,44×0,85 | 0,00 → 0,85 | ra đường |
| BA-17 | Xô chậu khăn lau | `XoChauKhanLau` |  | 7,20 | 15,80 | 0,32×0,32×0,33 | 0,00 → 0,33 | trong |
| BA-18 | Chậu cây kiểng lá to | `ChauCayKieng_LaTo` |  | 6,15 | 16,00 | 0,74×0,78×1,17 | 0,00 → 1,17 | trong |
| BA-19 | Mặt bếp xây 1,80 m | `MatBepXay_180` |  | 0,92 | 16,10 | 1,80×0,60×0,80 | −0,00 → 0,80 | ra đường |
| BA-20 | Bình gas | `BinhGas` |  | 2,05 | 16,15 | 0,30×0,30×0,60 | 0,00 → 0,60 | trong |
| BA-21 | Thùng gạo nhựa | `ThungGao_Nhua` |  | 2,65 | 16,05 | 0,46×0,44×0,60 | 0,00 → 0,60 | trong |

### Đồ đặt trên bàn / kệ / giường (21)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| BA-22 | Phích (bình thuỷ) nước | `BinhThuy_Phich` |  | 7,25 | 11,50 | 0,26×0,20×0,42 | 1,30 → 1,72 | trong |
| BA-23 | Móc áo + khăn tắm | `MocAo_KhanTam` |  | 0,05 | 11,58 | 0,59×0,10×0,41 | 1,23 → 1,64 | → phải |
| BA-24 | Bát đĩa úp trên bàn | `BatDia_DangUp_Ban` |  | 6,95 | 13,10 | 0,48×0,34×0,12 | 0,43 → 0,55 | trong |
| BA-25 | Cốc bàn chải (trẻ em) | `CocBanChai_TreEm` |  | 0,28 | 15,00 | 0,07×0,06×0,22 | 0,80 → 1,02 | trong |
| BA-26 | Bát đĩa trong chậu | `BatDia_TrongChau` |  | 0,40 | 15,15 | 0,22×0,22×0,09 | 0,80 → 0,89 | → phải |
| BA-27 | Giá bát úp | `GiaBatUp` |  | 0,35 | 15,55 | 0,51×0,31×0,20 | 0,80 → 1,00 | → phải |
| BA-28 | Phích vỏ sắt hoa | `PhichHoa` |  | 1,74 | 15,98 | 0,16×0,12×0,34 | 0,77 → 1,10 | ra đường |
| BA-29 | Nồi cháo | `NoiChao_Rong` |  | 0,52 | 16,10 | 0,36×0,28×0,15 | 0,83 → 0,98 | ra đường |
| BA-30 | Bếp gas đôi | `BepGas_Doi` |  | 0,70 | 16,10 | 0,73×0,48×0,07 | 0,77 → 0,83 | ra đường |
| BA-31 | Ấm nhôm | `AmNhom` |  | 0,90 | 16,10 | 0,39×0,17×0,18 | 0,82 → 0,99 | trong |
| BA-32 | Lon sữa đặc đựng bút | `LonSuaDac_DungBut` |  | 1,15 | 16,10 | 0,12×0,12×0,26 | 0,80 → 1,06 | trong |
| BA-33 | Nồi cơm điện | `NoiComDien` |  | 1,50 | 16,10 | 0,29×0,37×0,23 | 0,77 → 0,99 | ra đường |
| BA-34 | Thanh treo dụng cụ bếp | `ThanhTreoDungCu_Bep` |  | 2,50 | 16,34 | 0,80×0,11×0,39 | 1,37 → 1,76 | ra đường |
| BA-35 | Túi gia vị treo | `TuiGiaVi_Treo` |  | 1,85 | 16,37 | 0,32×0,06×0,36 | 1,20 → 1,56 | ra đường |
| BA-36 | Bát cháo ăn | `BatChao_An` | Đ1 | 3,90 | 14,60 | 0,15×0,10×0,06 | 0,84 → 0,90 | trong |
| BA-37 | Lồng bàn nhựa | `LongBan` | Đ1 | 4,25 | 14,60 | 0,45×0,48×0,26 | 0,79 → 1,05 | trong |
| BA-38 | Bát cháo ăn | `BatChao_An` | Đ1 | 4,60 | 14,60 | 0,15×0,10×0,05 | 0,79 → 0,85 | trong |
| BA-39 | Bát cháo ăn | `BatChao_An` | Đ2 | 3,90 | 14,60 | 0,15×0,10×0,05 | 0,79 → 0,84 | trong |
| BA-40 | Lồng bàn nhựa | `LongBan` | Đ2 | 4,25 | 14,60 | 0,45×0,48×0,26 | 0,79 → 1,05 | trong |
| BA-41 | Bát cháo ăn | `BatChao_An` | Đ2 | 4,60 | 14,60 | 0,15×0,10×0,06 | 0,84 → 0,90 | trong |
| BA-42 | Mâm đồ cúng mặn (Đêm 3) | `MamDoCungMan` | Đ3 | 4,25 | 14,60 | 0,84×0,84×0,30 | 0,76 → 1,05 | trong |

### Treo tường / treo trần (7)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| BA-43 | Tranh hoa sen (khung) | `TranhHoaSen_Khung` |  | 5,75 | 11,02 | 0,66×0,05×0,46 | 1,55 → 2,01 | trong |
| BA-44 | Hộp bánh quy thiếc (tái dụng) | `HopBanhQuy_ThiecTaiDung` |  | 0,28 | 12,40 | 0,19×0,19×0,11 | 1,60 → 1,71 | trong |
| BA-45 | Đồng hồ treo vuông | `DongHoTreo_Vuong` |  | 7,56 | 12,50 | 0,30×0,09×0,30 | 2,05 → 2,35 | ← trái |
| BA-46 | Chai nước mắm, lọ gia vị | `ChaiNuocMam_Lo` |  | 0,25 | 12,95 | 0,34×0,08×0,27 | 1,60 → 1,87 | → phải |
| BA-47 | Lịch bloc (5/8) | `LichBloc` |  | 7,59 | 14,00 | 0,20×0,03×0,28 | 1,32 → 1,60 | ← trái |
| BA-48 | Túi nilon treo | `TuiNilon_Treo` |  | 0,03 | 14,30 | 0,20×0,10×0,31 | 1,49 → 1,80 | → phải |
| BA-49 | Kệ chén treo tường | `KeChenTreoTuong` |  | 0,11 | 15,20 | 0,90×0,22×0,59 | 1,59 → 2,18 | → phải |

### Điện – đèn – quạt (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| BA-50 | Đèn tuýp máng 1,2 m | `DenTuyp_120` |  | 3,80 | 13,70 | 1,22×0,07×0,06 | 3,14 → 3,20 | trong |
| BA-51 | Ổ cắm đôi | `OCam_Doi` |  | 2,40 | 16,39 | 0,01×0,07×0,07 | 1,05 → 1,12 | ra đường |

### Cửa, cổng, cửa sổ, rèm (5)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| BA-52 | Cửa sắt kính sau bếp | `CuaSatKinh (mở)` |  | 4,70 | 16,00 | 0,90×0,06×2,20 | 0,00 → 2,20 | ← trái |
| BA-53 | Cửa sổ bếp (cánh phải) | `CuaSo_Bep_Phai` |  | 0,65 | 16,45 | 0,50×0,05×1,00 | 1,10 → 2,10 | ra đường |
| BA-54 | Song sắt hoa cửa sổ bếp | `SongSatHoa_Bep` |  | 0,90 | 16,36 | 0,95×0,01×0,95 | 1,12 → 2,07 | ra đường |
| BA-55 | Bậu cửa sổ granito — bếp | `BauCuaSo_Granito_Bep` |  | 0,90 | 16,39 | 1,00×0,08×0,03 | 1,07 → 1,10 | ra đường |
| BA-56 | Cửa sổ bếp (cánh trái) | `CuaSo_Bep_Trai` |  | 1,15 | 16,45 | 0,50×0,05×1,00 | 1,10 → 2,10 | trong |

## Giếng trời + khối sau (kho, WC, bể giặt)

<a id="ks"></a>Tầng 1 (±0,00) · phòng GT, KS · 30 vật

### Đồ đặt trên sàn (20)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| GT-01 | Thùng nước có nắp | `ThungNuoc_Nap` |  | 0,65 | 17,45 | 0,61×0,61×0,82 | −0,05 → 0,77 | trong |
| GT-02 | Chậu trầu bà | `ChauTrauBa` |  | 6,90 | 17,40 | 0,24×0,24×0,26 | −0,05 → 0,21 | trong |
| GT-03 | Giàn phơi gấp | `GianPhoi_Gap` |  | 3,60 | 17,90 | 0,65×0,04×0,85 | −0,05 → 0,80 | trong |
| KS-01 | Bộ xô – gáo – chậu WC | `BoXoGauChau_WC` |  | 6,95 | 19,60 | 1,10×0,42×0,42 | 0,00 → 0,42 | trong |
| KS-02 | Dép tổ ong | `DepToOng_Fix` |  | 4,95 | 20,55 | 0,33×0,30×0,05 | 0,00 → 0,05 | 15° |
| KS-03 | Thảm chùi chân | `ThamChuiChan` |  | 4,20 | 20,90 | 0,40×0,60×0,02 | 0,00 → 0,02 | → phải |
| KS-04 | Xí bệt | `XiBet_Tron` |  | 7,29 | 21,20 | 0,38×0,62×0,81 | 0,00 → 0,81 | ← trái |
| KS-05 | Xô gáo úp | `XoGao_Up` |  | 5,35 | 21,45 | 0,29×0,23×0,32 | 0,00 → 0,32 | 20° |
| KS-06 | Cọ bồn cầu | `ChoiCauBonCau` |  | 7,40 | 21,60 | 0,13×0,13×0,48 | 0,00 → 0,48 | trong |
| KS-07 | Ghế đẩu gỗ | `GheDau_Go` |  | 6,60 | 22,70 | 0,32×0,32×0,44 | 0,00 → 0,44 | -20° |
| KS-08 | Chồng thùng carton | `ThungCarton_Chong` |  | 3,00 | 23,30 | 1,09×0,54×0,97 | 0,00 → 0,97 | 25° |
| KS-09 | Chậu nhựa úp | `ChauNhua_Up` |  | 5,30 | 23,30 | 0,42×0,20×0,41 | 0,00 → 0,41 | trong |
| KS-10 | Ghế đẩu nhựa | `GheDau_Nhua` |  | 6,10 | 23,30 | 0,30×0,30×0,30 | 0,00 → 0,30 | trong |
| KS-11 | Chồng thùng carton | `ThungCarton_Chong` |  | 1,20 | 23,90 | 1,09×0,54×0,97 | 0,00 → 0,97 | 10° |
| KS-12 | Thau nhựa quần áo | `Thau_Nhua_QuanAo` |  | 5,85 | 23,90 | 0,70×0,70×0,32 | 0,00 → 0,32 | 15° |
| KS-13 | Bể giặt xi măng | `BeGiat` |  | 7,09 | 23,87 | 1,00×0,70×1,11 | 0,00 → 1,11 | ra đường |
| KS-14 | Chồng thùng carton | `ThungCarton_Chong` |  | 2,49 | 24,05 | 1,09×0,54×0,97 | 0,00 → 0,97 | -8° |
| KS-15 | Thùng nước có nắp | `ThungNuoc_Nap` |  | 4,20 | 24,20 | 0,61×0,61×0,82 | 0,00 → 0,82 | trong |
| KS-16 | Bàn chải cọ + xà phòng | `BanChaiCo_XaPhong` |  | 6,80 | 24,27 | 0,23×0,04×0,04 | 0,00 → 0,04 | 170° |
| KS-17 | Bao bột giặt, xà phòng | `BaoBotGiat_XaPhong` |  | 7,20 | 24,30 | 0,38×0,13×0,25 | 0,00 → 0,25 | trong |

### Đồ đặt trên bàn / kệ / giường (4)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| KS-18 | Móc áo + khăn tắm | `MocAo_KhanTam` |  | 6,02 | 19,25 | 0,58×0,10×0,41 | 1,23 → 1,64 | trong |
| KS-19 | Kệ xà phòng WC | `KeXaPhong_WC` |  | 6,40 | 21,73 | 0,50×0,14×0,18 | 1,19 → 1,37 | ra đường |
| KS-20 | Cuộn giấy vệ sinh treo | `GiayVeSinh_Treo` |  | 7,25 | 21,72 | 0,12×0,16×0,16 | 0,60 → 0,76 | ra đường |
| KS-21 | Khăn + móc nhựa (WC) | `Khan_Ru` |  | 5,05 | 21,80 | 1,00×1,00×1,00 | 1,10 → 2,10 | ra đường |

### Treo tường / treo trần (1)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| KS-22 | Vòi sen | `VoiSen` |  | 5,61 | 21,80 | 0,15×0,08×0,64 | 1,08 → 1,73 | ra đường |

### Điện – đèn – quạt (1)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| KS-23 | Đèn tuýp máng 0,6 m | `DenTuyp_60` |  | 6,10 | 20,50 | 0,62×0,07×0,06 | 3,14 → 3,20 | → phải |

### Cửa, cổng, cửa sổ, rèm (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| KS-24 | Khung cửa WC tầng 1 | `KhungCuaWC_T1` |  | 4,65 | 20,55 | 0,80×0,06×1,95 | 0,00 → 1,95 | ← trái |
| KS-25 | Cửa WC tầng 1 | `CuaWC_T1 (mở)` |  | 5,00 | 20,90 | 0,70×0,10×1,90 | 0,00 → 1,90 | ra đường |

### Kết cấu phụ / cố định (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| GT-04 | Máy bơm nước (giếng trời) | `MayBom_NuocGiengTroi` |  | 2,40 | 16,85 | 0,40×0,32×3,40 | −0,05 → 3,35 | trong |
| GT-05 | Ống nước mưa (giếng trời) | `OngNuocMua_GiengTroi` |  | 7,43 | 18,50 | 0,15×0,38×9,65 | −0,05 → 9,60 | ← trái |

## Hầm

<a id="h"></a>Hầm (−2,30) · phòng H · 8 vật

### Đồ đặt trên sàn (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| H-01 | Giấy kê chân (hầm) | `GiayKeChan` |  | 6,00 | 13,00 | 0,07×0,05×0,01 | −2,30 → −2,29 | trong |
| H-02 | Bàn thờ hầm | `BanThoHam` |  | 6,40 | 13,18 | 0,90×0,40×0,68 | −2,30 → −1,62 | ra đường |

### Đồ đặt trên bàn / kệ / giường (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| H-03 | Năm chén nước trên đế (bàn thờ hầm) | `Bo_Chen_Tren_De` |  | 6,40 | 13,08 | 0,11×0,42×0,08 | −1,62 → −1,54 | → phải |
| H-04 | Bát nhang (đủ bộ) | `BatNhang_FullAssembly` |  | 6,40 | 13,27 | 0,19×0,19×0,19 | −1,61 → −1,43 | ra đường |

### Điện – đèn – quạt (1)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| H-05 | Đèn hầm — bóng dây tóc (đủ bộ) | `DenHam_FullAssembly` |  | 6,40 | 11,91 | 0,06×1,43×0,64 | −0,84 → −0,20 | trong |

### Kết cấu phụ / cố định (3)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| H-06 | Chữ sơn đỏ "BẬT ĐIỆN" | `Decal_ChuBatDien` |  | 7,59 | 6,50 | 0,60×0,00×0,16 | 1,37 → 1,53 | ← trái |
| H-07 | Decal bong vôi | `Decal_BongVoi_A` |  | 5,80 | 13,38 | 0,30×0,00×0,40 | −0,60 → −0,20 | ra đường |
| H-08 | Tường dán kín bùa (hầm) | `TuongBuaMau` |  | 6,31 | 13,38 | 2,20×0,03×1,42 | −1,73 → −0,30 | ra đường |

## Phòng bố mẹ + WC khép kín + ban công

<a id="bm"></a>Tầng 2 (+3,40) · phòng BM · 70 vật

### Đồ đặt trên sàn (16)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| BM-01 | Bàn gỗ (đồ học cũ) | `BanCoHoc_Go` |  | 1,30 | 0,27 | 1,00×0,50×0,75 | 3,40 → 4,15 | trong |
| BM-02 | Tủ đầu giường | `TuDauGiuong` |  | 7,37 | 0,54 | 0,42×0,45×0,50 | 3,40 → 3,90 | ← trái |
| BM-03 | Ghế tựa + áo sơ mi của bố | `GheTua_AoBo_Fix` |  | 2,17 | 0,60 | 0,44×0,49×0,91 | 3,40 → 4,31 | ra đường |
| BM-04 | Giường đôi (bộ) | `GiuongDoi_Bo` |  | 6,46 | 1,70 | 1,87×2,27×1,80 | 3,40 → 5,20 | → phải |
| BM-05 | Thảm đỏ 1,2 m | `Tham_Do_1m2` |  | 3,50 | 2,40 | 0,92×1,37×0,01 | 3,40 → 3,41 | → phải |
| BM-06 | Bộ bàn trang điểm (+ gương) | `BanTrangDiem_Bo` |  | 0,21 | 2,45 | 0,90×0,42×1,61 | 3,40 → 5,01 | → phải |
| BM-07 | Đôn ngồi trang điểm | `DonTrangDiem` |  | 0,75 | 2,45 | 0,32×0,32×0,35 | 3,40 → 3,75 | trong |
| BM-08 | Tủ đầu giường | `TuDauGiuong` |  | 7,37 | 2,86 | 0,42×0,45×0,50 | 3,40 → 3,90 | ← trái |
| BM-09 | Kệ sách hồ sơ gỗ | `KeSachHoSo_Go` |  | 0,17 | 3,95 | 0,85×0,32×1,52 | 3,40 → 4,92 | → phải |
| BM-10 | Mắc áo đứng | `MacAoDung` |  | 0,25 | 4,95 | 0,34×0,34×1,62 | 3,41 → 5,03 | trong |
| BM-11 | Giỏ quần áo | `GioQuanAo` |  | 3,85 | 5,00 | 0,63×0,55×0,42 | 3,40 → 3,82 | 10° |
| BM-12 | Tủ quần áo gầm hở (két sắt dưới gầm) | `TuQuanAo_GamHo` |  | 1,20 | 5,12 | 1,26×0,60×1,94 | 3,40 → 5,34 | ra đường |
| BM-13 | Rổ nhựa quần áo bẩn | `RoNhua_QuanAoBan` |  | 4,95 | 5,10 | 0,54×0,53×0,54 | 3,40 → 3,94 | trong |
| BM-14 | Xí bệt | `XiBet_Tron` |  | 7,15 | 5,09 | 0,38×0,62×0,81 | 3,40 → 4,21 | ra đường |
| BM-15 | Két sắt khoá số | `KetSat_KhoaSo` |  | 1,18 | 5,15 | 0,40×0,38×0,24 | 3,40 → 3,64 | ra đường |
| BM-16 | Cọ bồn cầu | `ChoiCauBonCau` |  | 6,78 | 5,28 | 0,13×0,13×0,48 | 3,40 → 3,88 | trong |

### Đồ đặt trên bàn / kệ / giường (15)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| BM-17 | Nhật ký (đóng) | `NhatKy_Dong` |  | 1,30 | 0,30 | 0,16×0,22×0,02 | 4,15 → 4,17 | trong |
| BM-18 | Lọ thuốc nam của bố | `ThuocBo_LoThuocNam` |  | 7,38 | 0,50 | 0,30×0,11×0,14 | 3,90 → 4,04 | ← trái |
| BM-19 | Đồng hồ báo thức để bàn | `DongHoDeBan_BaoThuc` |  | 7,51 | 0,50 | 0,11×0,05×0,13 | 3,90 → 4,03 | trong |
| BM-20 | Đồ trên tủ đầu giường (bố) | `DoTuDauGiuong_Bo` |  | 6,50 | 0,95 | 0,24×0,30×0,06 | 3,87 → 3,93 | trong |
| BM-21 | Chăn mền bông gấp gọn | `ChanMenBong_GapGon` |  | 5,75 | 1,70 | 0,92×0,63×0,35 | 4,01 → 4,36 | → phải |
| BM-22 | Túi xách của mẹ (ví) | `TuiXachMe_ViTien` |  | 0,75 | 2,45 | 0,49×0,13×0,35 | 3,75 → 4,10 | 20° |
| BM-23 | Hộp kim chỉ của mẹ | `HopKimChi_Me` |  | 0,20 | 2,60 | 0,20×0,20×0,10 | 4,13 → 4,23 | trong |
| BM-24 | Đồ trên tủ đầu giường (mẹ) | `DoTuDauGiuong_Me` |  | 7,37 | 2,69 | 0,22×0,25×0,04 | 3,90 → 3,94 | ← trái |
| BM-25 | Điện thoại di động đổ chuông | `Nokia_DoChuong` |  | 7,31 | 2,85 | 0,16×0,13×0,02 | 4,17 → 4,20 | -160° |
| BM-26 | Móc áo + khăn tắm | `MocAo_KhanTam` |  | 6,42 | 3,65 | 0,58×0,10×0,41 | 4,63 → 5,04 | trong |
| BM-27 | Cốc bàn chải (người lớn) | `CocBanChai_NguoiLon` |  | 7,51 | 4,05 | 0,07×0,06×0,22 | 4,29 → 4,51 | trong |
| BM-28 | Kệ kính đồ dùng (WC bố mẹ) | `KeKinh_DoBoMe` |  | 7,55 | 4,10 | 0,42×0,04×0,11 | 4,29 → 4,40 | ← trái |
| BM-29 | Vải che gương — WC | `VaiChePhuGuong_WC` |  | 7,58 | 4,10 | 0,47×0,05×0,47 | 4,32 → 4,79 | ← trái |
| BM-30 | Cuộn giấy vệ sinh treo | `GiayVeSinh_Treo` |  | 7,52 | 5,05 | 0,12×0,17×0,16 | 4,00 → 4,16 | ← trái |
| BM-31 | Kệ xà phòng WC | `KeXaPhong_WC` |  | 6,40 | 5,33 | 0,50×0,14×0,18 | 4,69 → 4,87 | ra đường |

### Treo tường / treo trần (7)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| BM-32 | Tranh chữ "Phúc" (quốc ngữ) | `TranhThuPhap_Phuc` |  | 0,02 | 1,55 | 0,42×0,05×0,62 | 4,95 → 5,57 | → phải |
| BM-33 | Ảnh cưới bố mẹ (khung treo) | `AnhCuoi_Tex` |  | 7,59 | 1,70 | 0,50×0,02×0,70 | 5,05 → 5,75 | ← trái |
| BM-34 | Lavabo | `Lavabo` |  | 7,42 | 4,10 | 0,45×0,35×0,15 | 4,07 → 4,22 | ← trái |
| BM-35 | Kệ kính | `KeKinh` |  | 7,55 | 4,10 | 0,30×0,10×0,02 | 4,27 → 4,29 | ← trái |
| BM-36 | Gương WC | `GuongWC` |  | 7,59 | 4,10 | 0,40×0,02×0,40 | 4,35 → 4,75 | ← trái |
| BM-37 | Bình nóng lạnh | `BinhNongLanh` |  | 7,42 | 4,75 | 0,77×0,35×0,35 | 5,35 → 5,70 | ← trái |
| BM-38 | Áo mưa của bố treo cửa | `AoMuaBo_TreoCua` |  | 3,95 | 5,34 | 0,56×0,12×0,91 | 4,24 → 5,15 | ra đường |

### Điện – đèn – quạt (6)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| BM-39 | Đèn ốp trần | `DenOpTran` |  | 3,40 | 2,70 | 0,30×0,30×0,11 | 6,29 → 6,40 | trong |
| BM-40 | Đèn ngủ đầu giường bố mẹ | `DenNgu_BoMe` |  | 7,38 | 2,92 | 0,20×0,20×0,32 | 3,90 → 4,22 | trong |
| BM-41 | Quạt cây đứng | `QuatCayDung` |  | 0,55 | 3,15 | 0,49×0,48×1,32 | 3,41 → 4,73 | 109° |
| BM-42 | Ổ cắm đôi | `OCam_Doi` |  | 7,59 | 3,20 | 0,01×0,07×0,07 | 3,90 → 3,97 | ← trái |
| BM-43 | Quạt treo tường | `QuatTreoTuong_Fix` |  | 0,15 | 3,80 | 0,46×0,30×0,57 | 5,41 → 5,98 | → phải |
| BM-44 | Đèn tuýp máng 0,6 m | `DenTuyp_60` |  | 6,70 | 4,50 | 0,62×0,07×0,06 | 6,34 → 6,40 | → phải |

### Cửa, cổng, cửa sổ, rèm (22)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| BM-45 | Cửa sổ phòng bố mẹ (trái) | `CuaSo_BoMe_Trai` |  | 1,02 | −0,05 | 0,55×0,05×1,40 | 4,30 → 5,70 | ra đường |
| BM-46 | Cửa sổ phòng bố mẹ (phải) | `CuaSo_BoMe_Phai` |  | 1,57 | −0,05 | 0,55×0,05×1,40 | 4,30 → 5,70 | trong |
| BM-47 | Cửa sổ phòng bố mẹ (trái) | `CuaSo_BoMe_Trai` |  | 5,83 | −0,05 | 0,55×0,05×1,40 | 4,30 → 5,70 | ra đường |
| BM-48 | Cửa sổ phòng bố mẹ (phải) | `CuaSo_BoMe_Phai` |  | 6,38 | −0,05 | 0,55×0,05×1,40 | 4,30 → 5,70 | trong |
| BM-49 | Bậu cửa sổ granito — bố mẹ | `BauCuaSo_Granito_BoMe` |  | 1,30 | 0,01 | 1,10×0,08×0,03 | 4,27 → 4,30 | trong |
| BM-50 | Song sắt hoa cửa sổ bố mẹ | `SongSatHoa_BoMe` |  | 1,30 | 0,04 | 1,05×0,01×1,35 | 4,33 → 5,67 | trong |
| BM-51 | Bậu cửa sổ granito — bố mẹ | `BauCuaSo_Granito_BoMe` |  | 6,10 | 0,01 | 1,10×0,08×0,03 | 4,27 → 4,30 | trong |
| BM-52 | Song sắt hoa cửa sổ bố mẹ | `SongSatHoa_BoMe` |  | 6,11 | 0,04 | 1,05×0,01×1,35 | 4,33 → 5,67 | trong |
| BM-53 | Rèm voan — phòng bố mẹ | `Rem_Voan_BoMe` |  | 1,30 | 0,07 | 1,20×0,04×1,65 | 4,20 → 5,85 | trong |
| BM-54 | Thanh treo rèm | `ThanhRem` |  | 1,30 | 0,07 | 1,33×0,03×0,03 | 5,85 → 5,88 | trong |
| BM-55 | Thanh treo rèm | `ThanhRem` |  | 6,10 | 0,07 | 1,33×0,03×0,03 | 5,85 → 5,88 | trong |
| BM-56 | Rèm voan — phòng bố mẹ | `Rem_Voan_BoMe` |  | 6,10 | 0,07 | 1,20×0,04×1,65 | 4,20 → 5,85 | trong |
| BM-57 | Thanh treo rèm | `ThanhRem` |  | 1,30 | 0,16 | 1,33×0,03×0,03 | 5,85 → 5,88 | trong |
| BM-58 | Rèm vải — phòng bố mẹ | `Rem_Vai_BoMe_Buong` |  | 1,30 | 0,16 | 1,20×0,06×1,65 | 4,20 → 5,85 | trong |
| BM-59 | Thanh treo rèm | `ThanhRem` |  | 6,10 | 0,16 | 1,33×0,03×0,03 | 5,85 → 5,88 | trong |
| BM-60 | Rèm vải — phòng bố mẹ | `Rem_Vai_BoMe_Buong` |  | 6,10 | 0,16 | 1,20×0,06×1,65 | 4,20 → 5,85 | trong |
| BM-61 | Khung ảnh cưới nhỏ | `KhungAnh_CuoiNho` |  | 0,90 | 0,27 | 0,24×0,19×0,18 | 4,15 → 4,33 | 10° |
| BM-62 | Kính lão + hộp kính | `KinhLao_HopKinh` |  | 1,75 | 0,30 | 0,36×0,16×0,04 | 4,15 → 4,19 | 20° |
| BM-63 | Cánh cửa ban công (trái) | `CanhBanCong_Trai` |  | 3,20 | 0,33 | 0,60×0,05×2,20 | 3,40 → 5,60 | → phải |
| BM-64 | Cánh cửa ban công (phải) | `CanhBanCong_Phai` |  | 4,40 | 0,33 | 0,60×0,05×2,20 | 3,40 → 5,60 | → phải |
| BM-65 | Khung cửa WC bố mẹ | `KhungCuaWC_BoMe` |  | 5,75 | 4,25 | 0,80×0,06×1,95 | 3,40 → 5,35 | ← trái |
| BM-66 | Cửa WC phòng bố mẹ | `CuaWC_BoMe (mở)` |  | 6,10 | 4,60 | 0,70×0,10×1,90 | 3,40 → 5,30 | ra đường |

### Kết cấu phụ / cố định (4)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| BM-67 | Lan can sắt ban công | `LanCan_BanCong` |  | 1,70 | −1,18 | 3,80×0,04×1,02 | 3,48 → 4,50 | trong |
| BM-68 | Gờ bê tông ban công | `GoBanCong` |  | 1,75 | −1,18 | 3,90×0,14×0,10 | 3,38 → 3,48 | trong |
| BM-69 | Lan can sắt ban công | `LanCan_BanCong` |  | 5,50 | −1,18 | 3,80×0,04×1,02 | 3,48 → 4,50 | trong |
| BM-70 | Gờ bê tông ban công | `GoBanCong` |  | 5,55 | −1,18 | 3,90×0,14×0,10 | 3,38 → 3,48 | trong |

## Sảnh, cầu thang, hành lang tầng 2

<a id="s2"></a>Tầng 2 (+3,40) · phòng S2 · 32 vật

### Đồ đặt trên sàn (8)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| S2-01 | Tủ thấp sảnh | `TuThap_Sanh` |  | 0,19 | 6,22 | 0,80×0,36×0,65 | 3,40 → 4,04 | → phải |
| S2-02 | Chậu lưỡi hổ | `ChauLuoiHo` |  | 0,22 | 6,85 | 0,24×0,24×0,48 | 3,40 → 3,88 | trong |
| S2-03 | Chậu cây kiểng lá to | `ChauCayKieng_LaTo` |  | 2,73 | 11,60 | 0,74×0,78×1,17 | 3,40 → 4,57 | trong |
| S2-04 | Thảm / chiếu hoa | `Tham_Chieu_Hoa` |  | 1,55 | 12,30 | 0,92×1,37×0,01 | 3,40 → 3,41 | trong |
| S2-05 | Tủ đứng (rương chăn màn) | `TuDung_RuongChanMan` |  | 0,33 | 13,25 | 2,32×0,65×1,80 | 3,40 → 5,20 | → phải |
| S2-06 | Kệ giày dép gỗ | `KeGiayDep_Go` |  | 0,20 | 14,81 | 0,81×0,30×0,65 | 3,40 → 4,05 | → phải |
| S2-07 | Chồng thùng carton | `ThungCarton_Chong` |  | 0,35 | 15,82 | 1,09×0,54×0,97 | 3,40 → 4,37 | 80° |
| S2-08 | Kệ sách hồ sơ gỗ | `KeSachHoSo_Go` |  | 1,50 | 16,20 | 0,85×0,32×1,52 | 3,40 → 4,92 | ra đường |

### Đồ đặt trên bàn / kệ / giường (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| S2-09 | Vải che gương — sảnh | `VaiChePhuGuong_Sanh` |  | 0,02 | 6,22 | 0,52×0,05×0,64 | 4,56 → 5,20 | → phải |
| S2-10 | Móc áo + khăn tắm | `MocAo_KhanTam` |  | 3,05 | 15,42 | 0,59×0,10×0,41 | 4,63 → 5,04 | ← trái |

### Treo tường / treo trần (3)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| S2-11 | Gương treo sảnh | `GuongTreo_Sanh` |  | 0,01 | 6,22 | 0,45×0,00×0,55 | 4,60 → 5,15 | → phải |
| S2-12 | Tranh nhỏ hành lang | `TranhNho_HanhLang` |  | 3,10 | 11,20 | 0,22×0,00×0,17 | 4,93 → 5,10 | ← trái |
| S2-13 | Tranh sơn thuỷ (khung) | `TranhSonThuy_Khung` |  | 0,02 | 11,50 | 0,66×0,05×0,46 | 4,95 → 5,41 | → phải |

### Điện – đèn – quạt (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| S2-14 | Chuông điện (chuông) | `ChuongDien_Chuong` |  | 3,02 | 7,70 | 0,16×0,15×0,16 | 5,52 → 5,68 | ← trái |
| S2-15 | Bóng đèn compact | `BongCompact` |  | 2,50 | 13,00 | 0,04×0,03×0,13 | 6,27 → 6,40 | trong |

### Kết cấu phụ / cố định (17)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| S2-16 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 7,59 | 0,90×0,02×0,02 | 3,56 → 3,58 | trong |
| S2-17 | Trụ đầu thang gỗ tiện | `TruDauThang` |  | 1,87 | 7,60 | 0,14×0,14×0,95 | 3,40 → 4,35 | trong |
| S2-18 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 7,84 | 0,90×0,02×0,02 | 3,73 → 3,76 | trong |
| S2-19 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 7,86 | 0,90×0,02×0,02 | 6,40 → 6,42 | ra đường |
| S2-20 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 8,11 | 0,90×0,02×0,02 | 6,22 → 6,25 | ra đường |
| S2-21 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 8,09 | 0,90×0,02×0,02 | 3,91 → 3,93 | trong |
| S2-22 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 8,34 | 0,90×0,02×0,02 | 4,09 → 4,11 | trong |
| S2-23 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 8,36 | 0,90×0,02×0,02 | 6,04 → 6,07 | ra đường |
| S2-24 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 8,61 | 0,90×0,02×0,02 | 5,87 → 5,89 | ra đường |
| S2-25 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 8,59 | 0,90×0,02×0,02 | 4,27 → 4,29 | trong |
| S2-26 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 8,84 | 0,90×0,02×0,02 | 4,45 → 4,47 | trong |
| S2-27 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 8,86 | 0,90×0,02×0,02 | 5,69 → 5,71 | ra đường |
| S2-28 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 9,11 | 0,90×0,02×0,02 | 5,51 → 5,53 | ra đường |
| S2-29 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 9,09 | 0,90×0,02×0,02 | 4,62 → 4,65 | trong |
| S2-30 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 1,45 | 9,34 | 0,90×0,02×0,02 | 4,80 → 4,82 | trong |
| S2-31 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 9,36 | 0,90×0,02×0,02 | 5,33 → 5,36 | ra đường |
| S2-32 | Mũi bậc bo tròn (mòn) | `MuiBac_Mon` |  | 0,45 | 9,61 | 0,90×0,02×0,02 | 5,16 → 5,18 | ra đường |

## Góc làm việc (tầng 2)

<a id="lv"></a>Tầng 2 (+3,40) · phòng LV · 9 vật

### Đồ đặt trên sàn (3)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| LV-01 | Ghế bàn giấy | `GheBanGiay` |  | 5,90 | 6,30 | 0,44×0,46×0,85 | 3,40 → 4,25 | trong |
| LV-02 | Tủ hồ sơ sắt | `TuHoSo_Sat` |  | 7,38 | 6,30 | 1,20×0,48×1,70 | 3,40 → 5,10 | ← trái |
| LV-03 | Bàn giấy gỗ | `BanGiay_Go` |  | 5,90 | 6,95 | 1,40×0,71×0,75 | 3,40 → 4,15 | ra đường |

### Đồ đặt trên bàn / kệ / giường (4)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| LV-04 | Con dấu + hộp mực | `ConDau_HopMuc` |  | 5,64 | 6,75 | 0,19×0,10×0,09 | 4,15 → 4,24 | 30° |
| LV-05 | Bàn tính gỗ | `BanTinh_Go` |  | 5,85 | 6,85 | 0,30×0,18×0,06 | 4,15 → 4,21 | 175° |
| LV-06 | Máy tính bỏ túi | `MayTinhBoTui` |  | 5,45 | 6,90 | 0,12×0,18×0,02 | 4,15 → 4,17 | 20° |
| LV-07 | Chồng sổ sách cũ | `ChongSoSach_Cu` |  | 6,30 | 6,95 | 0,23×0,29×0,28 | 4,15 → 4,43 | trong |

### Cửa, cổng, cửa sổ, rèm (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| LV-08 | Cửa góc làm việc — cánh trái | `CuaLamViec_Trai (mở)` |  | 3,45 | 6,00 | 0,60×0,05×2,20 | 3,40 → 5,60 | ra đường |
| LV-09 | Cửa góc làm việc — cánh phải | `CuaLamViec_Phai (mở)` |  | 3,45 | 7,20 | 0,60×0,05×2,20 | 3,40 → 5,60 | ra đường |

## Phòng Nhím

<a id="nh"></a>Tầng 2 (+3,40) · phòng NH · 22 vật

### Đồ đặt trên sàn (11)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| NH-01 | Bàn cạnh giường (thuốc) | `BanCanhGiuong_Thuoc` |  | 6,55 | 8,75 | 0,40×0,40×0,72 | 3,40 → 4,12 | trong |
| NH-02 | Giường Nhím + chiếu + chăn | `GiuongNhim_ChieuChan` |  | 5,35 | 8,95 | 1,90×0,90×0,85 | 3,40 → 4,25 | trong |
| NH-03 | Búp bê nhựa cũ | `BupBe_NhuaCu` |  | 4,90 | 9,80 | 0,29×0,46×0,13 | 3,40 → 3,53 | 60° |
| NH-04 | Chậu nước + khăn ướt (Nhím) | `ChauNuoc_KhanUot` |  | 5,85 | 9,80 | 0,37×0,32×0,09 | 3,40 → 3,49 | trong |
| NH-05 | Đồ chơi lớn (Nhím) | `DoChoi_Lon` |  | 4,60 | 10,20 | 0,60×0,21×0,12 | 3,40 → 3,52 | trong |
| NH-06 | Sách truyện tranh | `SachTruyenTranh` |  | 4,90 | 10,35 | 0,52×0,42×0,02 | 3,40 → 3,42 | 30° |
| NH-07 | Tủ nhựa ghép của Nhím | `TuNhua_Nhim` |  | 7,40 | 11,00 | 0,45×0,39×1,18 | 3,40 → 4,58 | ← trái |
| NH-08 | Giường xếp (khách) | `GiuongXep_CoTam` |  | 3,85 | 11,30 | 1,06×1,80×0,46 | 3,40 → 3,85 | trong |
| NH-09 | Ghế nhựa của Nhím | `GheNhua_Nhim` |  | 5,00 | 11,80 | 0,26×0,23×0,26 | 3,40 → 3,66 | trong |
| NH-10 | Bàn học của Nhím | `BanHoc_Nhim` |  | 5,00 | 12,22 | 0,48×0,34×0,45 | 3,40 → 3,85 | ra đường |
| NH-11 | Cặp sách tiểu học | `CapSach_TieuHoc` |  | 4,30 | 12,64 | 0,30×0,25×0,42 | 3,40 → 3,81 | → phải |

### Đồ đặt trên bàn / kệ / giường (3)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| NH-12 | Băng bột tay gãy (Nhím) | `BangTayGay` |  | 5,30 | 9,00 | 0,58×0,25×0,08 | 3,73 → 3,81 | 15° |
| NH-13 | Gấu bông cũ | `GauBong_Cu` |  | 5,85 | 9,05 | 0,27×0,21×0,33 | 3,67 → 4,00 | -160° |
| NH-14 | Đồng phục Nhím treo móc | `DongPhuc_Treo` |  | 7,53 | 11,90 | 0,56×0,08×0,78 | 4,04 → 4,82 | ← trái |

### Treo tường / treo trần (5)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| NH-15 | Tranh sáp màu "cả nhà" của Nhím | `TranhSap_Nhim` |  | 5,25 | 8,51 | 0,26×0,00×0,20 | 4,50 → 4,70 | trong |
| NH-16 | Móc treo đồng phục | `MocDongPhuc` |  | 7,54 | 11,90 | 0,23×0,11×0,47 | 4,43 → 4,90 | ← trái |
| NH-17 | Hộp bút + thước kẻ | `HopBut_ThuocKe` |  | 4,91 | 12,23 | 0,47×0,14×0,06 | 4,04 → 4,11 | 10° |
| NH-18 | Đồ học trên bàn của Nhím | `DoHoc_Nhim` |  | 5,00 | 12,22 | 0,54×0,36×0,20 | 3,85 → 4,04 | ra đường |
| NH-19 | Bình nước học sinh | `BinhNuocHocSinh` |  | 5,12 | 12,30 | 0,11×0,14×0,23 | 4,04 → 4,28 | 30° |

### Điện – đèn – quạt (1)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| NH-20 | Đèn ngủ của Nhím | `DenNgu_Nhim` |  | 6,55 | 8,75 | 0,17×0,17×0,24 | 3,95 → 4,19 | trong |

### Cửa, cổng, cửa sổ, rèm (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| NH-21 | Ô thoáng bông gió | `OThoang_BongGio` |  | 3,15 | 9,40 | 0,80×0,10×0,30 | 5,55 → 5,85 | → phải |
| NH-22 | Cửa phòng Nhím (đủ bộ) | `CuaPhongNhim_FullAssembly` |  | 3,18 | 9,97 | 1,22×2,14×2,00 | 2,90 → 4,90 | ra đường |

## Phòng Khôi

<a id="kh"></a>Tầng 2 (+3,40) · phòng KH · 25 vật

### Đồ đặt trên sàn (8)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| KH-01 | Tủ áo của Khôi | `TuAo_Khoi` |  | 3,68 | 12,88 | 0,91×0,73×1,80 | 3,40 → 5,20 | trong |
| KH-02 | Giường đơn của Khôi | `GiuongDon_Khoi` |  | 7,11 | 13,52 | 0,95×2,00×0,60 | 3,40 → 4,00 | ra đường |
| KH-03 | Ghế bàn học gỗ | `GheBanHoc_Go` |  | 5,70 | 15,50 | 0,42×0,42×0,87 | 3,40 → 4,27 | trong |
| KH-04 | Hành lý của Khôi (túi vải) | `HanhLyKhoi_TuiVai` |  | 3,58 | 15,75 | 0,67×0,34×0,40 | 3,40 → 3,80 | 30° |
| KH-05 | Bộ bàn học của Khôi | `BanHoc_Khoi_Bo` |  | 5,70 | 16,10 | 1,10×0,56×0,73 | 3,40 → 4,13 | ra đường |
| KH-06 | Đàn guitar cũ | `Guitar` |  | 7,45 | 16,22 | 0,32×0,05×0,88 | 3,40 → 4,28 | ← trái |
| KH-07 | Ba lô của Khôi (xanh lá) | `BaloKhoi_XanhLa` | Đ2 | 6,43 | 12,80 | 0,30×0,28×0,55 | 3,40 → 3,95 | 20° |
| KH-08 | Ba lô của Khôi (xanh lá) | `BaloKhoi_XanhLa` | Đ3 | 6,43 | 12,80 | 0,30×0,28×0,55 | 3,40 → 3,95 | 20° |

### Đồ đặt trên bàn / kệ / giường (4)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| KH-09 | Tấm nilon phủ bụi | `TamNilon_PhuBui` |  | 7,11 | 13,52 | 0,98×2,00×0,08 | 3,86 → 3,94 | trong |
| KH-10 | Giấy báo nhập học | `GiayBaoNhapHoc` |  | 5,18 | 16,01 | 0,49×0,33×0,00 | 4,13 → 4,13 | 15° |
| KH-11 | Máy nghe băng + băng cũ | `Walkman_BangCu` |  | 6,10 | 16,00 | 0,45×0,17×0,06 | 4,13 → 4,19 | -170° |
| KH-12 | Đèn bàn học | `DenBanHoc` |  | 5,25 | 16,15 | 0,48×0,15×0,32 | 4,13 → 4,45 | ra đường |

### Treo tường / treo trần (4)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| KH-13 | Lịch tờ năm 2000 | `LichTo_2000` |  | 7,59 | 13,20 | 0,30×0,00×0,19 | 4,81 → 5,00 | ← trái |
| KH-14 | Poster ban nhạc (bịa) | `Poster_BanNhac` |  | 7,59 | 14,00 | 0,34×0,00×0,24 | 4,81 → 5,05 | ← trái |
| KH-15 | Giá sách (bộ) | `GiaSach_Bo` |  | 7,50 | 15,40 | 0,70×0,20×0,85 | 4,40 → 5,25 | ← trái |
| KH-16 | Giấy khen của Khôi | `GiayKhen_Khoi` |  | 4,40 | 16,39 | 0,24×0,00×0,17 | 4,93 → 5,10 | ra đường |

### Điện – đèn – quạt (1)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| KH-17 | Quả bóng đá xẹp | `BongDa_Xep` |  | 7,10 | 14,00 | 0,19×0,19×0,12 | 3,40 → 3,52 | trong |

### Cửa, cổng, cửa sổ, rèm (8)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| KH-18 | Rèm vải — phòng Khôi | `Rem_Vai_Khoi_Buong` |  | 5,70 | 16,24 | 1,30×0,06×1,65 | 4,20 → 5,85 | ra đường |
| KH-19 | Thanh treo rèm | `ThanhRem` |  | 5,70 | 16,24 | 1,33×0,03×0,03 | 5,85 → 5,88 | ra đường |
| KH-20 | Rèm voan — phòng Khôi | `Rem_Voan_Khoi` |  | 5,70 | 16,33 | 1,30×0,04×1,65 | 4,20 → 5,85 | ra đường |
| KH-21 | Thanh treo rèm | `ThanhRem` |  | 5,70 | 16,33 | 1,33×0,03×0,03 | 5,85 → 5,88 | ra đường |
| KH-22 | Cửa sổ phòng Khôi (phải) | `CuaSo_Khoi_Phai` |  | 5,40 | 16,45 | 0,60×0,05×1,40 | 4,30 → 5,70 | ra đường |
| KH-23 | Song sắt hoa cửa sổ Khôi | `SongSatHoa_Khoi` |  | 5,70 | 16,36 | 1,15×0,01×1,35 | 4,33 → 5,67 | ra đường |
| KH-24 | Bậu cửa sổ granito — Khôi | `BauCuaSo_Granito_Khoi` |  | 5,70 | 16,39 | 1,20×0,08×0,03 | 4,27 → 4,30 | ra đường |
| KH-25 | Cửa sổ phòng Khôi (trái) | `CuaSo_Khoi_Trai` |  | 6,00 | 16,45 | 0,60×0,05×1,40 | 4,30 → 5,70 | trong |

## Sân phơi + phòng thờ gia tiên (tầng 3)

<a id="spt"></a>Tầng 3 (+6,60) · phòng SP, PT · 57 vật

### Đồ đặt trên sàn (18)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| SP-01 | Chậu nhựa | `ChauNhua` |  | 1,75 | 0,50 | 0,41×0,53×0,27 | 6,60 → 6,87 | trong |
| SP-02 | Chậu cây sân phơi | `ChauCay_SanPhoi` |  | 5,90 | 0,50 | 0,80×0,32×0,39 | 6,60 → 6,99 | trong |
| SP-03 | Bể nước có nắp | `BeNuoc_Nap` |  | 0,85 | 0,68 | 0,91×0,74×0,82 | 6,60 → 7,42 | trong |
| SP-04 | Bồn hoa xây sân phơi | `BonHoa_SanPhoi` |  | 7,08 | 1,50 | 1,03×2,20×0,77 | 6,60 → 7,37 | trong |
| PT-01 | Sập gỗ thấp | `SapGo` |  | 1,00 | 4,50 | 1,40×0,90×0,36 | 6,60 → 6,96 | → phải |
| PT-02 | Chiếu cói phòng thờ | `ChieuCoi_PhongTho` |  | 3,80 | 4,75 | 1,60×1,20×0,01 | 6,60 → 6,61 | → phải |
| PT-03 | Ghế đẩu gỗ | `GheDau_Go` |  | 6,40 | 5,60 | 0,32×0,32×0,44 | 6,60 → 7,04 | -15° |
| PT-04 | Chậu cây kiểng lá to | `ChauCayKieng_LaTo` |  | 0,45 | 5,95 | 0,74×0,78×1,17 | 6,60 → 7,77 | trong |
| PT-05 | Bình hoa gốm lớn | `BinhHoaLon_Gom` |  | 1,90 | 6,21 | 0,38×0,38×0,80 | 6,60 → 7,40 | trong |
| PT-06 | Hương, nến dự trữ | `HuongNen_DuTru` |  | 2,60 | 6,15 | 0,41×0,11×0,30 | 6,60 → 6,90 | trong |
| PT-07 | Tủ đồ thờ | `TuDoTho_Fix` |  | 5,20 | 6,20 | 0,72×0,35×0,85 | 6,60 → 7,45 | ra đường |
| PT-08 | Bàn thờ gia tiên (cả bộ) | `BanThoGiaTien_Set` |  | 3,90 | 7,03 | 1,76×2,32×3,97 | 6,60 → 10,57 | ra đường |
| PT-09 | Kệ giày dép gỗ | `KeGiayDep_Go` |  | 7,43 | 7,00 | 0,81×0,30×0,66 | 6,60 → 7,25 | ← trái |
| PT-10 | Chậu cây kiểng lá to | `ChauCayKieng_LaTo` |  | 6,89 | 7,30 | 0,74×0,78×1,17 | 6,60 → 7,77 | trong |
| PT-11 | Hũ / bình ngâm rượu | `HuBinhNgamRuou` |  | 2,35 | 7,95 | 0,56×0,48×0,48 | 6,60 → 7,08 | trong |
| PT-12 | Giá phơi đồ xếp | `GiaPhoiDo_Xep` |  | 3,40 | 8,90 | 0,95×0,47×1,01 | 6,60 → 7,61 | → phải |
| PT-13 | Chồng thùng carton | `ThungCarton_Chong` |  | 3,67 | 10,45 | 1,09×0,54×0,97 | 6,60 → 7,57 | 8° |
| PT-14 | Rổ nhựa quần áo bẩn | `RoNhua_QuanAoBan` |  | 6,90 | 10,40 | 0,54×0,53×0,54 | 6,60 → 7,14 | trong |

### Đồ đặt trên bàn / kệ / giường (11)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| SP-05 | Áo tang phơi #3 | `AoTang_Phoi_03` |  | 3,81 | 1,73 | 0,50×0,16×0,94 | 7,31 → 8,25 | trong |
| SP-06 | Áo tang phơi #1 | `AoTang_Phoi_01` |  | 2,85 | 1,76 | 0,50×0,11×0,80 | 7,45 → 8,25 | trong |
| SP-07 | Áo tang phơi #2 | `AoTang_Phoi_02` |  | 3,33 | 1,84 | 0,52×0,11×0,71 | 7,55 → 8,25 | trong |
| SP-08 | Áo tang phơi #4 | `AoTang_Phoi_04` |  | 4,29 | 1,82 | 0,50×0,08×0,79 | 7,46 → 8,25 | trong |
| SP-09 | Áo tang phơi #5 | `AoTang_Phoi_05` |  | 4,77 | 1,75 | 0,50×0,12×0,77 | 7,49 → 8,25 | trong |
| PT-15 | Móc áo + khăn tắm | `MocAo_KhanTam` |  | 7,55 | 3,77 | 0,58×0,10×0,41 | 7,83 → 8,24 | ← trái |
| PT-16 | Áo tang gấp (trên sạp) | `AoTangGap` |  | 1,00 | 4,50 | 0,48×0,46×0,19 | 6,96 → 7,15 | trong |
| PT-17 | Cụm ảnh cũ treo tường | `CumAnhCu_TreoTuong` |  | 0,02 | 5,61 | 0,72×0,04×0,75 | 7,84 → 8,60 | → phải |
| PT-18 | Bình hoa bàn thờ | `BinhHoaTho` |  | 3,20 | 6,10 | 0,25×0,19×0,64 | 7,93 → 8,57 | trong |
| PT-19 | Mâm đồng hoa quả (thờ) | `MamDong_HoaQuaTho` |  | 5,20 | 6,17 | 0,47×0,47×0,12 | 7,45 → 7,56 | ra đường |
| PT-20 | Kệ gỗ treo tường | `KeGo_TreoTuong` |  | 7,50 | 6,30 | 0,80×0,20×0,67 | 7,98 → 8,65 | ← trái |

### Treo tường / treo trần (4)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| SP-10 | Ăng-ten TV chữ Y trên mái | `AntenTV_MaiNha` |  | 6,60 | 5,00 | 1,41×1,01×2,00 | 9,80 → 11,80 | trong |
| PT-21 | Tranh chữ "Phúc" (quốc ngữ) | `TranhThuPhap_Phuc` |  | 0,02 | 3,30 | 0,42×0,05×0,62 | 8,25 → 8,87 | → phải |
| PT-22 | Tranh hoa sen (khung) | `TranhHoaSen_Khung` |  | 7,58 | 4,60 | 0,66×0,05×0,46 | 8,25 → 8,71 | ← trái |
| PT-23 | Lịch âm treo tường | `LichAm_TreoTuong` |  | 5,40 | 6,39 | 0,28×0,02×0,44 | 8,06 → 8,50 | ra đường |

### Điện – đèn – quạt (3)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| SP-11 | Bóng đèn chao tôn sân phơi + dây điện | `BongDenSan_DayDien` |  | 6,30 | 2,73 | 0,41×0,54×0,71 | 8,31 → 9,03 | ra đường |
| PT-24 | Đèn tuýp máng 1,2 m | `DenTuyp_120` |  | 2,20 | 4,70 | 1,22×0,07×0,06 | 9,54 → 9,60 | trong |
| PT-25 | Đèn tuýp máng 1,2 m | `DenTuyp_120` |  | 5,40 | 4,70 | 1,22×0,07×0,06 | 9,54 → 9,60 | trong |

### Cửa, cổng, cửa sổ, rèm (16)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| PT-26 | Cửa ra sân phơi (hé 0,25 m) | `CuaSanPhoi` |  | 3,80 | 1,25 | 1,02×0,33×2,16 | 6,60 → 8,76 | ra đường |
| PT-27 | Cửa ra sân phơi (hé 0,25 m) | `CuaSanPhoi` |  | 0,44 | 1,91 | 1,02×0,33×2,16 | −0,00 → 2,16 | trong |
| PT-28 | Cửa sổ phòng Khôi (trái) | `CuaSo_Khoi_Trai` |  | 1,10 | 3,05 | 0,60×0,05×1,40 | 7,50 → 8,90 | ra đường |
| PT-29 | Cửa sổ phòng Khôi (phải) | `CuaSo_Khoi_Phai` |  | 1,70 | 3,05 | 0,60×0,05×1,40 | 7,50 → 8,90 | trong |
| PT-30 | Cửa sổ phòng Khôi (trái) | `CuaSo_Khoi_Trai` |  | 5,90 | 3,05 | 0,60×0,05×1,40 | 7,50 → 8,90 | ra đường |
| PT-31 | Cửa sổ phòng Khôi (phải) | `CuaSo_Khoi_Phai` |  | 6,50 | 3,05 | 0,60×0,05×1,40 | 7,50 → 8,90 | trong |
| PT-32 | Bậu cửa sổ granito — Khôi | `BauCuaSo_Granito_Khoi` |  | 1,40 | 3,11 | 1,20×0,08×0,03 | 7,47 → 7,50 | trong |
| PT-33 | Song sắt hoa cửa sổ Khôi | `SongSatHoa_Khoi` |  | 1,41 | 3,14 | 1,15×0,01×1,35 | 7,53 → 8,87 | trong |
| PT-34 | Bậu cửa sổ granito — Khôi | `BauCuaSo_Granito_Khoi` |  | 6,20 | 3,11 | 1,20×0,08×0,03 | 7,47 → 7,50 | trong |
| PT-35 | Song sắt hoa cửa sổ Khôi | `SongSatHoa_Khoi` |  | 6,21 | 3,14 | 1,15×0,01×1,35 | 7,53 → 8,87 | trong |
| PT-36 | Rèm voan trắng — phòng thờ | `Rem_Voan_PhongTho` |  | 1,40 | 3,17 | 1,30×0,04×1,65 | 7,40 → 9,05 | trong |
| PT-37 | Thanh treo rèm | `ThanhRem` |  | 1,40 | 3,17 | 1,33×0,03×0,03 | 9,05 → 9,08 | trong |
| PT-38 | Thanh treo rèm | `ThanhRem` |  | 6,20 | 3,17 | 1,33×0,03×0,03 | 9,05 → 9,08 | trong |
| PT-39 | Rèm voan trắng — phòng thờ | `Rem_Voan_PhongTho` |  | 6,20 | 3,17 | 1,30×0,04×1,65 | 7,40 → 9,05 | trong |
| PT-40 | Thanh treo rèm | `ThanhRem` |  | 1,40 | 3,26 | 1,33×0,03×0,03 | 9,05 → 9,08 | trong |
| PT-41 | Thanh treo rèm | `ThanhRem` |  | 6,20 | 3,26 | 1,33×0,03×0,03 | 9,05 → 9,08 | trong |

### Kết cấu phụ / cố định (5)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| SP-12 | Lan can sân phơi (mô-đun) | `LanCanSanPhoi` |  | 3,80 | −0,10 | 2,95×0,05×1,10 | 6,70 → 7,80 | trong |
| SP-13 | Nền sân phơi | `NenSanPhoi` |  | 3,80 | 1,40 | 3,02×2,20×0,05 | 6,60 → 6,65 | trong |
| SP-14 | Kẹp phơi | `KepPhoi` |  | 3,80 | 1,80 | 2,38×0,52×0,12 | 8,20 → 8,32 | trong |
| SP-15 | Dây phơi | `DayPhoi` |  | 3,80 | 1,80 | 2,60×0,51×0,06 | 8,19 → 8,25 | trong |
| SP-16 | Cột phơi | `CotPhoi` |  | 3,80 | 1,80 | 2,76×0,56×1,80 | 6,64 → 8,44 | trong |

## Sảnh + góc kho mở (tầng 3)

<a id="gk"></a>Tầng 3 (+6,60) · phòng GK · 7 vật

### Đồ đặt trên sàn (4)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| GK-01 | Góc kho: đồ Tết (mai giả, đèn ông sao) | `GocKho_DoTet_Fix` |  | 7,41 | 8,20 | 0,79×0,38×0,68 | 6,60 → 7,28 | ← trái |
| GK-02 | Góc kho: đồ giỗ (nồi đồng, thùng, chiếu) | `GocKho_DoGio_Fix` |  | 7,37 | 9,80 | 1,67×0,43×0,29 | 6,60 → 6,89 | ← trái |
| GK-03 | Chậu cây kiểng lá to | `ChauCayKieng_LaTo` |  | 2,70 | 10,40 | 0,74×0,78×1,17 | 6,60 → 7,77 | trong |
| GK-04 | Góc kho: quạt gãy, chiếu cuộn, mâm nhôm | `GocKho_DoNha_Fix` |  | 4,95 | 10,60 | 1,58×0,38×1,06 | 6,60 → 7,66 | ra đường |

### Điện – đèn – quạt (1)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| GK-05 | Bóng đèn compact | `BongCompact` |  | 4,00 | 8,60 | 0,04×0,03×0,13 | 9,47 → 9,60 | trong |

### Kết cấu phụ / cố định (2)

| Mã | Tên | Tên Unity | Đêm | Tâm X | Tâm Z | Rộng×Sâu×Cao | Y đáy → đỉnh | Hướng |
|---|---|---|---|---|---|---|---|---|
| GK-06 | Giếng trời (mái lấy sáng) | `GiengTroi` |  | 0,95 | 9,20 | 1,95×3,25×0,26 | 9,60 → 9,86 | trong |
| GK-07 | Bồn nước mái | `BonNuocMai` |  | 5,30 | 13,50 | 1,47×0,96×1,56 | 6,60 → 8,16 | trong |

## Phụ lục A — công tắc & ổ cắm

Mỗi cụm = mặt nhựa + phím + đèn neon + biểu tượng (LocCongTac). Tất cả đặt ở tâm cao 1,45 m (1,33 → 1,57).

| Mã | Cụm | Tầng | Tâm X | Tâm Z | Y đáy → đỉnh | Gắn trên tường | Số khối |
|---|---|---|---|---|---|---|---|
| CT01 | CongTac_Hien | Tầng 1 (±0,00) | 2,10 | 0,02 | 1,33 → 1,57 | vuông góc trục Z (tường trước/sau) | 12 |
| CT02 | CongTac_Ham | Tầng 1 (±0,00) | 7,58 | 6,90 | 1,33 → 1,57 | vuông góc trục X (tường trái/phải) | 7 |
| CT03 | CongTac_PhongKhach | Tầng 1 (±0,00) | 5,30 | 7,38 | 1,33 → 1,57 | vuông góc trục Z (tường trước/sau) | 22 |
| CT04 | CongTac_SanhSau | Tầng 1 (±0,00) | 4,75 | 10,88 | 1,33 → 1,57 | vuông góc trục Z (tường trước/sau) | 12 |
| CT05 | CongTac_Bep_Cua | Tầng 1 (±0,00) | 4,75 | 11,02 | 1,33 → 1,57 | vuông góc trục Z (tường trước/sau) | 12 |
| CT06 | CongTac_Bep_GiengTroi | Tầng 1 (±0,00) | 3,10 | 16,38 | 1,33 → 1,57 | vuông góc trục Z (tường trước/sau) | 7 |
| CT07 | CongTac_WC_T1 | Tầng 1 (±0,00) | 4,58 | 21,30 | 1,33 → 1,57 | vuông góc trục X (tường trái/phải) | 7 |
| CT08 | CongTac_BoMe | Tầng 2 (+3,40) | 3,25 | 5,38 | 4,73 → 4,97 | vuông góc trục Z (tường trước/sau) | 12 |
| CT09 | CongTac_PhongNhim | Tầng 2 (+3,40) | 3,22 | 10,15 | 4,73 → 4,97 | vuông góc trục X (tường trái/phải) | 7 |
| CT10 | CongTac_HanhLang_ChieuNghi | Tầng 2 (+3,40) | 3,08 | 10,40 | 4,73 → 4,97 | vuông góc trục X (tường trái/phải) | 12 |
| CT11 | CongTac_HanhLang_Khoi | Tầng 2 (+3,40) | 3,08 | 13,00 | 4,73 → 4,97 | vuông góc trục X (tường trái/phải) | 7 |
| CT12 | CongTac_PhongKhoi | Tầng 2 (+3,40) | 3,22 | 14,45 | 4,73 → 4,97 | vuông góc trục X (tường trái/phải) | 12 |
| CT13 | CongTac_PhongTho | Tầng 3 (+6,60) | 7,05 | 6,38 | 7,93 → 8,17 | vuông góc trục Z (tường trước/sau) | 12 |
| CT14 | CongTac_Sanh_T3 | Tầng 3 (+6,60) | 5,45 | 6,52 | 7,93 → 8,17 | vuông góc trục Z (tường trước/sau) | 7 |

## Phụ lục B — nguồn sáng

Point Light của Unity, giá trị như trong scene (chưa tính ánh sáng bake).

| Mã | Tên | Tầng | X | Z | Y | Màu | Tầm (m) | Cường độ |
|---|---|---|---|---|---|---|---|---|
| L01 | Den_BongDayToc_Ham | Hầm (−2,30) | 6,40 | 11,90 | −0,50 | #FF9A3C | 4,0 | 1,00 |
| L02 | Den_Rap_Trai | Tầng 1 (±0,00) | 1,90 | −2,60 | 2,40 | #FFD27A | 6,0 | 2,00 |
| L03 | Den_Rap_Phai | Tầng 1 (±0,00) | 5,70 | −2,60 | 2,40 | #FFD27A | 6,0 | 2,00 |
| L04 | Den_PhongKhach_Tuyp | Tầng 1 (±0,00) | 3,00 | 1,00 | 3,00 | #DDEBFF | 7,0 | 1,10 |
| L05 | Den_NenDien_BanTho | Tầng 1 (±0,00) | 0,80 | 3,70 | 1,15 | #FF5A3C | 1,8 | 0,80 |
| L06 | Den_PhongKhach_Chum | Tầng 1 (±0,00) | 3,00 | 3,70 | 2,45 | #FFD9A0 | 7,0 | 1,20 |
| L07 | Den_Tiem_Kho | Tầng 1 (±0,00) | −2,30 | 4,30 | 3,20 | #FFE9C8 | 5,0 | 0,50 |
| L08 | Den_PhongKhach_Sau | Tầng 1 (±0,00) | 4,50 | 6,40 | 3,00 | #DDEBFF | 5,0 | 0,60 |
| L09 | Den_SanhSau | Tầng 1 (±0,00) | 4,10 | 9,20 | 3,00 | #FFF1D6 | 5,0 | 0,70 |
| L10 | Den_ChieuNghi_T1 | Tầng 1 (±0,00) | 0,95 | 10,30 | 2,90 | #FFF1D6 | 4,0 | 0,60 |
| L11 | Den_Bep | Tầng 1 (±0,00) | 3,80 | 13,70 | 3,00 | #DDEBFF | 7,0 | 1,20 |
| L12 | Den_GiengTroi | Tầng 1 (±0,00) | 0,20 | 17,80 | 2,60 | #FFE9B8 | 5,0 | 0,50 |
| L13 | Den_WC_T1 | Tầng 1 (±0,00) | 6,10 | 20,50 | 3,00 | #FFF1D6 | 3,0 | 0,40 |
| L14 | Den_Kho | Tầng 1 (±0,00) | 1,80 | 21,90 | 3,00 | #FFF1D6 | 5,0 | 0,50 |
| L15 | Den_Giat | Tầng 1 (±0,00) | 5,60 | 23,30 | 3,00 | #FFF1D6 | 4,0 | 0,40 |
| L16 | Den_Tiem_BanHang | Tầng 2 (+3,40) | −2,30 | −2,00 | 3,35 | #DDEBFF | 6,0 | 0,90 |
| L17 | Den_BoMe_OpTran | Tầng 2 (+3,40) | 3,40 | 2,70 | 6,30 | #FFE3B0 | 7,0 | 1,00 |
| L18 | Den_Sanh_T2 | Tầng 2 (+3,40) | 1,50 | 6,50 | 6,30 | #FFE3B0 | 5,0 | 0,70 |
| L19 | Den_LamViec | Tầng 2 (+3,40) | 5,40 | 6,90 | 6,30 | #FFE3B0 | 4,0 | 0,40 |
| L20 | Den_DenNgu_Nhim | Tầng 2 (+3,40) | 6,55 | 8,75 | 4,35 | #FFB36B | 4,5 | 0,90 |
| L21 | Den_ChieuNghi_T2 | Tầng 2 (+3,40) | 0,95 | 10,20 | 6,20 | #FFF1D6 | 4,0 | 0,60 |
| L22 | Den_HanhLang_T2 | Tầng 2 (+3,40) | 2,50 | 13,00 | 6,30 | #FFE3B0 | 5,0 | 0,60 |
| L23 | Den_Khoi_Tran | Tầng 2 (+3,40) | 5,40 | 14,40 | 6,30 | #FFE3B0 | 5,0 | 0,35 |
| L24 | Den_DenBan_Khoi | Tầng 2 (+3,40) | 5,30 | 15,90 | 4,60 | #FFD08A | 3,5 | 0,90 |
| L25 | Den_SanPhoi | Tầng 3 (+6,60) | 0,20 | 2,70 | 8,60 | #FFE9B8 | 5,0 | 0,70 |
| L26 | Den_PhongTho_1 | Tầng 3 (+6,60) | 2,20 | 4,70 | 9,40 | #FFF6E0 | 6,0 | 1,30 |
| L27 | Den_PhongTho_2 | Tầng 3 (+6,60) | 5,40 | 4,70 | 9,40 | #FFF6E0 | 6,0 | 1,30 |
| L28 | Den_NenDien_GiaTien | Tầng 3 (+6,60) | 3,80 | 5,60 | 7,90 | #FF5A3C | 1,5 | 0,60 |
| L29 | Den_Sanh_T3 | Tầng 3 (+6,60) | 4,00 | 8,60 | 9,50 | #FFF6E0 | 6,0 | 0,90 |

## Phụ lục C — decal dán bề mặt

| STT | Tên decal | Tầng | Tâm X | Tâm Z | Cao độ Y | Rộng × Cao (m) | Nhóm trong scene |
|---|---|---|---|---|---|---|---|
| 1 | T_BienHieu_BC | Tầng 1 (±0,00) | −2,30 | −5,46 | 2,93 | 3,52 × 1,00 | Tiem/VoNha |
| 2 | ToGiay_NghiBan | Tầng 1 (±0,00) | −2,30 | −5,33 | 1,50 | 1,00 × 1,00 | Tiem/VoNha |
| 3 | ChanTuong_Ban | Tầng 1 (±0,00) | 0,00 | 0,90 | 0,20 | 1,60 × 1,00 | Tang_1/PhongKhach |
| 4 | VetGheCoTuong | Tầng 1 (±0,00) | 7,60 | 1,95 | 0,55 | 1,50 × 1,00 | Tang_1/PhongKhach |
| 5 | KhoiAmTran | Tầng 1 (±0,00) | 0,90 | 3,70 | 3,20 | 1,60 × 1,00 | Tang_1/PhongKhach |
| 6 | ChanTuong_Ban | Tầng 1 (±0,00) | 7,60 | 5,50 | 0,20 | 1,60 × 1,00 | Tang_1/PhongKhach |
| 7 | VetTayCongTac | Tầng 1 (±0,00) | 7,60 | 6,90 | 1,56 | 1,00 × 1,00 | Ham |
| 8 | VetTayCongTac | Tầng 1 (±0,00) | 5,30 | 7,39 | 1,56 | 1,00 × 1,00 | Tang_1/PhongKhach |
| 9 | BacThang_Mon | Tầng 1 (±0,00) | 0,45 | 7,72 | 3,23 | 1,00 × 1,00 | Tang_1/VoNha |
| 10 | BacThang_Mon | Tầng 1 (±0,00) | 1,45 | 7,72 | 0,17 | 1,00 × 1,00 | Tang_1/VoNha |
| 11 | BacThang_Mon | Tầng 1 (±0,00) | 0,45 | 7,97 | 3,06 | 1,00 × 1,00 | Tang_1/VoNha |
| 12 | BacThang_Mon | Tầng 1 (±0,00) | 1,45 | 7,97 | 0,34 | 1,00 × 1,00 | Tang_1/VoNha |
| 13 | BacThang_Mon | Tầng 1 (±0,00) | 0,45 | 8,22 | 2,89 | 1,00 × 1,00 | Tang_1/VoNha |
| 14 | BacThang_Mon | Tầng 1 (±0,00) | 1,45 | 8,22 | 0,51 | 1,00 × 1,00 | Tang_1/VoNha |
| 15 | BacThang_Mon | Tầng 1 (±0,00) | 0,45 | 8,47 | 2,72 | 1,00 × 1,00 | Tang_1/VoNha |
| 16 | BacThang_Mon | Tầng 1 (±0,00) | 1,45 | 8,47 | 0,68 | 1,00 × 1,00 | Tang_1/VoNha |
| 17 | BacThang_Mon | Tầng 1 (±0,00) | 0,45 | 8,72 | 2,55 | 1,00 × 1,00 | Tang_1/VoNha |
| 18 | BacThang_Mon | Tầng 1 (±0,00) | 1,45 | 8,72 | 0,85 | 1,00 × 1,00 | Tang_1/VoNha |
| 19 | BacThang_Mon | Tầng 1 (±0,00) | 0,45 | 8,97 | 2,38 | 1,00 × 1,00 | Tang_1/VoNha |
| 20 | BacThang_Mon | Tầng 1 (±0,00) | 1,45 | 8,97 | 1,02 | 1,00 × 1,00 | Tang_1/VoNha |
| 21 | BacThang_Mon | Tầng 1 (±0,00) | 0,45 | 9,22 | 2,21 | 1,00 × 1,00 | Tang_1/VoNha |
| 22 | BacThang_Mon | Tầng 1 (±0,00) | 1,45 | 9,22 | 1,19 | 1,00 × 1,00 | Tang_1/VoNha |
| 23 | BacThang_Mon | Tầng 1 (±0,00) | 0,45 | 9,47 | 2,04 | 1,00 × 1,00 | Tang_1/VoNha |
| 24 | BacThang_Mon | Tầng 1 (±0,00) | 1,45 | 9,47 | 1,36 | 1,00 × 1,00 | Tang_1/VoNha |
| 25 | BacThang_Mon | Tầng 1 (±0,00) | 0,45 | 9,72 | 1,87 | 1,00 × 1,00 | Tang_1/VoNha |
| 26 | BacThang_Mon | Tầng 1 (±0,00) | 1,45 | 9,72 | 1,53 | 1,00 × 1,00 | Tang_1/VoNha |
| 27 | VetNuocTran | Tầng 1 (±0,00) | 6,60 | 12,40 | 3,20 | 1,00 × 1,00 | Tang_1/Bep |
| 28 | VetNuoc_San | Tầng 1 (±0,00) | 0,90 | 15,30 | 0,00 | 1,00 × 1,00 | Tang_1/Bep |
| 29 | ChanTuong_Ban | Tầng 1 (±0,00) | 7,60 | 15,30 | 0,20 | 1,60 × 1,00 | Tang_1/Bep |
| 30 | VetTayCongTac | Tầng 1 (±0,00) | 3,10 | 16,39 | 1,56 | 1,00 × 1,00 | Tang_1/Bep |
| 31 | NamMoc | Tầng 1 (±0,00) | 3,00 | 16,50 | 2,70 | 1,00 × 1,00 | Tang_1/GiengTroi |
| 32 | MangNhen | Tầng 1 (±0,00) | 0,00 | 19,25 | 2,90 | 1,00 × 1,00 | Tang_1/KhoiSau |
| 33 | NamMoc | Tầng 1 (±0,00) | 4,71 | 20,30 | 2,48 | 1,00 × 1,00 | Tang_1/KhoiSau |
| 34 | VetNuoc_San | Tầng 1 (±0,00) | 6,60 | 21,40 | 0,00 | 1,00 × 1,00 | Tang_1/KhoiSau |
| 35 | NamMoc | Tầng 1 (±0,00) | 6,20 | 21,80 | 2,65 | 1,00 × 1,00 | Tang_1/KhoiSau |
| 36 | NamMoc | Tầng 1 (±0,00) | 3,60 | 22,30 | 2,65 | 1,00 × 1,00 | Tang_1/KhoiSau |
| 37 | NamMoc | Tầng 1 (±0,00) | 0,01 | 23,00 | 2,70 | 1,00 × 1,00 | Tang_1/KhoiSau |
| 38 | VetAm_Ham | Hầm (−2,30) | 5,20 | 12,00 | −1,02 | 1,20 × 1,00 | Ham |
| 39 | BacThang_Mon | Tầng 2 (+3,40) | 0,45 | 7,72 | 6,42 | 1,00 × 1,00 | Tang_2/VoNha |
| 40 | BacThang_Mon | Tầng 2 (+3,40) | 1,45 | 7,72 | 3,58 | 1,00 × 1,00 | Tang_2/VoNha |
| 41 | BacThang_Mon | Tầng 2 (+3,40) | 0,45 | 7,97 | 6,25 | 1,00 × 1,00 | Tang_2/VoNha |
| 42 | BacThang_Mon | Tầng 2 (+3,40) | 1,45 | 7,97 | 3,76 | 1,00 × 1,00 | Tang_2/VoNha |
| 43 | VetTreoAnh | Tầng 2 (+3,40) | 3,10 | 8,05 | 5,03 | 1,00 × 1,00 | Tang_2/SanhHanhLang |
| 44 | BacThang_Mon | Tầng 2 (+3,40) | 0,45 | 8,22 | 6,07 | 1,00 × 1,00 | Tang_2/VoNha |
| 45 | BacThang_Mon | Tầng 2 (+3,40) | 1,45 | 8,22 | 3,94 | 1,00 × 1,00 | Tang_2/VoNha |
| 46 | BacThang_Mon | Tầng 2 (+3,40) | 0,45 | 8,47 | 5,89 | 1,00 × 1,00 | Tang_2/VoNha |
| 47 | BacThang_Mon | Tầng 2 (+3,40) | 1,45 | 8,47 | 4,11 | 1,00 × 1,00 | Tang_2/VoNha |
| 48 | BacThang_Mon | Tầng 2 (+3,40) | 0,45 | 8,72 | 5,71 | 1,00 × 1,00 | Tang_2/VoNha |
| 49 | BacThang_Mon | Tầng 2 (+3,40) | 1,45 | 8,72 | 4,29 | 1,00 × 1,00 | Tang_2/VoNha |
| 50 | BacThang_Mon | Tầng 2 (+3,40) | 0,45 | 8,97 | 5,54 | 1,00 × 1,00 | Tang_2/VoNha |
| 51 | BacThang_Mon | Tầng 2 (+3,40) | 1,45 | 8,97 | 4,47 | 1,00 × 1,00 | Tang_2/VoNha |
| 52 | BacThang_Mon | Tầng 2 (+3,40) | 0,45 | 9,22 | 5,36 | 1,00 × 1,00 | Tang_2/VoNha |
| 53 | BacThang_Mon | Tầng 2 (+3,40) | 1,45 | 9,22 | 4,65 | 1,00 × 1,00 | Tang_2/VoNha |
| 54 | BacThang_Mon | Tầng 2 (+3,40) | 0,45 | 9,47 | 5,18 | 1,00 × 1,00 | Tang_2/VoNha |
| 55 | BacThang_Mon | Tầng 2 (+3,40) | 1,45 | 9,47 | 4,83 | 1,00 × 1,00 | Tang_2/VoNha |
| 56 | TranhSap_2 | Tầng 2 (+3,40) | 7,60 | 9,55 | 4,59 | 1,00 × 1,00 | Tang_2/PhongNhim |
| 57 | VachChieuCao | Tầng 2 (+3,40) | 3,21 | 9,98 | 4,33 | 1,00 × 1,00 | Tang_2/PhongNhim |
| 58 | VetButSap | Tầng 2 (+3,40) | 7,59 | 10,00 | 3,83 | 1,00 × 1,00 | Tang_2/PhongNhim |
| 59 | Sticker_TuNhua | Tầng 2 (+3,40) | 7,20 | 11,00 | 4,15 | 1,00 × 1,00 | Tang_2/PhongNhim |
| 60 | MangNhen | Tầng 2 (+3,40) | 3,20 | 12,55 | 6,25 | 1,00 × 1,00 | Tang_2/PhongKhoi |
| 61 | VetNuocTran | Tầng 2 (+3,40) | 2,00 | 12,60 | 6,38 | 1,10 × 1,00 | Tang_2/SanhHanhLang |
| 62 | NamMoc | Tầng 2 (+3,40) | 3,21 | 15,90 | 5,85 | 1,00 × 1,00 | Tang_2/PhongKhoi |
| 63 | VetNuocTran | Tầng 3 (+6,60) | 6,80 | 4,40 | 9,60 | 1,10 × 1,00 | Tang_3/PhongTho |
| 64 | KhoiAmTran | Tầng 3 (+6,60) | 3,80 | 6,00 | 9,60 | 2,00 × 1,00 | Tang_3/PhongTho |

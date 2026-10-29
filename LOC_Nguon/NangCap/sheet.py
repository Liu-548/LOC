import sys,os,json
from PIL import Image, ImageDraw
out, names, dst = sys.argv[1], sys.argv[2].split(','), sys.argv[3]
tiles=[]
for n in names:
    fs=[f'{out}/_xem/{n}_{i}.png' for i in range(3)]
    if not os.path.exists(fs[0]): continue
    g=f'/home/claude/nc/goc/{n}_0.png'
    ims=[Image.open(g).resize((210,210)) if os.path.exists(g) else Image.new('RGB',(210,210))]+[Image.open(f).resize((210,210)) for f in fs[:2]]
    t=Image.new('RGB',(630,226),(30,30,30))
    for i,im in enumerate(ims): t.paste(im,(i*210,0))
    ImageDraw.Draw(t).text((4,212),n,fill=(255,255,0)); tiles.append(t)
cols=2; rows=(len(tiles)+1)//2
s=Image.new('RGB',(630*cols,226*rows),(20,20,20))
for i,t in enumerate(tiles): s.paste(t,((i%cols)*630,(i//cols)*226))
s.save(dst)

import json, os, re, math, collections, sys
sys.path.insert(0,os.path.expanduser('~/work'))
from glbb import bounds
raw=json.load(open(os.path.expanduser('~/work/scene_raw.json')))
boxes=json.load(open(os.path.expanduser('~/work/scene_boxes.json')))
lights=json.load(open(os.path.expanduser('~/work/scene_lights.json')))
g2p=json.load(open(os.path.expanduser('~/work/guid2path.json')))
def rot(x,z,yaw):
    t=math.radians(yaw); c,s=math.cos(t),math.sin(t)
    return x*c+z*s, -x*s+z*c
def obb(cx,cz,lx0,lx1,lz0,lz1,yaw):
    pts=[]
    for (a,b) in ((lx0,lz0),(lx1,lz0),(lx1,lz1),(lx0,lz1)):
        r=rot(a,b,yaw); pts.append([round(cx+r[0],4),round(cz+r[1],4)])
    return pts
cache={}
def gb(p):
    if p not in cache: cache[p]=bounds(p)
    return cache[p]
NIGHT=re.compile(r'^Dem([123])$')
def parse_path(p):
    seg=p.split('/')[1:]
    return seg
def group_room(seg):
    top=seg[0] if seg else ''
    if top=='Tang_1': g=seg[1] if len(seg)>1 else ''; return ('T1',{'PhongKhach':'PK','SanhSau':'SS','Bep':'BA','GiengTroi':'GT','KhoiSau':'KS'}.get(g))
    if top=='Tang_2': g=seg[1] if len(seg)>1 else ''; return ('T2',{'PhongBoMe':'BM','SanhHanhLang':'S2','PhongNhim':'NH','PhongKhoi':'KH','GocLamViec':'LV'}.get(g))
    if top=='Tang_3': g=seg[1] if len(seg)>1 else ''; return ('T3',{'PhongTho':'PT','SanPhoi':'SP','SanhGocKho':'GK'}.get(g))
    if top=='Tiem': g=seg[1] if len(seg)>1 else ''; return ('T1',{'BanHang':'TB','Kho':'TK'}.get(g))
    if top=='SanTruoc': return ('T1','ST')
    if top=='Ham': return ('H','H')
    return (None,None)
def by_pos(level,x,z):
    if level=='T1':
        if -4.6<=x<-0.2: return 'TB' if z<1.85 else 'TK'
        if z<-0.2: return 'ST'
        for lim,r in ((7.4,'PK'),(11.0,'SS'),(16.5,'BA'),(19.2,'GT'),(99,'KS')):
            if z<lim: return r
    if level=='T2':
        if z<5.5: return 'BM'
        if x<3.2: return 'S2'
        if z<8.45: return 'LV'
        if z<12.45: return 'NH'
        return 'KH'
    if level=='T3':
        if z<3.05: return 'SP'
        if z<6.45: return 'PT'
        return 'GK'
    return None
def level_of_y(y):
    if y<-0.3: return 'H'
    if y<3.3: return 'T1'
    if y<6.5: return 'T2'
    return 'T3'
units={}
def add_part(uid, uname, kieu, path, part):
    u=units.setdefault(uid, dict(id=uid,name=uname,kieu=kieu,path=path,parts=[]))
    u['parts'].append(part)
for x in raw:
    if x['kind']!='prefab': continue
    p=g2p.get(x['src']); 
    if not p: continue
    b=gb(p)
    if not b: continue
    lo,hi=b; s=x['scl']
    lx0,lx1=sorted([-hi[0]*s[0],-lo[0]*s[0]]); lz0,lz1=sorted([lo[2]*s[2],hi[2]*s[2]]); 
    y0=x['pos'][1]+lo[1]*s[1]; y1=x['pos'][1]+hi[1]*s[1]
    part=dict(kind='glb',name=x['name'],poly=obb(x['pos'][0],x['pos'][2],lx0,lx1,lz0,lz1,x['yaw']),y0=round(y0,3),y1=round(y1,3),yaw=round(x['yaw'],1),
              size=[round(lx1-lx0,3),round(hi[1]*s[1]-lo[1]*s[1],3),round(lz1-lz0,3)],active=x['active'],path=x['path'],model=os.path.basename(p))
    u=x.get('unit')
    if u: add_part(u[0],u[1],u[2],x['path'],part)
    else: add_part(('p',x['id']),x['name'],None,x['path'],part)
structure=[]; decals=[]
for b in boxes:
    s=b['scl']; q=b['rotq']; 
    nm=b['name']
    # yaw only
    fx=2*(q[0]*q[2]+q[3]*q[1]); fz=1-2*(q[0]**2+q[1]**2)
    yaw=math.degrees(math.atan2(fx,fz))
    cx,cy,cz=b['pos']
    poly=obb(cx,cz,-s[0]/2,s[0]/2,-s[2]/2,s[2]/2,yaw)
    rec=dict(name=nm,path=b['path'],poly=poly,y0=round(cy-s[1]/2,3),y1=round(cy+s[1]/2,3),size=[round(v,3) for v in s],yaw=round(yaw,1),active=b['active'],pos=[round(v,3) for v in b['pos']],rotq=q)
    if nm.startswith('Decal'):
        decals.append(rec); continue
    if b['unit']:
        u=b['unit']; add_part(u[0],u[1],u[2],b['path'],dict(kind='box',name=nm,poly=poly,y0=rec['y0'],y1=rec['y1'],yaw=round(yaw,1),size=rec['size'],active=b['active'],path=b['path']))
    else:
        structure.append(rec)
# finalize units
ulist=[]
for uid,u in units.items():
    parts=u['parts']
    pts=[pt for p in parts for pt in p['poly']]
    x0,x1=min(p[0] for p in pts),max(p[0] for p in pts); z0,z1=min(p[1] for p in pts),max(p[1] for p in pts)
    y0=min(p['y0'] for p in parts); y1=max(p['y1'] for p in parts)
    seg=parse_path(parts[0]['path'])
    night=None
    for sg in seg:
        m=NIGHT.match(sg)
        if m: night=int(m.group(1))
    lvl,room=group_room(seg)
    if lvl is None: lvl=level_of_y(y0+0.05)
    cx,cz=(x0+x1)/2,(z0+z1)/2
    if room is None or (seg[1:2]==['VoNha'] ): room=by_pos(lvl,cx,cz) if lvl!='H' else 'H'
    big=max(parts,key=lambda p:abs((p['poly'][1][0]-p['poly'][0][0])*(p['poly'][3][1]-p['poly'][0][1])) if True else 0)
    ulist.append(dict(id=str(uid),name=u['name'],kieu=u['kieu'],group='/'.join(seg[:-1]),level=lvl,room=room,night=night,
                      bbox=[round(x0,3),round(z0,3),round(x1,3),round(z1,3)],y0=round(y0,3),y1=round(y1,3),center=[round(cx,3),round(cz,3)],
                      yaw=big['yaw'],active=all(p['active'] for p in parts),parts=parts,top=seg[0] if seg else ''))
json.dump(dict(units=ulist,structure=structure,decals=decals,lights=lights),open(os.path.expanduser('~/work/data.json'),'w'))
print('units',len(ulist),'structure',len(structure),'decals',len(decals))
c=collections.Counter((u['level'],u['room']) for u in ulist); print(sorted(c.items(),key=lambda a:str(a)))
print(collections.Counter(u['top'] for u in ulist))

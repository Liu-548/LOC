import re, json, sys, os, math, collections
SCENE = os.path.expanduser('~/mnt/GameKinhDi/Assets/Scenes/LOC_NhaLoc.unity')
docs = {}   # fileID -> (cls, stripped, text)
hdr = re.compile(r'^--- !u!(\d+) &(\d+)( stripped)?\s*$')
cur=None; buf=[]
def flush():
    if cur: docs[cur[1]] = (cur[0], cur[2], '\n'.join(buf))
with open(SCENE, encoding='utf-8') as f:
    for line in f:
        m = hdr.match(line.rstrip('\n'))
        if m:
            flush(); cur=(int(m.group(1)), int(m.group(2)), bool(m.group(3))); buf=[]
        else:
            buf.append(line.rstrip('\n'))
flush()
print('docs', len(docs), collections.Counter(d[0] for d in docs.values()).most_common(12), file=sys.stderr)
def unq(v):
    v=v.strip()
    if v.startswith('"'):
        v=re.sub(r'\\x([0-9A-Fa-f]{2})', lambda m:'\\u00'+m.group(1), v)
        try: return json.loads(v)
        except Exception: return v.strip('"')
    if v.startswith("'"): return v[1:-1].replace("''","'")
    return v
def vec(txt, key, n=3):
    m = re.search(r'^\s*'+key+r': \{([^}]*)\}', txt, re.M)
    if not m: return None
    d = dict(p.split(': ') for p in m.group(1).split(', '))
    return [float(d[k]) for k in ('x','y','z','w')[:n]]
def fid(txt, key):
    m = re.search(r'^\s*'+key+r': \{fileID: (-?\d+)', txt, re.M)
    return int(m.group(1)) if m else 0
GO = {}; TR = {}; PI = {}
locprop_guid = None
for fid_, (cls, stripped, txt) in docs.items():
    if cls == 1 and not stripped:
        name = unq(re.search(r'^\s*m_Name: (.*)$', txt, re.M).group(1))
        act = int(re.search(r'^\s*m_IsActive: (\d)', txt, re.M).group(1))
        comps = [int(x) for x in re.findall(r'component: \{fileID: (-?\d+)\}', txt)]
        GO[fid_] = dict(name=name, active=act, comps=comps)
    elif cls == 4:
        TR[fid_] = dict(stripped=stripped, go=fid(txt,'m_GameObject'), father=fid(txt,'m_Father'),
                        pi=fid(txt,'m_PrefabInstance'), src_obj=fid(txt,'m_CorrespondingSourceObject'),
                        rot=vec(txt,'m_LocalRotation',4) if not stripped else None,
                        pos=vec(txt,'m_LocalPosition') if not stripped else None,
                        scl=vec(txt,'m_LocalScale') if not stripped else None)
    elif cls == 1001:
        # modification
        parent = fid(txt,'m_TransformParent')
        mods = re.findall(r'- target: \{fileID: (-?\d+), guid: ([0-9a-f]+), type: \d\}\s*\n\s*propertyPath: ([^\n]+)\n\s*value: ([^\n]*)', txt)
        src = re.search(r'm_SourcePrefab: \{fileID: -?\d+, guid: ([0-9a-f]+)', txt)
        PI[fid_] = dict(parent=parent, mods=mods, src=src.group(1) if src else None)
# LocProp guid
meta = os.path.expanduser('~/mnt/GameKinhDi/Assets/LOC_House/Scripts/LocProp.cs.meta')
locprop_guid = re.search(r'guid: (\w+)', open(meta).read()).group(1)
KIEU = {}
for fid_, (cls, stripped, txt) in docs.items():
    if cls == 114 and locprop_guid in txt:
        go = fid(txt,'m_GameObject'); k = re.search(r'^\s*kieu: (\d)', txt, re.M)
        KIEU[go] = int(k.group(1)) if k else 0
print('GO',len(GO),'TR',len(TR),'PI',len(PI),'LocProp',len(KIEU), file=sys.stderr)
# transform -> go name etc.
go_of_tr = {}
for t, d in TR.items():
    if not d['stripped']: go_of_tr[t] = d['go']
# prefab roots: stripped transforms whose pi points to PI ; need root transform (father = PI.parent)
# build effective local TRS per transform id
def q_mul(a,b):
    ax,ay,az,aw=a; bx,by,bz,bw=b
    return (aw*bx+ax*bw+ay*bz-az*by, aw*by-ax*bz+ay*bw+az*bx, aw*bz+ax*by-ay*bx+az*bw, aw*bw-ax*bx-ay*by-az*bz)
def q_rot(q,v):
    x,y,z,w=q; vx,vy,vz=v
    # v' = v + 2w(q x v) + 2 q x (q x v)
    tx=2*(y*vz-z*vy); ty=2*(z*vx-x*vz); tz=2*(x*vy-y*vx)
    return (vx+w*tx+(y*tz-z*ty), vy+w*ty+(z*tx-x*tz), vz+w*tz+(x*ty-y*tx))
def eff(t):
    d = TR[t]
    if not d['stripped']:
        return d['rot'], d['pos'], d['scl'], d['father']
    pi = PI.get(d['pi'])
    rot=[0,0,0,1]; pos=[0,0,0]; scl=[1,1,1]
    if pi:
        for tgt, g, pp, val in pi['mods']:
            if int(tgt)!=d['src_obj']: continue
            mm = re.match(r'm_Local(Position|Rotation|Scale)\.([xyzw])$', pp)
            if mm and True:
                tgt_ok = True
                key=mm.group(1); comp='xyzw'.index(mm.group(2)); v=float(val)
                (pos if key=='Position' else rot if key=='Rotation' else scl)[comp]=v
    return rot,pos,scl,(pi['parent'] if pi else 0)
t_src={}
WORLD={}
def world(t):
    if t in WORLD: return WORLD[t]
    if t==0 or t not in TR:
        WORLD[t]=((0,0,0,1),(0,0,0),(1,1,1)); return WORLD[t]
    rot,pos,scl,father = eff(t)
    pr,pp,ps = world(father)
    wp = q_rot(pr, (pos[0]*ps[0], pos[1]*ps[1], pos[2]*ps[2]))
    w = (q_mul(pr,tuple(rot)), (pp[0]+wp[0], pp[1]+wp[1], pp[2]+wp[2]), (ps[0]*scl[0], ps[1]*scl[1], ps[2]*scl[2]))
    WORLD[t]=w; return w
sys.setrecursionlimit(10000)
def path_of(t):
    names=[]
    while t and t in TR:
        d=TR[t]
        if d['stripped']:
            pi=PI.get(d['pi']); nm=None
            if pi:
                for tgt,g,pp,val in pi['mods']:
                    if pp=='m_Name': nm=unq(val)
            names.append(nm or '?'); t = pi['parent'] if pi else 0
        else:
            names.append(GO[d['go']]['name']); t=d['father']
    return '/'.join(reversed(names))
def active_chain(t):
    a=True
    while t and t in TR:
        d=TR[t]
        if d['stripped']:
            pi=PI.get(d['pi']); act=1
            if pi:
                for tgt,g,pp,val in pi['mods']:
                    if pp=='m_IsActive' and False: act=int(val)
            t = pi['parent'] if pi else 0
        else:
            if not GO[d['go']]['active']: a=False
            t=d['father']
    return a
def yaw_of(q):
    x,y,z,w=q
    fx,fy,fz = q_rot(q,(0,0,1))
    return math.degrees(math.atan2(fx,fz))
out=[]
for t,d in TR.items():
    if d['stripped']:
        pi=PI.get(d['pi'])
        if not pi: continue
        name=None
        for tgt,g,pp,val in pi['mods']:
            if pp=='m_Name': name=unq(val)
        q,p,s = world(t)
        out.append(dict(kind='prefab', id=t, name=name, src=pi['src'], path=path_of(t), pos=p, yaw=yaw_of(q), scl=s, active=active_chain(t),
                        parent_go=None))
    else:
        go=GO[d['go']]
        q,p,s = world(t)
        out.append(dict(kind='go', id=t, name=go['name'], path=path_of(t), pos=p, yaw=yaw_of(q), scl=s, active=active_chain(t),
                        locprop=KIEU.get(d['go']), nchild=None, father=d['father'], comps=len(go['comps'])))

def unit_of(t):
    # climb to the top-most ancestor (excluding group containers) that has LocProp; return (transform id, go name, kieu)
    best=None; cur_t=t
    while cur_t and cur_t in TR:
        d=TR[cur_t]
        if d['stripped']:
            pi=PI.get(d['pi']); cur_t = pi['parent'] if pi else 0; continue
        if KIEU.get(d['go']) is not None: best=(cur_t, GO[d['go']]['name'], KIEU[d['go']])
        cur_t=d['father']
    return best
for o_ in out:
    if o_['kind']=='prefab':
        u=unit_of(o_['id']); o_['unit']=list(u) if u else None
json.dump(out, open(os.path.expanduser('~/work/scene_raw.json'),'w'))
print('written', len(out), file=sys.stderr)

# ---- stage 1b: GO with MeshFilter
mf_go=set()
mesh_of={}
for fid_,(cls,stripped,txt) in docs.items():
    if cls==33 and not stripped:
        g=fid(txt,'m_GameObject'); mf_go.add(g)
        mm=re.search(r'm_Mesh: \{fileID: (-?\d+)(?:, guid: (\w+))?', txt)
        mesh_of[g]=(int(mm.group(1)), mm.group(2)) if mm else None
boxes=[]
for x in out:
    if x['kind']=='go':
        go=GO[TR[x['id']]['go']]
        gid=TR[x['id']]['go']
        if gid in mf_go:
            q,p,s = world(x['id'])
            u=unit_of(x['id'])
            boxes.append(dict(name=x['name'], path=x['path'], pos=p, scl=s, yaw=x['yaw'], rotq=q, active=x['active'], mesh=mesh_of[gid], unit=list(u) if u else None))
json.dump(boxes, open(os.path.expanduser('~/work/scene_boxes.json'),'w'))
print('boxes',len(boxes), file=sys.stderr)

# lights
lights=[]
for fid_,(cls,stripped,txt) in docs.items():
    if cls==108 and not stripped:
        g=fid(txt,'m_GameObject')
        t=[k for k,v in TR.items() if not v['stripped'] and v['go']==g]
        if not t: continue
        q,p,sc=world(t[0])
        col=re.search(r'm_Color: \{r: ([\d.e-]+), g: ([\d.e-]+), b: ([\d.e-]+)',txt)
        rng=re.search(r'm_Range: ([\d.e-]+)',txt); inten=re.search(r'm_Intensity: ([\d.e-]+)',txt); ty=re.search(r'm_Type: (\d)',txt)
        lights.append(dict(name=GO[g]['name'],path=path_of(t[0]),pos=p,color=[float(col.group(i)) for i in (1,2,3)] if col else None,range=float(rng.group(1)) if rng else None,intensity=float(inten.group(1)) if inten else None,type=int(ty.group(1)) if ty else None,active=active_chain(t[0]),goactive=GO[g]['active']))
json.dump(lights, open(os.path.expanduser('~/work/scene_lights.json'),'w'))
print('lights',len(lights), file=sys.stderr)

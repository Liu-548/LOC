import json, struct, os, math
BASE=os.path.expanduser('~/mnt/GameKinhDi')
HIDE=("ThamChieu","PhongThamChieu","VatKiem","EyeLevel","KhoiNhaXa","SM_ScaleRef")
def mat_from_node(n):
    if 'matrix' in n:
        m=n['matrix']; return [[m[c*4+r] for c in range(4)] for r in range(4)]
    t=n.get('translation',[0,0,0]); q=n.get('rotation',[0,0,0,1]); s=n.get('scale',[1,1,1])
    x,y,z,w=q
    R=[[1-2*(y*y+z*z),2*(x*y-z*w),2*(x*z+y*w)],[2*(x*y+z*w),1-2*(x*x+z*z),2*(y*z-x*w)],[2*(x*z-y*w),2*(y*z+x*w),1-2*(x*x+y*y)]]
    M=[[R[r][c]*s[c] for c in range(3)]+[t[r]] for r in range(3)]+[[0,0,0,1]]
    return M
def mm(A,B): return [[sum(A[i][k]*B[k][j] for k in range(4)) for j in range(4)] for i in range(4)]
def bounds(path):
    p=os.path.join(BASE,path)
    with open(p,'rb') as f:
        d=f.read()
    magic,ver,ln=struct.unpack('<III',d[:12]); off=12
    js=None
    while off<len(d):
        cl,ct=struct.unpack('<II',d[off:off+8]); 
        if ct==0x4E4F534A: js=json.loads(d[off+8:off+8+cl])
        off+=8+cl
    nodes=js.get('nodes',[]); meshes=js.get('meshes',[]); acc=js.get('accessors',[])
    sc=js['scenes'][js.get('scene',0)]['nodes']
    mn=[1e9]*3; mx=[-1e9]*3
    def walk(i,M):
        n=nodes[i]
        if n.get('name','').startswith(HIDE): return
        M2=mm(M,mat_from_node(n))
        if 'mesh' in n:
            for pr in meshes[n['mesh']]['primitives']:
                a=acc[pr['attributes']['POSITION']]
                lo,hi=a['min'],a['max']
                for cx in (lo[0],hi[0]):
                    for cy in (lo[1],hi[1]):
                        for cz in (lo[2],hi[2]):
                            v=[M2[r][0]*cx+M2[r][1]*cy+M2[r][2]*cz+M2[r][3] for r in range(3)]
                            for k in range(3): mn[k]=min(mn[k],v[k]); mx[k]=max(mx[k],v[k])
        for c in n.get('children',[]): walk(c,M2)
    I=[[1,0,0,0],[0,1,0,0],[0,0,1,0],[0,0,0,1]]
    for r in sc: walk(r,I)
    if mn[0]>1e8: return None
    return mn,mx
if __name__=='__main__':
    import sys
    g=json.load(open(os.path.expanduser('~/work/guid2path.json')))
    raw=json.load(open(os.path.expanduser('~/work/scene_raw.json')))
    for nm in ('BanAn','TuLanh','GiuongDoi','DenTuyp_120'):
        x=[y for y in raw if y['kind']=='prefab' and y['name']==nm][0]
        print(nm,g[x['src']],bounds(g[x['src']]), [round(v,2) for v in x['pos']], round(x['yaw']))

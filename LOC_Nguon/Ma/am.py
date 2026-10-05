import numpy as np, wave
SR=44100
def save(fn,x):
    x=x/np.max(np.abs(x))*0.9; d=(x*32767).astype(np.int16)
    with wave.open(fn,'wb') as w: w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR); w.writeframes(d.tobytes())
def bp(x,f,q):  # biquad bandpass
    w0=2*np.pi*f/SR; a=np.sin(w0)/(2*q); b0,b1,b2=a,0,-a; a0,a1,a2=1+a,-2*np.cos(w0),1-a
    y=np.zeros_like(x); x1=x2=y1=y2=0
    for i,v in enumerate(x):
        o=(b0*v+b1*x1+b2*x2-a1*y1-a2*y2)/a0; x2,x1,y2,y1=x1,v,y1,o; y[i]=o
    return y
def verb(x,n=6):
    y=np.copy(x)
    for k in range(n):
        d=int(SR*(0.031+0.017*k)); g=0.55-0.06*k
        z=np.zeros_like(y); z[d:]=y[:-d]*g; y=y+z
    return y
rng=np.random.default_rng(7)
# tiếng thét: 3 lớp giọng méo, cao độ vút lên rồi gãy, có rung và hơi khàn
T=2.8; t=np.arange(int(SR*T))/SR
f0=np.interp(t,[0,0.15,0.6,1.8,2.8],[260,820,980,700,380])
env=np.interp(t,[0,0.06,0.4,2.0,2.8],[0,1,0.9,0.7,0])
x=np.zeros_like(t)
for det,amp in ((1,1),(1.013,0.7),(0.494,0.5),(1.51,0.3)):
    vib=1+0.03*np.sin(2*np.pi*(6.5+det)*t)+0.01*rng.standard_normal(len(t)).cumsum()/300
    ph=2*np.pi*np.cumsum(f0*det*vib)/SR
    x+=amp*(2*((ph/(2*np.pi))%1)-1)
x=np.tanh(3*x)+0.35*rng.standard_normal(len(t))
y=bp(x,950,2.5)+0.8*bp(x,2600,3)+0.4*bp(x,3800,4)
y=verb(y*env)
pad=np.zeros(int(SR*0.8)); save('Ma_TiengThet.wav',np.concatenate([y,pad*0]))
# chén sứ: nứt (lách tách) - 3 giây - vỡ
def ping(f,dur,dec):
    tt=np.arange(int(SR*dur))/SR; return np.sin(2*np.pi*f*tt)*np.exp(-tt*dec)
crack=np.zeros(int(SR*0.6))
for k in range(9):
    p=int(SR*(0.02+0.06*k+0.01*rng.random())); c=rng.standard_normal(300)*np.exp(-np.arange(300)/40)
    crack[p:p+300]+=c*(0.5+rng.random()*0.5)
crack+=np.pad(0.15*ping(3400,0.6,9),(0,0))[:len(crack)]
silence=np.zeros(int(SR*3.0))
sh=np.zeros(int(SR*1.6)); n=rng.standard_normal(len(sh))*np.exp(-np.arange(len(sh))/(SR*0.06))
sh+=bp(n,4000,1.2)*1.5
for k in range(28):
    p=int(SR*rng.uniform(0,0.9)); pg=ping(rng.uniform(2500,7500),0.35,rng.uniform(15,40))*rng.uniform(0.2,0.6)
    e=min(len(sh),p+len(pg)); sh[p:e]+=pg[:e-p]
save('Ma_ChenVo.wav',verb(np.concatenate([crack,silence,sh]),3))

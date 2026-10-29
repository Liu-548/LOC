#!/bin/bash
# dong_goi.sh <tên gói> : gom mọi .glb trong out/Assets mới hơn mốc lần đóng gói trước → /mnt/user-data/outputs/<gói>.partNN
set -e
GOI=$1
cd /home/claude/nc/out
M=/home/claude/nc/.moc_goi
if [ -f $M ]; then find Assets -name '*.glb' -newer $M > /tmp/ds2.txt; else find Assets -name '*.glb' > /tmp/ds2.txt; fi
touch $M
rm -f /mnt/user-data/outputs/$GOI.*
tar czf /mnt/user-data/outputs/$GOI.tgz -T /tmp/ds2.txt
cd /mnt/user-data/outputs && split -b 25M -d $GOI.tgz $GOI.part && rm $GOI.tgz
ls /mnt/user-data/outputs/ | grep $GOI; wc -l < /tmp/ds2.txt

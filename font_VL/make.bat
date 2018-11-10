SET BMFONT="C:\Program Files - tools -\bmfont\bmfont64.exe"

%BMFONT% -c VL.bmfc -o out\VL.fnt
SJIS2UTF8 out\VL.fnt out\VL.fnt.txt
pause

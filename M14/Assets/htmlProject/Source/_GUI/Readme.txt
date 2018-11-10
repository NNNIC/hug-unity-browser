NGUIの代わりになるもの。
HTMLで使われる Bold, Italic, Shadow, Outlineが使える。
入力方法は、HTML式。 
   イタリック:<i>hoge</i> 
   ボールド  :<b>hoge</b>
   カラー    :<color=red>hoge</color>
   下線      :<u>hoge</u>
   打消線    :<s>hoge</s>
   点滅      :<blink>hoge</blink>
   上付      :<sup>hoge</sup>
   下付      :<sub>hoge</sub>
スタイルが使える
   <style class=hoge>XXXXX</style>

画像が使える
   <img src="hoge.png"/>  --  Resouces/hoge.pngを表示。
   <img src="image:0" />  --  インスペクタのimageを表示。
   <img src="atlas:hoge/> --  アトラスを表示
   <img src="atlas:v/hoge/>-- 親アトラスも指定。

縦書きができる。 --- 将来的に


TTFからBMPを作成
http://d.hatena.ne.jp/nakamura001/20120910/1347241168


作業の進め方

1.FONT読み込みと簡易表示
2.フォントサイズ/カラー
3.アウトライン、シャドー実装
4.絵の表示
5.深度レイヤの実装
6.枠線
7.下線



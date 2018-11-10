☆本ツールについて
利点
　NGUIのパフォーマンス。
　Unityとの完全インターフェース。
　TopバーとBottomバーの提供。
　オリジナルイメージ

欠点
　不完全なHTML解釈。
　NGUIによる制約。


☆サポートタグ
<a>　　　　ボタンになる。paramを指定すると、ジャンプ先でパラメータとして使える。参照：※param
<address>　無視
<body>　styleのbackgoundが有効
<br/>   閉じ必須
<div>　　　
<footer>　
<font>　無視
<h1>
<h2>
<head>
<hr>
<html>
<img>  アトラスは　atlas="xx"で指定する。
<li>
<p>
<spacer>
<span>
<style>
<title>
<table>　枠が透明。マージンが未サポート
<td>
<th>
<tr>
<ul>


☆サポートスタイル

	//Color
    color,

    //Margin
    margin,
    margin_top,
    margin_right,
    margin_bottom,
    margin_left,

    //Background
    background,         img("")-未対応　 atlas("hoge")-original アトラス指定
    background_color,
    background_image,　　無視される。
    background_atlas,　　orijinal  atlas("hoge")と記述。 bodyでのみ有効。

    //font
    font_size,
    line_height,　// 最終が全体に適応される

    //text
    text_align,
    text_shadow,
    text_outline,  // original

    //Unity:original
    update_z_axis,
	
	//Internal use
	address_harf, 
	address_param, 

※スタイルの制限
　id 未サポート

※スタイル中のアトラス指定
　atlas("hoge")    --- url("xx")の真似。


※paramについて

<a>タグにて、paramに引数を設定して、リンク先の表示時に使える。
例）
　　<a href="hoge.html" param="a.jpg;b.jpg">ホゲへ</a>

hoge.htmlにて
　　<img src="images/[P1] />  .... images/a.jpgと解釈される。
　　<img src="images/[P2] />  .... images/b.jpgと解釈される。





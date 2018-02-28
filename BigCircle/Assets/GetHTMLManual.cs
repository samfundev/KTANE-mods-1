using System;
using System.IO;
using System.Linq;

namespace Assets.Scripts.RuleGenerator
{
    public partial class BigCircleRuleGenerator
    {
        public override string GetHTMLManual(out string filename)
        {
            filename = "Big Circle.html";
            if (Rules == null) throw new Exception("You must initializae the random number generator and create the rules first");

            string[] letters =
            {
                "0, 1, 2", "3, 4, 5", "6, 7, 8", "9, A, B", "C, D, E", "F, G, H", "I, J, K", "L, M, N", "O, P, Q", "R, S, T", "U, V, W", "X, Y, Z"
            };
            string colorTable = "";
            for (int i = 0; i < 12; i++)
            {
                colorTable += "<tr><td>" + letters[i] + "</td><td>" + string.Join(", ", Rules[i].Select(x => x.ToString()).ToArray()) + "</td></tr>\n";
            }

            return @"<!DOCTYPE html>
    
<html class=""no-js""><!--<![endif]--><head><meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
    
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <title>Big Circle - Keep Talking and Nobody Explodes Module</title>
    <meta name=""viewport"" content=""initial-scale=1""> 
	<link href='https://fonts.googleapis.com/css?family=Special+Elite' rel='stylesheet' type='text/css'>
    <link rel=""stylesheet"" type=""text/css"" href=""css/normalize.css"">
    <link rel=""stylesheet"" type=""text/css"" href=""css/main.css"">
	<style type=""text/css"">
		.floater {
		    margin: 5px;
		    float: left;
		}
		.floater td {
		    padding: 4px;
		    border: 1px solid black;
		}
		.clear {
			clear: left;
		}
	</style>
</head>
<body>
<div class=""section"">
    <div class=""page page-bg-03"">
        <div class=""page-header"">
			<span class=""page-header-doc-title"">Keep Talking and Nobody Explodes Mod</span>
			<span class=""page-header-section-title"">Big Circle</span>
		</div>
        <div class=""page-content"">
    	    <img src=""img/Component/Big%20Circle.svg"" class=""diagram"">
            <h2>On the Subject of The Big Circle</h2>
            <p class=""flavour-text"">
					Who said that a circle was pointless?
			</p>

			<p> The bomb presents a button in the shape of a big circle spinning around.  Based on the tables below, determine which parts of the circle to press. If you press a wrong part of the circle or press the parts of a circle in the wrong order, a strike will be incurred.</p>
			
			<ul>
			<li>Start with Table 1 below. Determine which rules apply to find the next number to be used. If your number is negative, multiply it by -1.</li>
			<li>Count the position in the serial number as many times as your number. If you get to the end of the serial number, bounce back and continue to read the serial number backwards, and vice versa at the beginning. The first character of the serial number is considered to be position 0.</li>
			<li>After you have determined the position in the serial number, look at Table 2 below to determine the sequence of button presses based on the chosen character in the serial number and press it.</li>
			<li>Once the first correct color has been pressed, The solution will not change unless you incur a strike on this module.</li>
			<li>BUT, if there are five batteries in three holders and at least one BOB indicator, Any solution that would be valid for the serial number characters present on the bomb will be accepted. Look at table 2 to find out what those solutions are.</li>
			</ul>
        </div>
    </div>
    	
    <div class=""page page-bg-04"">
    	<div class=""page-header"">
			<span class=""page-header-doc-title"">Keep Talking and Nobody Explodes Mod</span>
			<span class=""page-header-section-title"">Big Circle</span>
		</div>
		<div class=""page-content"">
			<br>
    		<strong>Table 1</strong>
    		<br>
			<table class=""floater"">
				<tbody>
					<tr><td>For each BOB, CAR,  or CLR<br>indicator:</td><td>+1 if lit, -1 if unlit.</td></tr>
					<tr><td>For each FRK, FRQ, MSA, or NSA<br>indicator:</td><td>+2 if lit, -2 if unlit.</td></tr>
					<tr><td>For SIG, SND, or TRN indicator:</td><td>+3 if lit, -3 if unlit.</td></tr>
					<tr><td>For each solved modules:</td><td>+3 each.</td></tr>
					<tr><td>For the number of batteries:</td><td>+4 for odd, -4 for even.</td></tr>
					<tr><td>There are port plates with<br>parallel ports:</td><td>+5 each, -4 if paired<br>with serial port.</td></tr>
					<tr><td>There are port plates with<br>DVI-D ports:</td><td>-5 each, +4 if paired<br>with Stereo-RCA.</td></tr>
					<tr><td>For each special* indicator:</td><td>+6 each.</td></tr>
					<tr><td>For each special* port:</td><td>-6 each.</td></tr>
					<tr><td>For each Two Factor code:</td><td>Add the least<br>significant digit.</td></tr>
				</tbody>
			</table>
			<p>*Special ports or indicators are custom made, such as NLL or the USB port.</p>
			<br class=""clear"">
			<br>
			<strong>Table 2*</strong><br>
			<table class=""floater"">
				<tbody>
					" + colorTable + @"
				</tbody>
			</table>
			<p>*If circle is spinning counter-clockwise, reverse order of button presses.</p>
		</div>
    </div>
    
</div>
</body></html>";
        }

        public string[] ImagePaths = { Path.Combine("img", "Component") };

        public override string[] GetTextFiles(out string[] textFilePaths)
        {
            textFilePaths = new[] { Path.Combine(Path.Combine("img","Component"), "Big Circle.svg") };
            return new[] {
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<svg
   xmlns:dc=""http://purl.org/dc/elements/1.1/""
   xmlns:cc=""http://creativecommons.org/ns#""
   xmlns:rdf=""http://www.w3.org/1999/02/22-rdf-syntax-ns#""
   xmlns:svg=""http://www.w3.org/2000/svg""
   xmlns=""http://www.w3.org/2000/svg""
   xmlns:sodipodi=""http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd""
   xmlns:inkscape=""http://www.inkscape.org/namespaces/inkscape""
   version=""1.1""
   viewBox=""0.0 0.0 348.0 348.0""
   fill=""none""
   stroke=""none""
   stroke-linecap=""square""
   stroke-miterlimit=""10""
   id=""svg2""
   inkscape:version=""0.91 r13725""
   sodipodi:docname=""Component.svg"">
  <metadata
     id=""metadata25"">
    <rdf:RDF>
      <cc:Work
         rdf:about="""">
        <dc:format>image/svg+xml</dc:format>
        <dc:type
           rdf:resource=""http://purl.org/dc/dcmitype/StillImage"" />
      </cc:Work>
    </rdf:RDF>
  </metadata>
  <defs
     id=""defs23"" />
  <sodipodi:namedview
     pagecolor=""#ffffff""
     bordercolor=""#666666""
     borderopacity=""1""
     objecttolerance=""10""
     gridtolerance=""10""
     guidetolerance=""10""
     inkscape:pageopacity=""0""
     inkscape:pageshadow=""2""
     inkscape:window-width=""1920""
     inkscape:window-height=""1017""
     id=""namedview21""
     showgrid=""false""
     inkscape:zoom=""0.67816092""
     inkscape:cx=""174""
     inkscape:cy=""174""
     inkscape:window-x=""-8""
     inkscape:window-y=""-8""
     inkscape:window-maximized=""1""
     inkscape:current-layer=""svg2"" />
  <clipPath
     id=""p.0"">
    <path
       d=""m0 0l348.0 0l0 348.0l-348.0 0l0 -348.0z""
       clip-rule=""nonzero""
       id=""path5"" />
  </clipPath>
  <g
     clip-path=""url(#p.0)""
     id=""g7"">
    <path
       fill=""#000000""
       fill-opacity=""0.0""
       d=""m0 0l348.0 0l0 348.0l-348.0 0z""
       fill-rule=""nonzero""
       id=""path9"" />
    <path
       fill=""#ffffff""
       d=""m-6.0 0l348.0 0l0 347.9685l-348.0 0z""
       fill-rule=""nonzero""
       id=""path11"" />
    <path
       fill=""#000000""
       fill-opacity=""0.0""
       d=""m5.07874 5.7758217l336.9134 0l0 337.6693l-336.9134 0z""
       fill-rule=""nonzero""
       id=""path13"" />
    <path
       stroke=""#000000""
       stroke-width=""2.0""
       stroke-linejoin=""round""
       stroke-linecap=""butt""
       d=""m5.07874 5.7758217l336.9134 0l0 337.6693l-336.9134 0z""
       fill-rule=""nonzero""
       id=""path15"" />
    <path
       fill=""#ffffff""
       d=""m282.73444 40.553925l0 0c0 -8.375591 6.966034 -15.165352 15.5590515 -15.165352l0 0c4.126526 0 8.084015 1.5977726 11.001923 4.441828c2.9178772 2.844057 4.557129 6.7014217 4.557129 10.723524l0 0c0 8.375595 -6.9660034 15.165356 -15.5590515 15.165356l0 0c-8.593018 0 -15.5590515 -6.7897606 -15.5590515 -15.165356z""
       fill-rule=""nonzero""
       id=""path17"" />
    <path
       stroke=""#000000""
       stroke-width=""2.0""
       stroke-linejoin=""round""
       stroke-linecap=""butt""
       d=""m282.73444 40.553925l0 0c0 -8.375591 6.966034 -15.165352 15.5590515 -15.165352l0 0c4.126526 0 8.084015 1.5977726 11.001923 4.441828c2.9178772 2.844057 4.557129 6.7014217 4.557129 10.723524l0 0c0 8.375595 -6.9660034 15.165356 -15.5590515 15.165356l0 0c-8.593018 0 -15.5590515 -6.7897606 -15.5590515 -15.165356z""
       fill-rule=""nonzero""
       id=""path19"" />
  </g>
  <ellipse
     style=""fill:#ffffff;fill-opacity:1;stroke:#000000;stroke-width:5.065;stroke-miterlimit:9.25547409;stroke-dasharray:none;stroke-opacity:1""
     id=""path4157""
     cx=""164.48166""
     cy=""182.84732""
     rx=""111.96281""
     ry=""111.96263""
     inkscape:transform-center-x=""-16.220339""
     inkscape:transform-center-y=""-1.4745763"" />
</svg>
" 
            };
        }
    }
}
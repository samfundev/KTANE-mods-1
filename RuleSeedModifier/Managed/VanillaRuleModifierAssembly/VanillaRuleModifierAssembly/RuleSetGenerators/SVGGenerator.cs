using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Assets.Scripts.Utility;

namespace VanillaRuleModifierAssembly.RuleSetGenerators
{
    public class SVGGenerator
    {
        private const string Ns = "http://www.w3.org/2000/svg";
        // ReSharper disable InconsistentNaming
        private readonly XmlDocument document;
        private readonly XmlElement svg;
        // ReSharper restore InconsistentNaming

        public SVGGenerator(int width, int height)
        {
            document = new XmlDocument();
            svg = document.CreateElement(nameof(svg), Ns);
            document.AppendChild(svg);
            svg.SetAttribute(nameof(width), "100%");
            svg.SetAttribute(nameof(height), "100%");
            svg.SetAttribute("viewBox", $"0 0 {width} {height}");
            svg.SetAttribute("preserveAspectRatio", "xMidYMid meet");
            svg.SetAttribute("version", "1.1");
        }

        // ReSharper disable once UnusedMember.Global
        public void DrawLine(float x1, float y1, float x2, float y2)
        {
            DrawLine(x1, y1, x2, y2, "1", string.Empty);
        }

        public void DrawLine(float x1, float y1, float x2, float y2, string strokeWidth, string strokeDashArray)
        {
            XmlElement element = document.CreateElement("line", Ns);
            element.SetAttribute(nameof(x1), string.Empty + x1);
            element.SetAttribute(nameof(x2), string.Empty + x2);
            element.SetAttribute(nameof(y1), string.Empty + y1);
            element.SetAttribute(nameof(y2), string.Empty + y2);
            element.SetAttribute("stroke", "black");
            element.SetAttribute("stroke-width", strokeWidth);
            element.SetAttribute("stroke-dasharray", strokeDashArray);
            element.SetAttribute("fill", "none");
            svg.AppendChild(element);
        }

        // ReSharper disable once UnusedMember.Global
        public void DrawCircle(float cx, float cy, float r, bool filled)
        {
            XmlElement element = document.CreateElement("circle", Ns);
            element.SetAttribute(nameof(cx), string.Empty + cx);
            element.SetAttribute(nameof(cy), string.Empty + cy);
            element.SetAttribute(nameof(r), string.Empty + r);
            element.SetAttribute("stroke", "black");
            element.SetAttribute("fill", filled ? "grey" : "none");
            element.SetAttribute("stroke-width", "1");
            svg.AppendChild(element);
        }

        public void DrawEllipse(float cx, float cy, float rx, float ry, int posX, int posY, float rotation, string fillColor, float strokeWidth, float[] strokeDashArray)
        {
            var str = string.Join(",", strokeDashArray.Select(f => f.ToString()).ToArray());
            XmlElement element = document.CreateElement("ellipse", Ns);
            element.SetAttribute(nameof(cx), cx.ToString());
            element.SetAttribute(nameof(cy), cy.ToString());
            element.SetAttribute(nameof(rx), rx.ToString());
            element.SetAttribute(nameof(ry), ry.ToString());
            element.SetAttribute("stroke", "black");
            element.SetAttribute("fill", fillColor);
            element.SetAttribute("stroke-width", strokeWidth.ToString());
            element.SetAttribute("stroke-dasharray", str);
            element.SetAttribute("transform", $"translate({posX}, {posY}) rotate({rotation})");
            svg.AppendChild(element);
        }

        public void DrawLocalizableText(float x, float y, string fontSize, string textAnchor, string term, string lineHeight = "1.2em")
        {
            XmlElement element1 = document.CreateElement("text", Ns);
            element1.SetAttribute("font-family", "Special Elite, sans-serif");
            element1.SetAttribute("text-anchor", textAnchor);
            element1.SetAttribute(nameof(x), x.ToString());
            element1.SetAttribute(nameof(y), y.ToString());
            element1.SetAttribute("font-size", fontSize);
            XmlElement element2 = document.CreateElement("tspan", Ns);
            //element2.SetAttribute("data-localize", term);
            //element2.InnerText = string.Empty;
            element2.InnerText = Localization.GetLocalizedString(term);
            element1.AppendChild(element2);
            svg.AppendChild(element1);
        }

        public void DrawRect(float x, float y, float width, float height, string fillColor, string strokeWidth, string strokeDashArray)
        {
            XmlElement element = document.CreateElement("rect", Ns);
            element.SetAttribute(nameof(x), x.ToString());
            element.SetAttribute(nameof(y), y.ToString());
            element.SetAttribute(nameof(width), width.ToString());
            element.SetAttribute(nameof(height), height.ToString());
            element.SetAttribute("stroke", "black");
            element.SetAttribute("fill", fillColor);
            element.SetAttribute("stroke-width", strokeWidth);
            element.SetAttribute("stroke-dasharray", strokeDashArray);
            svg.AppendChild(element);
        }

        public void Draw4SetVennDiagram(List<string> labels, List<float[]> strokeDashArrays, List<float> strokeThickness)
        {
            var index = 0;
            DrawEllipse(0f, 0f, 300f, 160f, 350, 300, 45f, "none", strokeThickness[index], strokeDashArrays[index++]);
            DrawEllipse(0f, 0f, 300f, 160f, 450, 300, -45f, "none", strokeThickness[index], strokeDashArrays[index++]);
            DrawEllipse(0f, 0f, 300f, 160f, 250, 400, 45f, "none", strokeThickness[index], strokeDashArrays[index++]);
            DrawEllipse(0f, 0f, 300f, 160f, 550, 400, -45f, "none", strokeThickness[index], strokeDashArrays[index]);
            index = 0;
            var fontSize = "48px";
            var fontSize2 = "32px";
            DrawLocalizableText(225f, 150f, fontSize, "middle", labels[index++]);
            DrawLocalizableText(575f, 150f, fontSize, "middle", labels[index++]);
            DrawLocalizableText(100f, 350f, fontSize, "middle", labels[index++]);
            DrawLocalizableText(700f, 350f, fontSize, "middle", labels[index++]);
            DrawLocalizableText(185f, 250f, fontSize2, "middle", labels[index++]);
            DrawLocalizableText(400f, 225f, fontSize2, "middle", labels[index++]);
            DrawLocalizableText(615f, 250f, fontSize2, "middle", labels[index++]);
            DrawLocalizableText(400f, 590f, fontSize2, "middle", labels[index++]);
            DrawLocalizableText(260f, 495f, fontSize2, "middle", labels[index++]);
            DrawLocalizableText(530f, 495f, fontSize2, "middle", labels[index++]);
            DrawLocalizableText(305f, 350f, fontSize2, "middle", labels[index++]);
            DrawLocalizableText(500f, 350f, fontSize2, "middle", labels[index++]);
            DrawLocalizableText(335f, 530f, fontSize2, "middle", labels[index++]);
            DrawLocalizableText(465f, 530f, fontSize2, "middle", labels[index++]);
            DrawLocalizableText(400f, 450f, fontSize2, "middle", labels[index++]);
            DrawLocalizableText(400f, 80f, fontSize, "middle", labels[index]);
        }

        public void DrawVennDiagramLegend(List<string> terms, List<float[]> strokeDashArrays, List<float> strokeThickness)
        {
            var index = 0;
            DrawRect(2f, 2f, 250f, 180f, "none", "2", string.Empty);
            const float num = 0.7f;
            DrawLine(20f, 30f, 125f, 30f, (strokeThickness[index] * num).ToString(), ConvertFloatArrayToString(strokeDashArrays[index++], num));
            DrawLine(20f, 70f, 125f, 70f, (strokeThickness[index] * num).ToString(), ConvertFloatArrayToString(strokeDashArrays[index++], num));
            DrawLine(20f, 110f, 125f, 110f, (strokeThickness[index] * num).ToString(), ConvertFloatArrayToString(strokeDashArrays[index++], num));
            DrawLine(20f, 150f, 125f, 150f, (strokeThickness[index] * num).ToString(), ConvertFloatArrayToString(strokeDashArrays[index], num));

            index = 0;
            var fontSize = "12px";
            var textAnchor = "left-middle";
            DrawLocalizableText(145f, 27f, fontSize, textAnchor, terms[index++]);
            DrawLocalizableText(145f, 67f, fontSize, textAnchor, terms[index++]);
            DrawLocalizableText(145f, 113f, fontSize, textAnchor, terms[index++]);
            DrawLocalizableText(145f, 154f, fontSize, textAnchor, terms[index]);
        }

        protected string ConvertFloatArrayToString(float[] strokeArray, float scaleFactor)
        {
            return strokeArray.Length > 0 
                ? string.Join(",", strokeArray.Select(f => (f * scaleFactor).ToString()).ToArray()) 
                : string.Empty;
        }

        public override string ToString()
        {
            using (StringWriterWithEncoding writerWithEncoding = new StringWriterWithEncoding(Encoding.UTF8))
            {
                using (XmlWriter w = XmlWriter.Create(writerWithEncoding))
                {
                    svg.WriteTo(w);
                    w.Flush();
                }
                return writerWithEncoding.ToString();
            }
        }
    }
}

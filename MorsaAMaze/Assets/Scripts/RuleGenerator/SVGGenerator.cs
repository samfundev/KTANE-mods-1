using System.IO;
using System.Text;
using System.Xml;

namespace Assets.Scripts.RuleGenerator
{
    public class SVGGenerator
    {
        public SVGGenerator(int width, int height)
        {
            document = new XmlDocument();
            svg = document.CreateElement("svg", ns);
            document.AppendChild(svg);
            svg.SetAttribute("class", "morseMaze");
            svg.SetAttribute("width", "100%");
            svg.SetAttribute("height", "100%");
            svg.SetAttribute("viewBox", string.Format("0 0 {0} {1}", width, height));
            svg.SetAttribute("preserveAspectRatio", "xMidYMid meet");
            svg.SetAttribute("version", "1.1");
        }

        public void DrawLine(float x1, float y1, float x2, float y2)
        {
            DrawLine(x1, y1, x2, y2, "1", string.Empty);
        }

        public void DrawLine(float x1, float y1, float x2, float y2, string strokeWidth, string strokeDashArray)
        {
            var xmlElement = document.CreateElement("line", ns);
            xmlElement.SetAttribute("x1", string.Empty + x1);
            xmlElement.SetAttribute("x2", string.Empty + x2);
            xmlElement.SetAttribute("y1", string.Empty + y1);
            xmlElement.SetAttribute("y2", string.Empty + y2);
            xmlElement.SetAttribute("stroke", "black");
            xmlElement.SetAttribute("stroke-width", strokeWidth);
            xmlElement.SetAttribute("stroke-dasharray", strokeDashArray);
            xmlElement.SetAttribute("fill", "none");
            svg.AppendChild(xmlElement);
        }

        public void DrawCircle(float cx, float cy, float r, bool filled)
        {
            var xmlElement = document.CreateElement("circle", ns);
            xmlElement.SetAttribute("cx", string.Empty + cx);
            xmlElement.SetAttribute("cy", string.Empty + cy);
            xmlElement.SetAttribute("r", string.Empty + r);
            xmlElement.SetAttribute("stroke", "black");
            xmlElement.SetAttribute("fill", filled ? "grey" : "none");
            xmlElement.SetAttribute("stroke-width", "1");
            svg.AppendChild(xmlElement);
        }

        public void DrawEllipse(float cx, float cy, float rx, float ry, int posX, int posY, float rotation, string fillColor, int strokeWidth, string strokeDashArray)
        {
            var xmlElement = document.CreateElement("ellipse", ns);
            xmlElement.SetAttribute("cx", cx.ToString());
            xmlElement.SetAttribute("cy", cy.ToString());
            xmlElement.SetAttribute("rx", rx.ToString());
            xmlElement.SetAttribute("ry", ry.ToString());
            xmlElement.SetAttribute("stroke", "black");
            xmlElement.SetAttribute("fill", fillColor);
            xmlElement.SetAttribute("stroke-width", strokeWidth.ToString());
            xmlElement.SetAttribute("stroke-dasharray", strokeDashArray);
            xmlElement.SetAttribute("transform", string.Format("translate({0}, {1}) rotate({2})", posX, posY, rotation));
            svg.AppendChild(xmlElement);
        }

        public void DrawText(float x, float y, string fontSize, string textAnchor, string label, string lineHeight = "1.2em")
        {
            var xmlElement = document.CreateElement("text", ns);
            xmlElement.SetAttribute("font-family", "Special Elite, sans-serif");
            xmlElement.SetAttribute("text-anchor", textAnchor);
            xmlElement.SetAttribute("x", x.ToString());
            xmlElement.SetAttribute("y", y.ToString());
            xmlElement.SetAttribute("font-size", fontSize);
            var flag = true;
            foreach (var innerText in label.Split('\n'))
            {
                var xmlElement2 = document.CreateElement("tspan", ns);
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    xmlElement2.SetAttribute("x", x.ToString());
                    xmlElement2.SetAttribute("dy", lineHeight);
                }
                xmlElement2.InnerText = innerText;
                xmlElement.AppendChild(xmlElement2);
            }
            svg.AppendChild(xmlElement);
        }

        public void DrawRect(float x, float y, float width, float height, string fillColor, string strokeWidth, string strokeDashArray)
        {
            var xmlElement = document.CreateElement("rect", ns);
            xmlElement.SetAttribute("x", x.ToString());
            xmlElement.SetAttribute("y", y.ToString());
            xmlElement.SetAttribute("width", width.ToString());
            xmlElement.SetAttribute("height", height.ToString());
            xmlElement.SetAttribute("stroke", "black");
            xmlElement.SetAttribute("fill", fillColor);
            xmlElement.SetAttribute("stroke-width", strokeWidth);
            xmlElement.SetAttribute("stroke-dasharray", strokeDashArray);
            svg.AppendChild(xmlElement);
        }

        public override string ToString()
        {
            string result;
            using (var stringWriterWithEncoding = new StringWriterWithEncoding(Encoding.UTF8))
            {
                using (var xmlWriter = XmlWriter.Create(stringWriterWithEncoding))
                {
                    svg.WriteTo(xmlWriter);
                    xmlWriter.Flush();
                }
                result = stringWriterWithEncoding.ToString();
            }
            return result;
        }

        private XmlDocument document;

        private XmlElement svg;

        private string ns = "http://www.w3.org/2000/svg";
    }

    public sealed class StringWriterWithEncoding : StringWriter
    {
        public StringWriterWithEncoding(Encoding encoding)
        {
            _encoding = encoding;
        }

        public override Encoding Encoding
        {
            get
            {
                return _encoding;
            }
        }

        private readonly Encoding _encoding;
    }
}
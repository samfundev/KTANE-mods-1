/*
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;


    // Token: 0x020006B7 RID: 1719
public class SVGGenerator
{
    // Token: 0x06002D3E RID: 11582 RVA: 0x000F3F34 File Offset: 0x000F2334
    public SVGGenerator(int width, int height)
    {
        this.document = new XmlDocument();
        this.svg = this.document.CreateElement("svg", this.ns);
        this.document.AppendChild(this.svg);
        this.svg.SetAttribute("width", "100%");
        this.svg.SetAttribute("height", "100%");
        this.svg.SetAttribute("viewBox", string.Format("0 0 {0} {1}", width, height));
        this.svg.SetAttribute("preserveAspectRatio", "xMidYMid meet");
        this.svg.SetAttribute("version", "1.1");
    }

    // Token: 0x06002D3F RID: 11583 RVA: 0x000F4005 File Offset: 0x000F2405
    public void DrawLine(float x1, float y1, float x2, float y2)
    {
        this.DrawLine(x1, y1, x2, y2, "1", string.Empty);
    }

    // Token: 0x06002D40 RID: 11584 RVA: 0x000F401C File Offset: 0x000F241C
    public void DrawLine(float x1, float y1, float x2, float y2, string strokeWidth, string strokeDashArray)
    {
        XmlElement xmlElement = this.document.CreateElement("line", this.ns);
        xmlElement.SetAttribute("x1", string.Empty + x1);
        xmlElement.SetAttribute("x2", string.Empty + x2);
        xmlElement.SetAttribute("y1", string.Empty + y1);
        xmlElement.SetAttribute("y2", string.Empty + y2);
        xmlElement.SetAttribute("stroke", "black");
        xmlElement.SetAttribute("stroke-width", strokeWidth);
        xmlElement.SetAttribute("stroke-dasharray", strokeDashArray);
        xmlElement.SetAttribute("fill", "none");
        this.svg.AppendChild(xmlElement);
    }

    // Token: 0x06002D41 RID: 11585 RVA: 0x000F40F4 File Offset: 0x000F24F4
    public void DrawCircle(float cx, float cy, float r, bool filled)
    {
        XmlElement xmlElement = this.document.CreateElement("circle", this.ns);
        xmlElement.SetAttribute("cx", string.Empty + cx);
        xmlElement.SetAttribute("cy", string.Empty + cy);
        xmlElement.SetAttribute("r", string.Empty + r);
        xmlElement.SetAttribute("stroke", "black");
        if (filled)
        {
            xmlElement.SetAttribute("fill", "grey");
        }
        else
        {
            xmlElement.SetAttribute("fill", "none");
        }
        xmlElement.SetAttribute("stroke-width", "1");
        this.svg.AppendChild(xmlElement);
    }

    // Token: 0x06002D42 RID: 11586 RVA: 0x000F41C4 File Offset: 0x000F25C4
    public void DrawEllipse(float cx, float cy, float rx, float ry, int posX, int posY, float rotation, string fillColor, int strokeWidth, string strokeDashArray)
    {
        XmlElement xmlElement = this.document.CreateElement("ellipse", this.ns);
        xmlElement.SetAttribute("cx", cx.ToString());
        xmlElement.SetAttribute("cy", cy.ToString());
        xmlElement.SetAttribute("rx", rx.ToString());
        xmlElement.SetAttribute("ry", ry.ToString());
        xmlElement.SetAttribute("stroke", "black");
        xmlElement.SetAttribute("fill", fillColor);
        xmlElement.SetAttribute("stroke-width", strokeWidth.ToString());
        xmlElement.SetAttribute("stroke-dasharray", strokeDashArray);
        xmlElement.SetAttribute("transform", string.Format("translate({0}, {1}) rotate({2})", posX, posY, rotation));
        this.svg.AppendChild(xmlElement);
    }

    // Token: 0x06002D43 RID: 11587 RVA: 0x000F42C4 File Offset: 0x000F26C4
    public void DrawText(float x, float y, string fontSize, string textAnchor, string label, string lineHeight = "1.2em")
    {
        XmlElement xmlElement = this.document.CreateElement("text", this.ns);
        xmlElement.SetAttribute("font-family", "Special Elite, sans-serif");
        xmlElement.SetAttribute("text-anchor", textAnchor);
        xmlElement.SetAttribute("x", x.ToString());
        xmlElement.SetAttribute("y", y.ToString());
        xmlElement.SetAttribute("font-size", fontSize);
        bool flag = true;
        foreach (string innerText in label.Split(new char[]
        {
            '\n'
        }))
        {
            XmlElement xmlElement2 = this.document.CreateElement("tspan", this.ns);
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
        this.svg.AppendChild(xmlElement);
    }

    // Token: 0x06002D44 RID: 11588 RVA: 0x000F43E0 File Offset: 0x000F27E0
    public void DrawRect(float x, float y, float width, float height, string fillColor, string strokeWidth, string strokeDashArray)
    {
        XmlElement xmlElement = this.document.CreateElement("rect", this.ns);
        xmlElement.SetAttribute("x", x.ToString());
        xmlElement.SetAttribute("y", y.ToString());
        xmlElement.SetAttribute("width", width.ToString());
        xmlElement.SetAttribute("height", height.ToString());
        xmlElement.SetAttribute("stroke", "black");
        xmlElement.SetAttribute("fill", fillColor);
        xmlElement.SetAttribute("stroke-width", strokeWidth);
        xmlElement.SetAttribute("stroke-dasharray", strokeDashArray);
        this.svg.AppendChild(xmlElement);
    }

    // Token: 0x06002D45 RID: 11589 RVA: 0x000F44A8 File Offset: 0x000F28A8
    public void Draw4SetVennDiagram(List<string> labels, List<string> strokeDashArrays)
    {
        int num = 0;
        this.DrawEllipse(0f, 0f, 300f, 160f, 350, 300, 45f, "none", 3, strokeDashArrays[num++]);
        this.DrawEllipse(0f, 0f, 300f, 160f, 450, 300, -45f, "none", 3, strokeDashArrays[num++]);
        this.DrawEllipse(0f, 0f, 300f, 160f, 250, 400, 45f, "none", 3, strokeDashArrays[num++]);
        this.DrawEllipse(0f, 0f, 300f, 160f, 550, 400, -45f, "none", 8, strokeDashArrays[num++]);
        num = 0;
        this.DrawText(225f, 150f, "3em", "middle", labels[num++], "1.2em");
        this.DrawText(575f, 150f, "3em", "middle", labels[num++], "1.2em");
        this.DrawText(100f, 350f, "3em", "middle", labels[num++], "1.2em");
        this.DrawText(700f, 350f, "3em", "middle", labels[num++], "1.2em");
        this.DrawText(185f, 250f, "2em", "middle", labels[num++], "1.2em");
        this.DrawText(400f, 225f, "2em", "middle", labels[num++], "1.2em");
        this.DrawText(615f, 250f, "2em", "middle", labels[num++], "1.2em");
        this.DrawText(400f, 590f, "2em", "middle", labels[num++], "1.2em");
        this.DrawText(260f, 495f, "2em", "middle", labels[num++], "1.2em");
        this.DrawText(530f, 495f, "2em", "middle", labels[num++], "1.2em");
        this.DrawText(305f, 350f, "2em", "middle", labels[num++], "1.2em");
        this.DrawText(500f, 350f, "2em", "middle", labels[num++], "1.2em");
        this.DrawText(335f, 530f, "2em", "middle", labels[num++], "1.2em");
        this.DrawText(465f, 530f, "2em", "middle", labels[num++], "1.2em");
        this.DrawText(400f, 450f, "2em", "middle", labels[num++], "1.2em");
        this.DrawText(400f, 80f, "3em", "middle", labels[num++], "1.2em");
    }

    // Token: 0x06002D46 RID: 11590 RVA: 0x000F4844 File Offset: 0x000F2C44
    public void DrawVennDiagramLegend(List<string> labels, List<string> strokeDashArrays)
    {
        int num = 0;
        this.DrawRect(2f, 2f, 250f, 180f, "none", "2", string.Empty);
        this.DrawLine(20f, 30f, 130f, 30f, "3", strokeDashArrays[num++]);
        this.DrawLine(20f, 70f, 130f, 70f, "3", strokeDashArrays[num++]);
        this.DrawLine(20f, 110f, 130f, 110f, "3", strokeDashArrays[num++]);
        this.DrawLine(20f, 150f, 130f, 150f, "8", strokeDashArrays[num++]);
        num = 0;
        string fontSize = "0.75em";
        string textAnchor = "left-middle";
        this.DrawText(150f, 27f, fontSize, textAnchor, labels[num++], "1.2em");
        this.DrawText(150f, 67f, fontSize, textAnchor, labels[num++], "1.2em");
        this.DrawText(150f, 113f, fontSize, textAnchor, labels[num++], "1.2em");
        this.DrawText(150f, 154f, fontSize, textAnchor, labels[num++], "1.2em");
    }

    // Token: 0x06002D47 RID: 11591 RVA: 0x000F49BC File Offset: 0x000F2DBC
    public override string ToString()
    {
        string result;
        using (StringWriterWithEncoding stringWriterWithEncoding = new StringWriterWithEncoding(Encoding.UTF8))
        {
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriterWithEncoding))
            {
                this.svg.WriteTo(xmlWriter);
                xmlWriter.Flush();
            }
            result = stringWriterWithEncoding.ToString();
        }
        return result;
    }

    // Token: 0x04002008 RID: 8200
    private XmlDocument document;

    // Token: 0x04002009 RID: 8201
    private XmlElement svg;

    // Token: 0x0400200A RID: 8202
    private string ns = "http://www.w3.org/2000/svg";
}

public sealed class StringWriterWithEncoding : StringWriter
{
    // Token: 0x0600371D RID: 14109 RVA: 0x0012AAC0 File Offset: 0x00128EC0
    public StringWriterWithEncoding(Encoding encoding)
    {
        this.encoding = encoding;
    }

    // Token: 0x1700074F RID: 1871
    // (get) Token: 0x0600371E RID: 14110 RVA: 0x0012AACF File Offset: 0x00128ECF
    public override Encoding Encoding
    {
        get { return this.encoding; }
    }

    // Token: 0x04002846 RID: 10310
    private readonly Encoding encoding;
}
*/
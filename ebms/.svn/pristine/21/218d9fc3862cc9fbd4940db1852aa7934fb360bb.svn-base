using System;
using System.Collections.Generic;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Design;
using System.IO;
using System.Collections;

namespace Lib
{
    /// <summary>
    ///绘制条形码
    /// </summary>
    public class BarCode
    {
        public BarCode()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }

        #region 根据字体产生条形码
        /// <summary>
        /// 根据条形码绘制图片
        /// </summary>
        /// <param name="strNumber">条形码</param>
        public void DrawingBarCode(string strNumber)
        {
            PrivateFontCollection fonts = new PrivateFontCollection();
            //39带数字
            //fonts.AddFontFile(HttpContext.Current.Server.MapPath(".") + "/BarCodeFonts/V100010_.TTF");
            //FontFamily ff = new FontFamily("C39HrP48DlTt", fonts);

            //39码
            strNumber = "*" + strNumber + "*";
            fonts.AddFontFile(HttpContext.Current.Server.MapPath(".") + "/3of9.TTF");
            FontFamily ff = new FontFamily("Free 3 of 9 Extended", fonts);

            //接近条形码
            //fonts.AddFontFile(HttpContext.Current.Server.MapPath(".") + "/BarCodeFonts/V100014_.TTF");
            //FontFamily ff = new FontFamily("C39P24DlTt", fonts);
            Font font = new Font(ff, 12);

            //设置图片大小
            Image img = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(img);
            SizeF fontSize = g.MeasureString(strNumber, font);
            int intWidth = Convert.ToInt32(fontSize.Width);
            int intHeight = Convert.ToInt32(fontSize.Height);
            g.Dispose();
            img.Dispose();

            img = new Bitmap(intWidth, intHeight);
            g = Graphics.FromImage(img);
            g.Clear(Color.White);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawString(strNumber, font, Brushes.Black, 0, 0);
            MemoryStream stream = new MemoryStream();
            img.Save(stream, ImageFormat.Jpeg);
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ContentType = "image/Jpeg";
            HttpContext.Current.Response.BinaryWrite(stream.ToArray());
            g.Dispose();
            img.Dispose();
        }

        /// <summary>
        /// 根据条形码绘制图片
        /// </summary>
        /// <param name="strNumber">条形码</param>
        /// <param name="intFontSize">字体大小</param>
        public void DrawingBarCode(string strNumber, int intFontSize)
        {

            PrivateFontCollection fonts = new PrivateFontCollection();
            //39带数字
            //fonts.AddFontFile(HttpContext.Current.Server.MapPath(".") + "/BarCodeFonts/V100010_.TTF");
            //FontFamily ff = new FontFamily("C39HrP48DlTt", fonts);

            //39码
            strNumber = "*" + strNumber + "*";
            fonts.AddFontFile(HttpContext.Current.Server.MapPath(".") + "/BarCodeFonts/FREE3OF9X.TTF");
            FontFamily ff = new FontFamily("Free 3 of 9 Extended", fonts);

            //接近条形码
            //fonts.AddFontFile(HttpContext.Current.Server.MapPath(".") + "/BarCodeFonts/V100014_.TTF");
            //FontFamily ff = new FontFamily("C39P24DlTt", fonts);
            Font font = new Font(ff, intFontSize);

            //设置图片大小
            Image img = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(img);
            SizeF fontSize = g.MeasureString(strNumber, font);
            int intWidth = Convert.ToInt32(fontSize.Width);
            int intHeight = Convert.ToInt32(fontSize.Height);
            g.Dispose();
            img.Dispose();

            img = new Bitmap(intWidth, intHeight);
            g = Graphics.FromImage(img);
            g.Clear(Color.White);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawString(strNumber, font, Brushes.Black, 0, 0);
            MemoryStream stream = new MemoryStream();
            img.Save(stream, ImageFormat.Jpeg);
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ContentType = "image/Jpeg";
            HttpContext.Current.Response.BinaryWrite(stream.ToArray());
            g.Dispose();
            img.Dispose();
        }
        #endregion

        #region 绘制Code39码

        /// <summary>
        /// 根据条形码绘制图片
        /// </summary>
        /// <param name="strNumber">条形码</param>
        public void DrawingBarCode39(string strNumber, int intFontSize, bool WithStart)
        {
            ViewFont = new Font("宋体", intFontSize);
            Image img = GetCodeImage(strNumber, Code39Model.Code39Normal, WithStart);
            MemoryStream stream = new MemoryStream();
            img.Save(stream, ImageFormat.Jpeg);
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.BufferOutput = true;
            HttpContext.Current.Response.ContentType = "image/Jpeg";
            HttpContext.Current.Response.BinaryWrite(stream.GetBuffer());
            HttpContext.Current.Response.Flush();
        }

        private Hashtable m_Code39 = new Hashtable();

        private byte m_Magnify = 0;
        /// <summary> 
        /// 放大倍数 
        /// </summary> 
        public byte Magnify { get { return m_Magnify; } set { m_Magnify = value; } }

        private int m_Height = 40;
        /// <summary> 
        /// 图形高 
        /// </summary> 
        public int Height { get { return m_Height; } set { m_Height = value; } }

        private Font m_ViewFont = null;
        /// <summary> 
        /// 字体大小 
        /// </summary> 
        public Font ViewFont { get { return m_ViewFont; } set { m_ViewFont = value; } }

        /**
         Code39码的编码规则是： 

           1、 每五条线表示一个字符； 

           2、 粗线表示1，细线表示0； 

           3、 线条间的间隙宽的表示1，窄的表示0； 

           4、 五条线加上它们之间的四条间隙就是九位二进制编码，而且这九位中必定有三位是1，所以称为39码； 

           5、 条形码的首尾各一个＊标识开始和结束

         */
        public BarCode(Boolean IsCode39)
        {

            m_Code39.Add("A", "1101010010110");
            m_Code39.Add("B", "1011010010110");
            m_Code39.Add("C", "1101101001010");
            m_Code39.Add("D", "1010110010110");
            m_Code39.Add("E", "1101011001010");
            m_Code39.Add("F", "1011011001010");
            m_Code39.Add("G", "1010100110110");
            m_Code39.Add("H", "1101010011010");
            m_Code39.Add("I", "1011010011010");
            m_Code39.Add("J", "1010110011010");
            m_Code39.Add("K", "1101010100110");
            m_Code39.Add("L", "1011010100110");
            m_Code39.Add("M", "1101101010010");
            m_Code39.Add("N", "1010110100110");
            m_Code39.Add("O", "1101011010010");
            m_Code39.Add("P", "1011011010010");
            m_Code39.Add("Q", "1010101100110");
            m_Code39.Add("R", "1101010110010");
            m_Code39.Add("S", "1011010110010");
            m_Code39.Add("T", "1010110110010");
            m_Code39.Add("U", "1100101010110");
            m_Code39.Add("V", "1001101010110");
            m_Code39.Add("W", "1100110101010");
            m_Code39.Add("X", "1001011010110");
            m_Code39.Add("Y", "1100101101010");
            m_Code39.Add("Z", "1001101101010");
            m_Code39.Add("0", "1010011011010");
            m_Code39.Add("1", "1101001010110");
            m_Code39.Add("2", "1011001010110");
            m_Code39.Add("3", "1101100101010");
            m_Code39.Add("4", "1010011010110");
            m_Code39.Add("5", "1101001101010");
            m_Code39.Add("6", "1011001101010");
            m_Code39.Add("7", "1010010110110");
            m_Code39.Add("8", "1101001011010");
            m_Code39.Add("9", "1011001011010");
            m_Code39.Add("+", "1001010010010");
            m_Code39.Add("-", "1001010110110");
            m_Code39.Add("*", "1001011011010");
            m_Code39.Add("/", "1001001010010");
            m_Code39.Add("%", "1010010010010");
            m_Code39.Add("$", "1001001001010");
            m_Code39.Add(".", "1100101011010");
            m_Code39.Add(" ", "1001101011010");

        }

        public enum Code39Model
        {
            /// <summary> 
            /// 基本类别 1234567890ABC 
            /// </summary> 
            Code39Normal,
            /// <summary> 
            /// 全ASCII方式 +A+B 来表示小写 
            /// </summary> 
            Code39FullAscII
        }
        /// <summary> 
        /// 获得条码图形 
        /// </summary> 
        /// <param name="p_Text">文字信息</param> 
        /// <param name="p_Model">类别</param> 
        /// <param name="p_StarChar">是否增加前后*号</param> 
        /// <returns>图形</returns> 
        public Bitmap GetCodeImage(string p_Text, Code39Model p_Model, bool p_StarChar)
        {
            string _ValueText = "";
            string _CodeText = "";
            char[] _ValueChar = null;
            switch (p_Model)
            {
                case Code39Model.Code39Normal:
                    _ValueText = p_Text.ToUpper();
                    break;
                default:
                    _ValueChar = p_Text.ToCharArray();
                    for (int i = 0; i != _ValueChar.Length; i++)
                    {
                        if ((int)_ValueChar[i] >= 97 && (int)_ValueChar[i] <= 122)
                        {
                            _ValueText += "+" + _ValueChar[i].ToString().ToUpper();

                        }
                        else
                        {
                            _ValueText += _ValueChar[i].ToString();
                        }
                    }
                    break;
            }
            _ValueChar = _ValueText.ToCharArray();

            if (p_StarChar == true) _CodeText += m_Code39["*"];

            for (int i = 0; i != _ValueChar.Length; i++)
            {
                if (p_StarChar == true && _ValueChar[i] == '*') throw new Exception("带有起始符号不能出现*");

                object _CharCode = m_Code39[_ValueChar[i].ToString()];
                if (_CharCode == null) throw new Exception("不可用的字符" + _ValueChar[i].ToString());
                _CodeText += _CharCode.ToString();
            }
            if (p_StarChar == true) _CodeText += m_Code39["*"];
            Bitmap _CodeBmp = GetImage(_CodeText);
            GetViewImage(_CodeBmp, p_Text);
            return _CodeBmp;
        }

        /// <summary> 
        /// 绘制编码图形 
        /// </summary> 
        /// <param name="p_Text">编码</param> 
        /// <returns>图形</returns> 
        private Bitmap GetImage(string p_Text)
        {
            char[] _Value = p_Text.ToCharArray();

            //宽 == 需要绘制的数量*放大倍数 + 两个字的宽    
            Bitmap _CodeImage = new Bitmap(_Value.Length * ((int)m_Magnify + 1), (int)m_Height);
            Graphics _Garphics = Graphics.FromImage(_CodeImage);

            _Garphics.FillRectangle(Brushes.White, new Rectangle(0, 0, _CodeImage.Width, _CodeImage.Height));

            int _LenEx = 0;
            for (int i = 0; i != _Value.Length; i++)
            {
                int _DrawWidth = m_Magnify + 1;
                if (_Value[i] == '1')
                {
                    _Garphics.FillRectangle(Brushes.Black, new Rectangle(_LenEx, 0, _DrawWidth, m_Height));

                }
                else
                {
                    _Garphics.FillRectangle(Brushes.White, new Rectangle(_LenEx, 0, _DrawWidth, m_Height));
                }
                _LenEx += _DrawWidth;
            }
            _Garphics.Dispose();
            return _CodeImage;
        }

        /// <summary> 
        /// 绘制文字,文字宽度大于图片宽度将不显示 
        /// </summary> 
        /// <param name="p_CodeImage">图形</param> 
        /// <param name="p_Text">文字</param> 
        private void GetViewImage(Bitmap p_CodeImage, string p_Text)
        {
            if (m_ViewFont == null) return;
            Graphics _Graphics = Graphics.FromImage(p_CodeImage);
            SizeF _FontSize = _Graphics.MeasureString(p_Text, m_ViewFont);

            if (_FontSize.Width > p_CodeImage.Width || _FontSize.Height > p_CodeImage.Height - 20)
            {
                _Graphics.Dispose();
                return;
            }
            int _StarHeight = p_CodeImage.Height - (int)_FontSize.Height;

            _Graphics.FillRectangle(Brushes.White, new Rectangle(0, _StarHeight, p_CodeImage.Width, (int)_FontSize.Height));

            int _StarWidth = (p_CodeImage.Width - (int)_FontSize.Width) / 2;

            _Graphics.DrawString(p_Text, m_ViewFont, Brushes.Black, _StarWidth, _StarHeight);

            _Graphics.Dispose();

        }
        #endregion

    }

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows;

namespace MyDiary
{
    internal class RichTextHelpers
    {
        public static string XamlToPlainText(string xamlText)
        {
            FlowDocument flowDoc = new FlowDocument();

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xamlText)))
            {
                TextRange range = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd);
                range.Load(ms, DataFormats.Xaml);

                return range.Text;
            }
        }
    }
}

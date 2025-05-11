using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary
{
    public class DiaryEntryListItem
    {
        public int DiaryId {  get; set; }
        public DateTime DiaryDate {  get; set; }
        public string DiaryText {  get; set; }

        public string PlainDiaryText
        {
            get
            {
                return RichTextHelpers.XamlToPlainText(this.DiaryText).Replace('\n', ' ').Replace('\r', ' ');
            }
        }
    }
}

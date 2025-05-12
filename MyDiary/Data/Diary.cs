using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.Data
{
    public class Diary
    {
        public int DiaryId { get; set; }
        public DateTime DiaryDate { get; set; }
        public required string DiaryText { get; set; }
    }
}

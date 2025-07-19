using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary
{
    public class DiaryEntryListItem : INotifyPropertyChanged
    {
        public int DiaryId {  get; set; }
        public DateTime DiaryDate {  get; set; }
        private string _diaryText;
        private string _plainDiaryText;

        public string DiaryText 
        {
            get => _diaryText;
            set
            {
                _diaryText = value;
                OnPropertyChanged(nameof(DiaryText));
            }
        }

        public string PlainDiaryText
        {
            get => _plainDiaryText;
            set
            {
                _plainDiaryText = value;
                OnPropertyChanged(nameof(PlainDiaryText));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

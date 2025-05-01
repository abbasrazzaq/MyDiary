using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

/*
 *  TODO:
 *      - Have two views
 *          1 - For seeing a list of entries
 *          2 - For adding an entry
 */

namespace MyDiary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DiaryContext db = new DiaryContext();

        public MainWindow()
        {
            InitializeComponent();

            dateDiaryEntry.SelectedDate = DateTime.Now;
            txtDiaryEntry.AppendText("Dear Diary,\n");
            txtDiaryEntry.Focus();
            txtDiaryEntry.CaretPosition = txtDiaryEntry.Document.ContentEnd;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(txtDiaryEntry.Document.ContentStart, txtDiaryEntry.Document.ContentEnd);

            DateTime diaryDate = DateTime.Now;
            
            // TODO: Check if not select and asks user to enter a date
            if(dateDiaryEntry.SelectedDate is not null)
            {
                diaryDate = dateDiaryEntry.SelectedDate.Value;
            }

            // Create
            db.Add(new Diary { DiaryText = textRange.Text, DiaryDate = diaryDate });
            db.SaveChangesAsync();


            //if (!string.IsNullOrWhiteSpace(txtName.Text) && !lstNames.Items.Contains(txtName.Text))
            //{
            //    lstNames.Items.Add(txtName.Text);
            //    txtName.Clear();
            //}
        }
    }
}
using Newtonsoft.Json.Linq;
using System.Windows;

namespace JSONReader
{
    /// <summary>
    /// Interaction logic for CopyJSONWindow.xaml
    /// </summary>
    public partial class CopyJSONWindow : Window
    {
        public CopyJSONWindow()
        {
            InitializeComponent();
        }

        private void LoadCopyJson_Click(object sender, RoutedEventArgs e)
        {
            JObject json = JObject.Parse(txtJson.Text);

            MainWindow.MainWindowInstance.InitNodes(json);

            Close();
        }
    }
}

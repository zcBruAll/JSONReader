using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace JSONReader
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<TreeNode> ParentNodes { get; set; } = [];
        public ObservableCollection<TreeNode> Nodes { get; set; } = [];

        public MainWindow()
        {
            InitializeComponent();

            JObject json = InitJSON();
            InitNodes(json);
            
            ParentNodes.Clear();
            foreach (TreeNode node in Nodes)
            {
                ParentNodes.Add(node.Clone());
            }

            FilterOnlyParent(ParentNodes);

            DataContext = this;
        }

        private static JObject InitJSON()
        {
            string path = "C:/temp/example.json";
            string jsonString = File.ReadAllText(path);
            return JObject.Parse(jsonString);
        }

        private void InitNodes(JObject json)
        {
            var rootNode = TreeNode.FromJToken("Root", json);

            Nodes = [rootNode];
        }

        private static void FilterOnlyParent(ObservableCollection<TreeNode> treeNodes)
        {
            var nodesToRemove = new List<TreeNode>();

            foreach (var node in treeNodes)
            {
                if (node.Children.Count == 0)
                    nodesToRemove.Add(node);
                else
                    FilterOnlyParent(node.Children);
            }

            foreach (var node in nodesToRemove) 
                treeNodes.Remove(node);
        }

        private void trvParentNode_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            List<(string name, string value)> propertiesToDisplay = [];

            TreeNode nodeToSearch = (TreeNode)e.NewValue;
            TreeNode foundedNode = nodeToSearch.GetNode(Nodes);
            DisplayNodes(foundedNode);
        }

        private void DisplayNodes(TreeNode treeNode)
        {
            grdProperties.Children.Clear();
            grdProperties.RowDefinitions.Clear();
            grdProperties.ColumnDefinitions.Clear();

            int columnNumber = 2;
            int currentColumn = 0;
            int currentRow = 0;

            for (int i = 0; i < columnNumber; i++)
                grdProperties.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) });

            foreach (var node in treeNode.Children)
            {
                grdProperties.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                GroupBox groupBox = new GroupBox()
                {
                    Header = node.Name,
                    Margin = new Thickness(5)
                };

                TextBox textBox = new TextBox()
                {
                    BorderThickness = new Thickness(0)
                };

                textBox.SetBinding(TextBox.TextProperty, new Binding("Value")
                {
                    Source = node,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

                groupBox.Content = textBox;

                Grid.SetRow(groupBox, currentRow);
                Grid.SetColumn(groupBox, currentColumn);
                grdProperties.Children.Add(groupBox);

                currentColumn++;

                if (currentColumn >= columnNumber)
                {
                    currentColumn = 0;
                    currentRow++;
                }
            }
        }
    }
}

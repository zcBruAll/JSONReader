using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JSONReader
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<TreeNode> ParentNodes { get; set; } = [];
        public ObservableCollection<TreeNode> Nodes { get; set; } = [];

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void InitJSON(string path)
        {
            string jsonString = File.ReadAllText(path);
            JObject json = JObject.Parse(jsonString);

            InitNodes(json);

            ParentNodes.Clear();
            foreach (TreeNode node in Nodes)
            {
                ParentNodes.Add(node.Clone());
            }

            FilterOnlyParent(ParentNodes);
        }

        private void InitNodes(JObject json)
        {
            Nodes.Clear();
            TreeNode rootNode = TreeNode.FromJToken("Root", json);

            Nodes = [rootNode];
        }

        private static void FilterOnlyParent(ObservableCollection<TreeNode> treeNodes)
        {
            List<TreeNode> nodesToRemove = [];

            foreach (TreeNode node in treeNodes)
            {
                if (node.Children.Count == 0)
                    nodesToRemove.Add(node);
                else
                    FilterOnlyParent(node.Children);
            }

            foreach (TreeNode node in nodesToRemove) 
                treeNodes.Remove(node);
        }

        private void trvParentNode_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeNode nodeToSearch = (TreeNode)e.NewValue;
            if (nodeToSearch != null)
            {
                TreeNode foundedNode = nodeToSearch.GetNode(Nodes);
                DisplayNodes(foundedNode);
            }
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

            foreach (TreeNode node in treeNode.Children)
            {
                grdProperties.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                GroupBox groupBox = new()
                {
                    Header = node.Name,
                    Margin = new Thickness(5)
                };

                TextBox textBox = new()
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

        private void LoadTreeNode_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                InitJSON(openFileDialog.FileName);
            }
        }

        private void SaveTreeNode_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                JObject rootObject = new JObject();

                foreach (var node in Nodes[0].Children)
                {
                    JToken jsonObject = TreeNode.CreateJTokenFromTreeNode(node);
                    rootObject[node.Name] = jsonObject;
                }

                File.WriteAllText(saveFileDialog.FileName, rootObject.ToString());
            }
        }
    }
}

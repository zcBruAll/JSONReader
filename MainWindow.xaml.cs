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

        public static MainWindow MainWindowInstance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            MainWindowInstance = this;

            DataContext = this;
        }

        public void InitJSON(string path)
        {
            string jsonString = File.ReadAllText(path);
            JObject json = JObject.Parse(jsonString);

            InitNodes(json);
        }

        public void InitNodes(JObject json)
        {
            Nodes.Clear();
            TreeNode rootNode = TreeNode.FromJToken("Root", json);

            Nodes = [rootNode];

            ParentNodes.Clear();
            foreach (TreeNode node in Nodes)
            {
                ParentNodes.Add(node.Clone());
            }

            FilterOnlyParent(ParentNodes);
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

                if (node.Value is null)
                {
                    groupBox.Tag = node.Id;
                    groupBox.MouseDoubleClick += GroupBox_MouseDoubleClick;
                    textBox.IsReadOnly = true;
                } else {
                    textBox.SetBinding(TextBox.TextProperty, new Binding("Value")
                    {
                        Source = node,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });
                }

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

        private void GroupBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) 
        {
            GroupBox groupBox = sender as GroupBox;
            SetSelectedNode((Guid)groupBox.Tag);
        }

        private void SetSelectedNode(Guid guid)
        {
            foreach (TreeNode node in trvParentNode.ItemsSource)
            {
                TreeViewItem item = FindNodeInTreeView(trvParentNode, node, guid);
                if (item != null)
                {
                    item.IsSelected = true;
                    item.Focus();
                    break;
                }
            }
        }

        private TreeViewItem FindNodeInTreeView(ItemsControl parent, TreeNode node, Guid guid)
        {
            if (node.Id == guid)
            {
                return parent.ItemContainerGenerator.ContainerFromItem(node) as TreeViewItem;
            }

            if (node.Children != null)
            {
                TreeViewItem parentItem = parent.ItemContainerGenerator.ContainerFromItem(node) as TreeViewItem;
                if (parentItem != null && parentItem.Items.Count > 0)
                {
                    parentItem.IsExpanded = true;
                    parentItem.UpdateLayout();

                    foreach (TreeNode child in node.Children)
                    {
                        TreeViewItem childItem = FindNodeInTreeView(parentItem, child, guid);
                        if (childItem != null)
                        {
                            return childItem;
                        }
                    }
                }
            }

            return null;
        }


        private void LoadTreeNodeFromFile_Click(object sender, RoutedEventArgs e)
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

        private void LoadTreeNodeFromCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyJSONWindow copyJSONWindow = new();
            copyJSONWindow.ShowDialog();
        }
    }
}

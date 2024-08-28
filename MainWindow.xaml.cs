using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

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

            TreeNode foundedNode = GetNode((TreeNode)e.NewValue, Nodes);
        }

        private TreeNode? GetNode(TreeNode nodeToSearch, ObservableCollection<TreeNode> treeNodes)
        {
            foreach (var node in treeNodes)
            {
                if (node.Id == nodeToSearch.Id)
                    return node;

                TreeNode foundNode = GetNode((TreeNode)nodeToSearch, node.Children);
                if (foundNode != null)
                    return foundNode;
            }

            return null;
        }
    }
}

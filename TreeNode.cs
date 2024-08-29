using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JSONReader
{
    public class TreeNode(string name)
    {
        private string _value;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = name;
        public string Value 
        {
            get => _value; 
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TreeNode> Children { get; set; } = [];

        public static TreeNode FromJToken(string name, JToken token)
        {
            var node = new TreeNode(name);

            if (token is JObject obj)
            {
                foreach (var property in obj.Properties())
                {
                    var childNode = FromJToken(property.Name, property.Value);
                        node.Children.Add(childNode);
                }
            }
            else if (token is JArray array)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    var childNode = FromJToken($"[{i}]", array[i]);
                        node.Children.Add(childNode);
                }
            }
            else
                node.Value = token.ToString();

            return node;
        }

        public TreeNode Clone()
        {
            var newNode = new TreeNode(Name)
            {
                Id = this.Id,
                Value = this.Value
            };
            foreach (var child in Children)
            {
                newNode.Children.Add(child.Clone());
            }
            return newNode;
        }

        public TreeNode? GetNode(ObservableCollection<TreeNode> treeNodes)
        {
            foreach (var node in treeNodes)
            {
                if (node.Id == Id)
                    return node;

                TreeNode foundNode = GetNode(node.Children);
                if (foundNode != null)
                    return foundNode;
            }

            return null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}

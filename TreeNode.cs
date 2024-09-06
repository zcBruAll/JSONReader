using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JSONReader
{
    public class TreeNode(string name)
    {
        private object? _value;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = name;
        public object? Value 
        {
            get => _value; 
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public bool IsArray { get; set; } = false;

        public ObservableCollection<TreeNode> Children { get; set; } = [];

        public static TreeNode FromJToken(string name, JToken token)
        {
            TreeNode node = new(name);

            if (token is JObject obj)
            {
                foreach (JProperty property in obj.Properties())
                {
                    TreeNode childNode = FromJToken(property.Name, property.Value);
                        node.Children.Add(childNode);
                }
            }
            else if (token is JArray array)
            {
                node.IsArray = true;
                for (int i = 0; i < array.Count; i++)
                {
                    TreeNode childNode = FromJToken($"[{i}]", array[i]);
                        node.Children.Add(childNode);
                }
            }
            else {
                node.Value = token.Type switch
                {
                    JTokenType.Integer => token.ToObject<int>(),
                    JTokenType.Float => token.ToObject<double>(),
                    JTokenType.Boolean => token.ToObject<bool>(),
                    JTokenType.String => token.ToObject<string>(),
                    _ => token.ToString()
                };
            }

            return node;
        }

        public TreeNode Clone()
        {
            TreeNode newNode = new TreeNode(Name)
            {
                Id = Id,
                Value = Value
            };
            foreach (TreeNode child in Children)
            {
                newNode.Children.Add(child.Clone());
            }
            return newNode;
        }

        public TreeNode? GetNode(ObservableCollection<TreeNode> treeNodes)
        {
            foreach (TreeNode node in treeNodes)
            {
                if (node.Id == Id)
                    return node;

                TreeNode? foundNode = GetNode(node.Children);
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

        public static JToken CreateJTokenFromTreeNode(TreeNode treeNode)
        {
            if (treeNode.Children.Count == 0)
            {
                return JToken.FromObject(treeNode.Value);
            }

            if (treeNode.IsArray)
            {
                JArray array = new JArray();

                foreach (var child in treeNode.Children)
                {
                    array.Add(CreateJTokenFromTreeNode(child));
                }

                return array;
            }
            else
            {
                JObject jsonObject = new JObject();

                foreach (var child in treeNode.Children)
                {
                    jsonObject[child.Name] = CreateJTokenFromTreeNode(child);
                }

                return jsonObject;
            }
        }

    }

}

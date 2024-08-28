using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace JSONReader
{
    public class TreeNode(string name)
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = name;
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

            return node;
        }

        public TreeNode Clone()
        {
            var newNode = new TreeNode(Name)
            {
                Id = this.Id
            };
            foreach (var child in Children)
            {
                newNode.Children.Add(child.Clone());
            }
            return newNode;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public class UiContainerNode : UiNode
    {
        protected UiNode[] Childs = EmptyChilds;

        public UiContainerNode(string name, UiNodeType type)
            : base(name, type)
        {
        }

        public override ImageSource Icon
        {
            get
            {
                switch (Type)
                {
                    case UiNodeType.Group:
                        return Icons.PackageIcon;
                }
                
                return Icons.FolderIcon;
            }
        }

        public IEnumerable<UiNode> BindableHierarchyChilds
        {
            get { return RequestChilds().Where(n => n.Type <= UiNodeType.Directory).Order(UiNodeComparer.Instance); }
        }

        public IEnumerable<UiNode> BindableChilds
        {
            get { return GetChilds().Order(UiNodeComparer.Instance); }
        }

        public override UiNode[] GetChilds()
        {
            return Childs;
        }

        protected internal virtual UiNode[] SetChilds(UiNode[] childs)
        {
            return Childs = childs;
        }

        protected virtual UiNode[] RequestChilds()
        {
            return Childs;
        }

        public IEnumerable<UiNode> EnumerateNodes(UiNodePath path)
        {
            return (EnumerateNodes(path, 0));
        }

        private IEnumerable<UiNode> EnumerateNodes(UiNodePath path, int level)
        {
            UiNodePathElement element = path[level];
            if (element == null)
                yield break;

            foreach (UiNode node in GetChilds().Where(node => element.IsMatch(node)))
            {
                if (path.IsLast(level))
                {
                    yield return node;
                }
                else
                {
                    UiContainerNode container = node as UiContainerNode;
                    if (container == null)
                        continue;

                    foreach (UiNode child in container.EnumerateNodes(path, level + 1))
                        yield return child;
                }
            }
        }

        public IEnumerable<IUiLeaf> EnumerateCheckedLeafs(Wildcard wildcard, bool parentChecked)
        {
            foreach (UiNode node in GetChilds())
            {
                if (!parentChecked && node.IsChecked == false)
                    continue;

                // Родительский контейнер может быть помечен, а вложенный нет
                // Это сделано специально для отложенной загрузки вложенных элементов
                bool isChecked = parentChecked || node.IsChecked == true;

                UiContainerNode container = node as UiContainerNode;
                if (container != null)
                {
                    foreach (IUiLeaf child in container.EnumerateCheckedLeafs(wildcard, isChecked))
                        yield return child;
                }
                else
                {
                    IUiLeaf leaf = node as IUiLeaf;
                    if (leaf != null && isChecked && wildcard.IsMatch(leaf.Name))
                        yield return leaf;
                }
            }
        }

        public void AbsorbSingleChildContainer()
        {
            UiNode[] childs = GetChilds();
            if (childs.Length != 1)
                return;

            UiContainerNode singleChild = childs[0] as UiContainerNode;
            if (singleChild == null || singleChild.Type != UiNodeType.Directory)
                return;

            childs = singleChild.GetChilds();
            foreach (UiNode child in childs)
                child.Parent = this;

            SetChilds(childs);

            singleChild.Parent = null;
            singleChild.SetChilds(EmptyChilds);
            Name = Name + '/' + singleChild.Name;
        }
    }
}
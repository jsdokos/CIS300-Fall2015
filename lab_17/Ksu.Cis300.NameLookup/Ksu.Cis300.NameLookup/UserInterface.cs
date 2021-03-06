﻿/* UserInterface.cs
 * Author: Jacob dokos
 * code: 03 63 95 23
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Ksu.Cis300.ImmutableBinaryTrees;
using KansasStateUniversity.TreeViewer2;

namespace Ksu.Cis300.NameLookup
{
    /// <summary>
    /// A GUI for a program that retrieves information about last names in
    /// a sample of US data.
    /// </summary>
    public partial class UserInterface : Form
    {
        /// <summary>
        /// The information on all the names.
        /// </summary>
        private BinaryTreeNode<NameInformation> _names = null;

        /// <summary>
        /// Constructs a new GUI.
        /// </summary>
        public UserInterface()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles a Click event on the Open Data File button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uxOpen_Click(object sender, EventArgs e)
        {
            if (uxOpenDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _names = GetAllInformation(uxOpenDialog.FileName);
                    new TreeForm(_names, 100).Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Handles a Click event on the Get Statistics button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uxLookup_Click(object sender, EventArgs e)
        {
            string name = uxName.Text.Trim().ToUpper();
            NameInformation info = GetInformation(name, _names);
            if (info.Name == null)
            {
                MessageBox.Show("Name not found.");
                uxRank.Text = "";
                uxFrequency.Text = "";
            }
            else
            {
                uxFrequency.Text = info.Frequency.ToString();
                uxRank.Text = info.Rank.ToString();
                return;
            }
        }

        /// <summary>
        /// Reads the given file and forms a binary search tree from its contents.
        /// </summary>
        /// <param name="fn">The name of the file.</param>
        /// <returns>The binary search tree containing all the name information.</returns>
        private BinaryTreeNode<NameInformation> GetAllInformation(string fn)
        {
            BinaryTreeNode<NameInformation> names = null;
            using (StreamReader input = new StreamReader(fn))
            {
                while (!input.EndOfStream)
                {
                    string name = input.ReadLine().Trim();
                    float freq = Convert.ToSingle(input.ReadLine());
                    int rank = Convert.ToInt32(input.ReadLine());
                    names = Add(new NameInformation(name, freq, rank), names);
                }
            }
            return names;
        }

        /// <summary>
        /// Gets the information associated with the given name in the given binary search tree.
        /// If the given name is not in the tree, returns a NameInformation with a null name.
        /// </summary>
        /// <param name="name">The name to look for.</param>
        /// <param name="t">The binary search tree to look in.</param>
        /// <returns>The NameInformation containing the given name, or a NameInformation with a null name
        /// if the tree does not contain the name.</returns>
        private NameInformation GetInformation(string name, BinaryTreeNode<NameInformation> t)
        {
            if (t == null)
            {
                return new NameInformation();
            }
            else
            {
                int comp = name.CompareTo(t.Data.Name);
                if (comp == 0)
                {
                    return t.Data;
                }
                else if (comp < 0)
                {
                    return GetInformation(name, t.LeftChild);
                }
                else
                {
                    return GetInformation(name, t.RightChild);
                }
            }
        }

        /// <summary>
        /// Builds the binary search tree that results from adding the given NameInformation to the given tree.
        /// </summary>
        /// <param name="info">The data to add to the tree.</param>
        /// <param name="t">The binary search tree to which the data will be added.</param>
        /// <returns>The resulting tree.</returns>
        private BinaryTreeNode<NameInformation> Add(NameInformation nameInformation, BinaryTreeNode<NameInformation> t)
        {
            if (t == null)
            {
                return new BinaryTreeNode<NameInformation>(nameInformation, null, null);
            }
            else
            {
                int comp = nameInformation.Name.CompareTo(t.Data.Name);
                if (comp == 0)
                {
                    throw new ArgumentException();
                }
                else if (comp < 0)
                {
                    return new BinaryTreeNode<NameInformation>(t.Data, Add(nameInformation, t.LeftChild), t.RightChild);
                }
                else
                {
                    return new BinaryTreeNode<NameInformation>(t.Data, t.LeftChild, Add(nameInformation, t.RightChild));
                }
            }
        }

        /// <summary>
        /// Handles a Remove event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uxRemove_Click(object sender, EventArgs e)
        {
            bool removed;
            _names = Remove(uxName.Text.Trim().ToUpper(), _names, out removed);
            if (removed)
            {
                MessageBox.Show("Name removed.");
                new TreeForm(_names, 100).Show();
            }
            else
            {
                MessageBox.Show("Name not found.");
            }
            uxName.Text = "";
            uxFrequency.Text = "";
            uxRank.Text = "";
        }

        /// <summary>
        /// Removes the smallest element from the given binary search tree, which must not be null.
        /// </summary>
        /// <param name="t">The binary search tree from which to remove the minimum.</param>
        /// <param name="min">The data removed.</param>
        /// <returns>The resulting binary search tree.</returns>
        private BinaryTreeNode<NameInformation> RemoveMin(BinaryTreeNode<NameInformation> t, out NameInformation min)
        {
            if (t.LeftChild == null)
            {
                min = t.Data;
                return t.RightChild;
            }
            else
            {
                return new BinaryTreeNode<NameInformation>(t.Data, RemoveMin(t.LeftChild, out min), t.RightChild);
            }

        }

        /// <summary>
        /// Builds the binary search tree that results from removing the NameInformation containing the given name from the given tree.
        /// </summary>
        /// <param name="name">The name to remove.</param>
        /// <param name="t">The binary search tree from which to remove the name.</param>
        /// <param name="removed">Indicates whether the name was found.</param>
        /// <returns>The resulting binary search tree.</returns>
        private BinaryTreeNode<NameInformation> Remove(string name, BinaryTreeNode<NameInformation> t, out bool removed)
        {
            if (t == null)
            {
                removed = false;
                return t;
            }
            else
            {
                if (t.Data.Name == name)
                {
                    removed = true;
                    if (t.LeftChild == null && t.RightChild == null)
                    {
                        return null;
                    }
                    else if (t.RightChild == null)
                    {
                        return t.LeftChild;
                    }
                    else if (t.LeftChild == null)
                    {
                        return t.RightChild;
                    }
                    else //if (t.LeftChild != null && t.RightChild != null)
                    {
                        removed = true;
                        NameInformation newTemp;
                        BinaryTreeNode<NameInformation> newRight = RemoveMin(t.RightChild, out newTemp);
                        BinaryTreeNode<NameInformation> newLeft = t.LeftChild;

                        return new BinaryTreeNode<NameInformation>(newTemp, newLeft, newRight);
                    }
                }
                else if (t.Data.Name.CompareTo(name) > 0)
                {
                    BinaryTreeNode<NameInformation> newLeft = Remove(name, t.LeftChild, out removed);
                    BinaryTreeNode<NameInformation> newRight = t.RightChild;
                    NameInformation newData = t.Data;
                    return new BinaryTreeNode<NameInformation>(newData, newLeft, newRight);
                }
                else //if (t.Data.Name.CompareTo(name) < 0)
                {
                    BinaryTreeNode<NameInformation> newRight = Remove(name, t.RightChild, out removed);
                    BinaryTreeNode<NameInformation> newLeft = t.LeftChild;
                    NameInformation newData = t.Data;
                    return new BinaryTreeNode<NameInformation>(newData, newLeft, newRight);
                }
            }
            
            
        }
    }
}

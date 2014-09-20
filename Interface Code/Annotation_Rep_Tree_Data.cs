using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SenseCamBrowser1
{


    




    public class Annotation_Rep_Tree_Data
    {
        private static string ROOT_NAME = "xyzgata";

        public string Name { get; set; }
        public string Desc { get; set; }
        public List<Annotation_Rep_Tree_Data> Children { get; set; }


        public Annotation_Rep_Tree_Data(string name, string desc)
        {
            this.Name = name;
            this.Desc = desc;
            this.Children = new List<Annotation_Rep_Tree_Data>();
        } //close constructor for Annotation_Rep_Tree_Data()...


        public static Annotation_Rep_Tree_Data convert_delimited_string_collection_to_tree_hierarchy(List<string> list_of_items_semicolon_delimited)
        {
            //input items look like: <mainCategory;subCategory,description>
            Annotation_Rep_Tree_Data root_node = new Annotation_Rep_Tree_Data(ROOT_NAME, ""); //firstly declare an overall root node...
            Annotation_Rep_Tree_Data temp_root_node; //we'll need this temporary root node when traversing down the levels allowing us to carry out a form of recursion..

            string[] new_tree_element; //to collect the various levels as delimited in our flat text file store...
            string annotation = "";
            string desc = "";
            foreach (string delimited_item in list_of_items_semicolon_delimited) //e.g. social interaction;2-4 people
            {
                annotation = delimited_item.Split(',')[0];
                desc = delimited_item.Split(',')[1];
                new_tree_element = annotation.Split(';'); //e.g. element 1 = social interaction ... element 2 = 2-4 people...
                
                temp_root_node = root_node; //now let's work our way down from the root through each level of depth
                for(int depth_counter=0; depth_counter<new_tree_element.Length; depth_counter++)
                    temp_root_node = add_element_to_appropriate_child_node_which_is_returned(
                            temp_root_node, new_tree_element[depth_counter], desc); //we insert the new element (for each level of depth) to the relevant child position below, which is then returned
                
            } //close foreach (string delimited_item in list_of_items_semicolon_delimited)...

            return root_node; //now return the root node of our new tree hierarchy...
        } //close method convert_delimited_string_collection_to_tree_hierarchy()...


        private static Annotation_Rep_Tree_Data add_element_to_appropriate_child_node_which_is_returned(
                Annotation_Rep_Tree_Data root_node,
                string new_element,
                string desc)
        {
            //let's see if this "new" element, is already recorded as a child in the root node anyways...
            foreach (Annotation_Rep_Tree_Data child in root_node.Children)
            {
                if (child.Name.Equals(new_element))
                    return child; //return this child element which has been recorded already...
            } //close foreach (Annotation_Rep_Tree_Data child in root_node.Children)...

            //if the "new" element is unseen as a child for this root node (i.e. return call wasn't invoked above), we'll now add it to the collection of children
            root_node.Children.Add(new Annotation_Rep_Tree_Data(new_element, desc));
            return root_node.Children[root_node.Children.Count - 1]; //and then return it
        } //close add_element_to_appropriate_child_node_which_is_returned()...



        public static string convert_tree_node_to_delimited_string(Annotation_Rep_Tree_Data_Model individual_node)
        {
            if (!individual_node.Name.Equals(ROOT_NAME))
            {
                string final_output = individual_node.Name;

                Annotation_Rep_Tree_Data_Model parent = individual_node.Parent;

                if (parent != null)
                {
                    if (!parent.Name.Equals(ROOT_NAME))
                        final_output = convert_tree_node_to_delimited_string(parent) + ";" + final_output;
                }

                return final_output;
            }
            else return "";
        } //close method convert_tree_node_to_delimited_string()...

    } //close class Annotation_Rep_Tree_Data...













    public class AnnotationTreeViewModel
    {
        #region Data

        readonly ReadOnlyCollection<Annotation_Rep_Tree_Data_Model> _firstGeneration;
        readonly Annotation_Rep_Tree_Data_Model _rootAnnotation;
        readonly ICommand _searchCommand;

        IEnumerator<Annotation_Rep_Tree_Data_Model> _matchingAnnotationEnumerator;
        string _searchText = String.Empty;

        #endregion // Data

        #region Constructor

        public AnnotationTreeViewModel(Annotation_Rep_Tree_Data rootAnnotation)
        {
            _rootAnnotation = new Annotation_Rep_Tree_Data_Model(rootAnnotation);

            _firstGeneration = new ReadOnlyCollection<Annotation_Rep_Tree_Data_Model>(
                new Annotation_Rep_Tree_Data_Model[] 
                { 
                    _rootAnnotation 
                });

            _searchCommand = new SearchAnnotationTreeCommand(this);
        }

        #endregion // Constructor

        #region Properties

        #region FirstGeneration

        /// <summary>
        /// Returns a read-only collection containing the first person 
        /// in the family tree, to which the TreeView can bind.
        /// </summary>
        public ReadOnlyCollection<Annotation_Rep_Tree_Data_Model> FirstGeneration
        {
            get { return _firstGeneration; }
        }

        #endregion // FirstGeneration

        #region SearchCommand

        /// <summary>
        /// Returns the command used to execute a search in the family tree.
        /// </summary>
        public ICommand SearchCommand
        {
            get { return _searchCommand; }
        }

        private class SearchAnnotationTreeCommand : ICommand
        {
            readonly AnnotationTreeViewModel _familyTree;

            public SearchAnnotationTreeCommand(AnnotationTreeViewModel familyTree)
            {
                _familyTree = familyTree;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            event EventHandler ICommand.CanExecuteChanged
            {
                // I intentionally left these empty because
                // this command never raises the event, and
                // not using the WeakEvent pattern here can
                // cause memory leaks.  WeakEvent pattern is
                // not simple to implement, so why bother.
                add { }
                remove { }
            }

            public void Execute(object parameter)
            {
                _familyTree.PerformSearch();
            }
        }

        #endregion // SearchCommand

        #region SearchText

        /// <summary>
        /// Gets/sets a fragment of the name to search for.
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (value == _searchText)
                    return;

                _searchText = value;

                _matchingAnnotationEnumerator = null;
            }
        }

        #endregion // SearchText

        #endregion // Properties

        #region Search Logic

        void PerformSearch()
        {
            if (_matchingAnnotationEnumerator == null || !_matchingAnnotationEnumerator.MoveNext())
                this.VerifyMatchingPeopleEnumerator();

            var annotation = _matchingAnnotationEnumerator.Current;

            if (annotation == null)
                return;

            // Ensure that this person is in view.
            if (annotation.Parent != null)
                annotation.Parent.IsExpanded = true;

            annotation.IsSelected = true;
        }

        void VerifyMatchingPeopleEnumerator()
        {
            var matches = this.FindMatches(_searchText, _rootAnnotation);
            _matchingAnnotationEnumerator = matches.GetEnumerator();

            if (!_matchingAnnotationEnumerator.MoveNext())
            {
                MessageBox.Show(
                    "No matching names were found.",
                    "Try Again",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );
            }
        }

        IEnumerable<Annotation_Rep_Tree_Data_Model> FindMatches(string searchText, Annotation_Rep_Tree_Data_Model annotation)
        {
            if (annotation.NameContainsText(searchText))
                yield return annotation;

            foreach (Annotation_Rep_Tree_Data_Model child in annotation.Children)
                foreach (Annotation_Rep_Tree_Data_Model match in this.FindMatches(searchText, child))
                    yield return match;
        }

        #endregion // Search Logic
    }



    //http://www.codeproject.com/Articles/26288/Simplifying-the-WPF-TreeView-by-Using-the-ViewMode
    public class Annotation_Rep_Tree_Data_Model:INotifyPropertyChanged
    {
        readonly ReadOnlyCollection<Annotation_Rep_Tree_Data_Model> _children;
        readonly Annotation_Rep_Tree_Data_Model _parent;
        readonly Annotation_Rep_Tree_Data _annotation;

        bool _isExpanded;
        bool _isSelected;

        public Annotation_Rep_Tree_Data_Model(Annotation_Rep_Tree_Data rootAnnotation) : this (rootAnnotation, null)
        {

        } //close constructor method Annotation_Rep_Tree_Data_Model...

        private Annotation_Rep_Tree_Data_Model(Annotation_Rep_Tree_Data annotation, Annotation_Rep_Tree_Data_Model parent)
        {
            _annotation = annotation;
            _parent = parent;

            _children = new ReadOnlyCollection<Annotation_Rep_Tree_Data_Model>(
                    (from child in _annotation.Children
                     select new Annotation_Rep_Tree_Data_Model(child, this))
                     .ToList<Annotation_Rep_Tree_Data_Model>());
        }


        #region Annotation Properties

        public ReadOnlyCollection<Annotation_Rep_Tree_Data_Model> Children
        {
            get { return _children; }
        }

        public string Name
        {
            get { return _annotation.Name; }
        }

        public string Desc
        {
            get { return _annotation.Desc; }
        }
                

        #endregion // Annotation Properties

        #region Presentation Members

        #region IsExpanded

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        #endregion // IsExpanded

        #region IsSelected

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion // IsSelected

        #region NameContainsText

        public bool NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(this.Name))
                return false;

            return this.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        #endregion // NameContainsText

        #region Parent

        public Annotation_Rep_Tree_Data_Model Parent
        {
            get { return _parent; }
        }

        #endregion // Parent

        #endregion // Presentation Members

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members


    }
      

} //close namespace...

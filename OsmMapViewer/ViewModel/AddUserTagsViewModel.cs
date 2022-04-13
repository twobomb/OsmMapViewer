using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.HandleDecorator.Helpers;
using OsmMapViewer.Misc;
using OsmMapViewer.Models;

namespace OsmMapViewer.ViewModel
{
    public class AddUserTagsViewModel: ViewModelBase
    {

        public Dictionary<string, List<string>> autocomplete { get; } = Utils.GetTagsDictionary();

        public List<string> TagsAutocomplete
        {
            get
            {
                return autocomplete.Keys.ToList();
            }
        }

        public Dictionary<string, List<string>> ItemsStringArray{
            get{
                Dictionary<string, List<string>> list = new Dictionary<string, List<string>>();
                foreach (var v in Items)
                {
                    if (!list.ContainsKey(v.Tag))
                        list.Add(v.Tag, new List<string>());
                    list[v.Tag].Add(v.Key);
                }

                return list;
            }
        }

        public ObservableCollection<TagValue> Items { get; set; } = new ObservableCollection<TagValue>();

        public bool CheckItems() {
            foreach (var tagValue in Items)
            {
                if (string.IsNullOrWhiteSpace(tagValue.Tag) ||
                    string.IsNullOrWhiteSpace(tagValue.Key))
                {
                    DXMessageBox.Show("Заполните все теги или удалите пустые!", "Предупреждение", MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return false;
                }
                if (tagValue.Tag != tagValue.Tag.Trim())
                    tagValue.Tag = tagValue.Tag.Trim();
                if (tagValue.Key!= tagValue.Key.Trim())
                    tagValue.Key = tagValue.Key.Trim();
            }

            return true;
        }

        private RelayCommand addTag;
        public RelayCommand AddTag
        {
            get
            {
                return addTag ??
                       (addTag = new RelayCommand(obj =>
                       {
                           Items.Add(new TagValue());
                       }));
            }
        }
        private RelayCommand removeAllTags;
        public RelayCommand RemoveAllTags
        {
            get
            {
                    return removeAllTags ??
                           (removeAllTags = new RelayCommand(obj =>
                           {

                               if (Items.Count > 0 && DXMessageBox.Show("Вы уверены что хотите удалить все теги?", "Удалить теги?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                    Items.Clear();
                           }));
            }
        }
        private RelayCommand removeTag;
        public RelayCommand RemoveTag
        {
            get
            {
                return removeTag ??
                       (removeTag = new RelayCommand(obj =>
                       {
                           if(obj is TagValue value)
                            Items.Remove(value);
                       }));
            }
        }

    }
}

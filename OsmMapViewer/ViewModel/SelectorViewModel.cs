using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmMapViewer.Misc;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using OsmMapViewer.Models;

namespace OsmMapViewer.ViewModel
{
    public class SelectorViewModel:ViewModelBase   {

        public TreeViewTagValue tagSelected = null;
        public TreeViewTagValue TagSelected { get {
                return tagSelected;
            }
            set
            {
                SetProperty(ref tagSelected, value);
            }
        }

        public Dictionary<string, List<string>> CheckedItems {
            get {
                Dictionary<string, List<string>> list = new Dictionary<string, List<string>>();
                foreach (var tv1 in tagList) {
                    foreach (var tv2 in tv1.ChildrenItems) {
                        if(tv2.IsCheckedMe && !tv2.Variable && !string.IsNullOrWhiteSpace(tv2.Key) && !string.IsNullOrWhiteSpace(tv2.Tag)) {
                            if (!list.ContainsKey(tv2.Tag))
                                list.Add(tv2.Tag, new List<string>());
                            list[tv2.Tag].Add(tv2.Key);
                        }
                        foreach (var tv3 in tv2.ChildrenItems) {
                            if (tv3.IsCheckedMe && !tv3.Variable && !string.IsNullOrWhiteSpace(tv3.Key) && !string.IsNullOrWhiteSpace(tv3.Tag)) {
                                if (!list.ContainsKey(tv3.Tag))
                                    list.Add(tv3.Tag, new List<string>());
                                list[tv3.Tag].Add(tv3.Key);
                            }
                        }
                    }
                }
                //usertags
                foreach(var v in userTags){
                    if (!list.ContainsKey(v.Tag))
                        list.Add(v.Tag, new List<string>());
                    list[v.Tag].Add(v.Key);
                }

                return list;
            }
        }

        public ObservableCollection<TreeViewTagValue> tagList { get; } = Utils.GetTagsTree();

        public string CountTagsAdded { get
            {
                return "Добавлено тегов: "+userTags.Count;
            } 
        }
        public List<TagValue> userTags { get; set; } = new List<TagValue>();
        public SelectorViewModel(){
        }


        private RelayCommand addUserTags;
        public RelayCommand AddUserTags
        {
            get
            {
                return addUserTags ??
                       (addUserTags = new RelayCommand(obj =>
                       {
                           AddUserTags aut = new AddUserTags();
                           aut.WND_ViewModel.Items.Clear();
                           foreach (var userTag in userTags)
                               aut.WND_ViewModel.Items.Add(userTag);
                           if (aut.ShowDialog().GetValueOrDefault(false))
                               userTags = aut.WND_ViewModel.Items.ToList();
                           else
                               userTags.Clear();
                           OnPropertyChanged("CountTagsAdded");
                       }));
            }
        }
        private RelayCommand openWiki;
        public RelayCommand OpenWiki
        {
            get
            {
                return openWiki??
                  (openWiki= new RelayCommand(obj =>
                  {
                      Utils.OpenUrl("https://wiki.openstreetmap.org/wiki/RU:%D0%9E%D0%B1%D1%8A%D0%B5%D0%BA%D1%82%D1%8B_%D0%BA%D0%B0%D1%80%D1%82%D1%8B");
                  }));
            }
        }
    }
}

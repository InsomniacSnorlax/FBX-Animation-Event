using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Snorlax.Animation.Events
{
    public class SearchTreeEventMethods : ScriptableObject, ISearchWindowProvider
    {
        private Action<string> target;
        private string[] listItems;
        private List<string> lastItem = new List<string>();

        public SearchTreeEventMethods(string[] items, Action<string> anim)
        {
            listItems = items;
            target = anim;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchList = new List<SearchTreeEntry>();
            searchList.Add(new SearchTreeGroupEntry(new GUIContent("List"), 0));
            List<string> sortListItems = listItems.ToList();
            List<string> groups = new List<string>();

            for (int index = 0; index < sortListItems.Count; index++)
            {
                string[] entryTitle = sortListItems[index].Split(new[] { '.' }, 2);
                string groupName = "";

                for (int i = 0; i < entryTitle.Length - 1; i++)
                {
                    groupName += entryTitle[i];
                    if (!groups.Contains(groupName))
                    {
                        searchList.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
                        groups.Add(groupName);
                    }

                    groupName += "/";
                }
                SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(entryTitle.Last()));
                entry.level = entryTitle.Length;
                entry.userData = listItems[index];
                lastItem.Add(entryTitle.Last());
                searchList.Add(entry);
            }
            return searchList;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            string index = (string)SearchTreeEntry.userData.ToString().Split("(")[0].Split(".").Last();


            target.Invoke(index);
            return true;
        }
    }
}
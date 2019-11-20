using InfoLocker.Parts;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfoLocker
{
    public class Synchronizer
    {
        public static Cabinet Synchronize(Cabinet localCopy, Cabinet onlineCopy, DateTime lastSyncTime)
        {
            // check if the roots are the same, if not then we will just pick and save one of the cabinets as is
            if (localCopy.ID != onlineCopy.ID)
            {
                // take whichever cabinet was created last
                if (localCopy.CreationTime > onlineCopy.CreationTime)
                {
                    return localCopy;
                }
                else
                {
                    return onlineCopy;
                }
            }

            Merge(localCopy, onlineCopy, lastSyncTime);
            return localCopy;
        }

        private static void Merge(Node localCopy, Node onlineCopy, DateTime lastSyncTime)
        {
            if (localCopy.GetType() != onlineCopy.GetType())
            {
                throw new Exception(string.Format("Unexpected types. Local={0}, Online={1}", localCopy.GetType(), onlineCopy.GetType()));
            }

            // first check the properties based on type of the node
            MergeProperties(localCopy, onlineCopy, lastSyncTime);

            // then compare the children
            MergeChildren(localCopy, onlineCopy, lastSyncTime);
        }

        private static void MergeProperties(Node localCopy, Node onlineCopy, DateTime lastSyncTime)
        {
            if (localCopy is Cabinet)
            {
                Cabinet localCabinet = localCopy as Cabinet;
                Cabinet onlineCabinet = onlineCopy as Cabinet;

                // compare the label and pick the latest
                if (localCabinet.Label != onlineCabinet.Label)
                {
                    if (localCabinet.LastModifiedTime < onlineCabinet.LastModifiedTime)
                    {
                        localCabinet.Label = onlineCabinet.Label;
                    }
                }

                // compare the password and pick the latest
                if (localCabinet.Password != onlineCabinet.Password)
                {
                    if (localCabinet.LastModifiedTime < onlineCabinet.LastModifiedTime)
                    {
                        localCabinet.Password = onlineCabinet.Password;
                    }
                }
            }
            else if (localCopy is Folder)
            {
                Folder localFolder = localCopy as Folder;
                Folder onlineFolder = onlineCopy as Folder;

                // compare the label and pick the latest
                if (localFolder.Label != onlineFolder.Label)
                {
                    if (localFolder.LastModifiedTime < onlineFolder.LastModifiedTime)
                    {
                        localFolder.Label = onlineFolder.Label;
                    }
                }
            }
            else if (localCopy is Card)
            {
                Card localCard = localCopy as Card;
                Card onlineCard = onlineCopy as Card;

                // compare the label and pick the latest
                if (localCard.Label != onlineCard.Label)
                {
                    if (localCard.LastModifiedTime < onlineCard.LastModifiedTime)
                    {
                        localCard.Label = onlineCard.Label;
                    }
                }
            }
            else if (localCopy is MultiLineEntry)
            {
                MultiLineEntry localEntry = localCopy as MultiLineEntry;
                MultiLineEntry onlineEntry = onlineCopy as MultiLineEntry;

                // compare the label and pick the latest
                if (localEntry.Label != onlineEntry.Label)
                {
                    if (localEntry.LastModifiedTime < onlineEntry.LastModifiedTime)
                    {
                        localEntry.Label = onlineEntry.Label;
                    }
                }

                // compare the content and pick the latest
                if (localEntry.Content != onlineEntry.Content)
                {
                    if (localEntry.LastModifiedTime < onlineEntry.LastModifiedTime)
                    {
                        localEntry.Content = onlineEntry.Content;
                    }
                }
            }
            else if (localCopy is SingleLineEntry)
            {
                SingleLineEntry localEntry = localCopy as SingleLineEntry;
                SingleLineEntry onlineEntry = onlineCopy as SingleLineEntry;

                // compare the label and pick the latest
                if (localEntry.Label != onlineEntry.Label)
                {
                    if (localEntry.LastModifiedTime < onlineEntry.LastModifiedTime)
                    {
                        localEntry.Label = onlineEntry.Label;
                    }
                }

                // compare the content and pick the latest
                if (localEntry.Content != onlineEntry.Content)
                {
                    if (localEntry.LastModifiedTime < onlineEntry.LastModifiedTime)
                    {
                        localEntry.Content = onlineEntry.Content;
                    }
                }
            }
        }

        private static void MergeChildren(Node localCopy, Node onlineCopy, DateTime lastSyncTime)
        {
            // collect a list of all nodes unique to online copy and also build a temporary list
            List<Node> onlineList = new List<Node>();
            List<Node> uniqueToOnline = new List<Node>();
            if (onlineCopy.HasChildren)
            {
                foreach (Node onlineChild in onlineCopy.Children)
                {
                    bool found = false;
                    if (localCopy.HasChildren)
                    {
                        foreach (Node localChild in localCopy.Children)
                        {
                            if (onlineChild.ID == localChild.ID)
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        uniqueToOnline.Add(onlineChild);
                    }

                    onlineList.Add(onlineChild);
                }
            }

            // collect a list of all nodes unique to local copy
            List<Node> uniqueToLocal = new List<Node>();
            if (localCopy.HasChildren)
            {
                foreach (Node localChild in localCopy.Children)
                {
                    bool found = false;
                    if (onlineCopy.HasChildren)
                    {
                        foreach (Node onlineChild in onlineCopy.Children)
                        {
                            if (localChild.ID == onlineChild.ID)
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        uniqueToLocal.Add(localChild);
                    }
                }
            }

            int index = 0;

            // we will first look and fix all nodes which are unique in either list of whose ordering don't match
            while (true)
            {
                Node localChild = (localCopy.HasChildren && (index < localCopy.Children.Count)) ? localCopy.Children[index] : null;
                Node onlineChild = (onlineCopy.HasChildren && (index < onlineCopy.Children.Count)) ? onlineCopy.Children[index] : null;

                if (localChild == null && onlineChild == null)
                {
                    // done with all the children
                    break;
                }
                else if (localChild != null && onlineChild == null)
                {
                    // local list is not done, but online list is done
                    // in this case check if remaining item should be in the 
                    // unique list, hence handle appropriately
                    if (!uniqueToLocal.Contains(localChild))
                    {
                        // this should never happen as all matching nodes should already be taken care of
                        throw new Exception("Unexpected local child node");
                    }

                    if (localChild.CreationTime < lastSyncTime)
                    {
                        // this node was deleted online, hence remove from local
                        localCopy.RemoveChild(localChild);
                        continue;
                    }
                    else
                    {
                        // this node was added locally, hence add to end of online
                        onlineCopy.AddChild(localChild.Clone(), false);
                        continue;
                    }
                }
                else if (localChild == null && onlineChild != null)
                {
                    // local list is not done, but online list is done
                    // in this case check if remaining item should be in the 
                    // unique list, hence handle appropriately
                    if (!uniqueToOnline.Contains(onlineChild))
                    {
                        // this should never happen as all matching nodes should already be taken care of
                        throw new Exception("Unexpected online child node");
                    }

                    if (onlineChild.CreationTime < lastSyncTime)
                    {
                        // node was deleted locally, hence remove from online
                        onlineCopy.RemoveChild(onlineChild, false);
                        continue;
                    }
                    else
                    {
                        // this node was added online, hence add to end of local
                        localCopy.AddChild(onlineChild.Clone());
                        continue;
                    }
                }
                else
                {
                    if (localChild.ID != onlineChild.ID)
                    {
                        // if ID doesn't match then either one of the node is missing
                        // in the other list, or the ordering has changed.
                        if (uniqueToLocal.Contains(localChild))
                        {
                            if (localChild.CreationTime < lastSyncTime)
                            {
                                // this node was deleted online, hence remove from local
                                localCopy.RemoveChild(localChild);
                                continue;
                            }
                            else
                            {
                                // this node was added locally, hence add to online
                                onlineCopy.AddChild(localChild.Clone(), index, false);
                                continue;
                            }
                        }
                        else if (uniqueToOnline.Contains(onlineChild))
                        {
                            if (onlineChild.CreationTime < lastSyncTime)
                            {
                                // node was deleted locally, hence remove from online
                                onlineCopy.RemoveChild(onlineChild, false);
                                continue;
                            }
                            else
                            {
                                // this node was added online, hence add to local
                                localCopy.AddChild(onlineChild.Clone(), index);
                                continue;
                            }
                        }
                        else
                        {
                            // we need to fix the ordering
                            int index_of_local = index;
                            int index_of_online = 0;
                            Node matchingOnlineChild = null;
                            while (index_of_online < onlineCopy.Children.Count)
                            {
                                Node child = onlineCopy.Children[index_of_online];
                                if (localChild.ID == child.ID)
                                {
                                    matchingOnlineChild = child;
                                    break;
                                }

                                index_of_online++;
                            }

                            if (matchingOnlineChild == null)
                            {
                                // this should never happen as otherwise the local child would have been in the unique list
                                throw new Exception("Could not find matching child");
                            }

                            // now compare the update times of the matching nodes and choose the index according to the latest
                            if (localChild.LastModifiedTime > matchingOnlineChild.LastModifiedTime)
                            {
                                 // need to update the online list
                                onlineCopy.RemoveChild(matchingOnlineChild, false);
                                onlineCopy.AddChild(matchingOnlineChild, index_of_local, false);
                                continue;
                            }
                            else
                            {
                                // need to update the local list
                                localCopy.RemoveChild(localChild);
                                if (index_of_online < localCopy.Children.Count)
                                {
                                    localCopy.AddChild(localChild, index_of_online);
                                }
                                else
                                {
                                    localCopy.AddChild(localChild);
                                }

                                continue;
                            }
                        }
                    }
                    else
                    {
                        index++;
                        continue;
                    }
                }
            }

            // once the lists are matched, now we compare the children
            index = 0;
            while (true)
            {
                Node localChild = (localCopy.HasChildren && (index < localCopy.Children.Count)) ? localCopy.Children[index] : null;
                Node onlineChild = (onlineCopy.HasChildren && (index < onlineCopy.Children.Count)) ? onlineCopy.Children[index] : null;

                if (localChild == null || onlineChild == null)
                {
                    // done with all the children
                    break;
                }
                else
                {
                    Merge(localChild, onlineChild, lastSyncTime);
                    index++;
                    continue;
                }
            }
        }
    }
}

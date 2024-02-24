using SDV_Realty_Core.Framework.AssetUtils;
using StardewModdingAPI.Events;
using System.Collections.Generic;


namespace SDV_Realty_Core.Framework.DataProviders
{
    internal class NPCGiftTastesDataProvider : IGameDataProvider
    {
        /// <summary>
        /// Dictionary: key=objectid
        /// Value: NPC, taste
        /// </summary>
        private Dictionary<string, Dictionary<string, List<string>>> NPCTastes;
        public override string Name => "Data/NPCGiftTastes";
        public NPCGiftTastesDataProvider(SDRContentManager conManager)
        {
            NPCTastes = conManager.NPCTastes;
        }
        public override void CheckForActivations()
        {

        }

        public override void HandleEdit(AssetRequestedEventArgs e)
        {
            e.Edit(asset =>
            {
                foreach (string objectId in NPCTastes.Keys)
                {
                    foreach (string taste in NPCTastes[objectId].Keys)
                    {
                        switch (taste)
                        {
                            case "Universal_Love":
                            case "Universal_Like":
                            case "Universal_Neutral":
                            case "Universal_Dislike":
                            case "Universal_Hate":
                                asset.AsDictionary<string, string>().Data[taste] += $" {objectId}";
                                break;
                            case "Love":
                                foreach (string npc in NPCTastes[objectId][taste])
                                {
                                    if (asset.AsDictionary<string, string>().Data.ContainsKey(npc))
                                    {
                                        string[] arParts = asset.AsDictionary<string, string>().Data[npc].Split('/');
                                        arParts[1] += $" {objectId}";
                                        asset.AsDictionary<string, string>().Data[npc]= string.Join("/", arParts);
                                    }
                                }
                                break;
                            case "Like":
                                foreach (string npc in NPCTastes[objectId][taste])
                                {
                                    if (asset.AsDictionary<string, string>().Data.ContainsKey(npc))
                                    {
                                        string[] arParts = asset.AsDictionary<string, string>().Data[npc].Split('/');
                                        arParts[3] += $" {objectId}";
                                        asset.AsDictionary<string, string>().Data[npc] = string.Join("/", arParts);
                                    }
                                }
                                break;
                            case "Neutral":
                                foreach (string npc in NPCTastes[objectId][taste])
                                {
                                    if (asset.AsDictionary<string, string>().Data.ContainsKey(npc))
                                    {
                                        string[] arParts = asset.AsDictionary<string, string>().Data[npc].Split('/');
                                        arParts[5] += $" {objectId}";
                                        asset.AsDictionary<string, string>().Data[npc] = string.Join("/", arParts);
                                    }
                                }
                                break;
                            case "Dislike":
                                foreach (string npc in NPCTastes[objectId][taste])
                                {
                                    if (asset.AsDictionary<string, string>().Data.ContainsKey(npc))
                                    {
                                        string[] arParts = asset.AsDictionary<string, string>().Data[npc].Split('/');
                                        arParts[7] += $" {objectId}";
                                        asset.AsDictionary<string, string>().Data[npc] = string.Join("/", arParts);
                                    }
                                }
                                break;
                            case "Hate":
                                foreach (string npc in NPCTastes[objectId][taste])
                                {
                                    if (asset.AsDictionary<string, string>().Data.ContainsKey(npc))
                                    {
                                        string[] arParts = asset.AsDictionary<string, string>().Data[npc].Split('/');
                                        arParts[9] += $" {objectId}";
                                        asset.AsDictionary<string, string>().Data[npc] = string.Join("/", arParts);
                                    }
                                }
                                break;
                            //default:
                            //    if (asset.AsDictionary<string, string>().Data.ContainsKey(npc))
                            //    {
                            //        string[] arParts = asset.AsDictionary<string, string>().Data[npc].Split('/');
                            //        switch (NPCTastes[objectId][npc].ToLower())
                            //        {
                            //            case "love":
                            //                arParts[1] += $" {objectId}";
                            //                break;
                            //            case "like":
                            //                arParts[3] += $" {objectId}";
                            //                break;
                            //            case "neutral":
                            //                arParts[5] += $" {objectId}";
                            //                break;
                            //            case "dislike":
                            //                arParts[7] += $" {objectId}";
                            //                break;
                            //            case "hate":
                            //                arParts[9] += $" {objectId}";
                            //                break;
                            //        }
                            //        asset.AsDictionary<string, string>().Data[npc] = string.Join("/", arParts);
                            //    }
                            //    break;
                        }
                    }
                }
            });
        }
        public void AddTastes(string objectId, Dictionary<string, List<string>> tastes)
        {
            if (!NPCTastes.ContainsKey(objectId))
            {
                NPCTastes.Add(objectId, tastes);
            }
            else
            {
                foreach (string npc in tastes.Keys)
                {
                    NPCTastes[objectId].Add(npc, tastes[npc]);
                }
            }

        }

        public override void OnGameLaunched()
        {
        }
    }
}

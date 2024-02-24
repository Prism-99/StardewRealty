using System.Collections.Generic;


namespace SDV_Realty_Core.ContentPackFramework.Objects
{
    internal class Requirements
    {
        public List<string> ParametersUsed { get; set; }
        //
        //  context parameters
        //
        //public List<string> A { get; set; }// special dialogue event
        //public bool F { get; set; }//today is not a festival day
        //public int U { get; set; }// no festivals within U days
        //public List<string> d { get; set; }// today is not in d
        //public double r { get; set; }// random chance percent
        //public string v { get; set; }//NPC not visible
        //public string w { get; set; }// current weather matches w
        public int y { get; set; }// must be at least year y
        //public List<string> z { get; set; }// current season is not z
        //
        //  player paramters
        //
        public string D { get; set; }// player is dating D
        public bool J { get; set; }// player finished joja warehouse
        //public bool L { get; set; }// farmhouse event
        public int M { get; set; }// player has at least M money
        public string O { get; set; }// player is married to O
        public int S { get; set; }//player has seen secret note S
        //public Point a { get; set; }// player at tile x,y
        public int b { get; set; }// reached mine level b
        //public int c { get; set; }//player has c inventory slots free
        public List<int> e { get; set; }// player has seen events e
        public Dictionary<string, int> f { get; set; }// have at least int friendship points with string
        //public string g { get; set; }//player has gender g
        //public string h { get; set; }// have no pet, like pet h
        //public int i { get; set; }// player has i in inventory
        public int j { get; set; }// player has played more than j days
        public List<int> k { get; set; }// player has not seen events k
        public List<string> l { get; set; }// player has not received mail id l
        public int m { get; set; }// player has earned at least m gold
        public List<string> n { get; set; }// player has received mail id n
        public List<string> o { get; set; }//player is not marreid to o
                                           //public List<string> p { get; set; }// NPC is in players location
#if v16
  public List<string> q { get; set; }//dialog answers
#else
        public List<int> q { get; set; }//dialog answers
#endif
        //public Dictionary<string, int> s { get; set; }// must have shipped int of string
        //public int t_min { get; set; }// time must be at least t_min
        //public int t_max { get; set; }// time must be before t_max
        //public List<int> u { get; set; }// must be u day of the month
        //public List<string> x { get; set; }//mark event seen, send email x
        public int lv { get; set; }// farmer skill level
        //
        //  host player
        //
        public bool C { get; set; }// host has not finished CC
        //public bool H { get; set; }// player is host
        public List<string> Hl { get; set; }//host has not received mail Hl
        public List<string> Hn { get; set; }// host has not received mail Hn
        public List<string> _l { get; set; }// host and player has not received mail _l
        public List<string> _n { get; set; }// host and player has received mail _n
        //public string Location { get; set; }

        public Requirements()
        {
            ParametersUsed = new List<string> { };
            //r = -1;
            M = -1;
            y = -1;
            b = -1;
            //c = -1;
            j = -1;
            m = -1;
            //i = -1;
            lv = -1;
            //t_max = -1;
            //t_min = -1;
#if v16
            q = new List<string> { };
#else
            q = new List<int> { };
#endif
            //x = new List<string> { };
            //a = new Point(-1, -1);
            //d = new List<string> { };
            S = -1;
            e = new List<int> { };
            f = new Dictionary<string, int> { };
            //s = new Dictionary<string, int> { };
            //z = new List<string> { };
            l = new List<string> { };
            k = new List<int> { };
            Hn = new List<string> { };
            Hl = new List<string> { };
            o = new List<string> { };
            //p = new List<string> { };
            //u = new List<int> { };
            _l = new List<string> { };
            _n = new List<string> { };
            //A = new List<string> { };
        }

        public bool AreRequirementsMet()
        {
            foreach (string sParameter in ParametersUsed)
            {
                switch (sParameter)
                {
                    case "y": //  check year
                        if (Game1.year < y)
                            return false;
                        break;
                    case "j":   //  check game days played
                        if (Game1.stats.DaysPlayed < j)
                            return false;
                        break;
                    case "D":   // player is dating
                        if (Game1.player.friendshipData != null && Game1.player.friendshipData.ContainsKey(D))
                        {
                            Friendship fsTmp = Game1.player.friendshipData[D];
                            if (fsTmp.Status != FriendshipStatus.Dating) return false;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case "J":   // completed JoJa Warehouse
                        if (!PlayerHasLetter("jojaBoilerRoom") || !PlayerHasLetter("jojaCraftsRoom") || !PlayerHasLetter("jojaFishTank") || !PlayerHasLetter("jojaPantry") || !PlayerHasLetter("jojaVault"))
                            return false;
                        break;
                    case "M":   // player has at least M money
                        if (Game1.player.Money < M)
                            return false;
                        break;
                    case "O":   // player is married to O
                        if (Game1.player.friendshipData != null && Game1.player.friendshipData.ContainsKey(O))
                        {
                            Friendship fsTmp = Game1.player.friendshipData[O];
                            if (fsTmp.Status != FriendshipStatus.Married) return false;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case "S":  // has seen secret note
                        if (Game1.player.secretNotesSeen == null) return false;
                        if (!Game1.player.secretNotesSeen.Contains(S))
                            return false;
                        break;
                    case "b":   // reached mine level
                        if (Game1.player.deepestMineLevel < b)
                            return false;
                        break;
                    case "e":  // player has seen event
                        if (Game1.player.eventsSeen == null) return false;
#if v16
                        foreach (int iEvent in e)
                        {
                            if (!Game1.player.eventsSeen.Contains(iEvent.ToString()))
                                return false;
                        }
#else
                        foreach (int iEvent in e)
                        {
                            if (!Game1.player.eventsSeen.Contains(iEvent))
                                return false;
                        }
#endif
                        break;
                    case "f":   // friendship levels
                        if (Game1.player.friendshipData == null) return false;
                        foreach (string sNPC in f.Keys)
                        {
                            if (Game1.player.friendshipData.ContainsKey(sNPC))
                            {
                                if (Game1.player.friendshipData[sNPC].Points < f[sNPC])
                                    return false;
                            }
                            else
                            {
                                //  NPC not list, so cannot have required
                                //  friendship level
                                return false;
                            }
                        }
                        break;
                    case "k":   //not seen events
                        if (Game1.player.eventsSeen != null)
                        {
#if v16
                            foreach (int iEvent in k)
                            {
                                if (Game1.player.eventsSeen.Contains(iEvent.ToString()))
                                    return false;
                            }
#else
                            foreach (int iEvent in k)
                            {
                                if (Game1.player.eventsSeen.Contains(iEvent))
                                    return false;
                            }
#endif
                        }
                        break;
                    case "l":   // player has not recieve mail
                        foreach(string sMail in l){
                            if (Game1.player.mailReceived.Contains(sMail))
                                return false;
                        }
                        break;
                    case "m":   // player has earned m gold
                        if (Game1.player.totalMoneyEarned < m)
                            return false;
                        break;
                    case "n":   // player has received letter id
                        foreach (string sMail in n)
                        {
                            if (!Game1.player.mailReceived.Contains(sMail))
                                return false;
                        }
                        break;
                    case "o": // player is not marreid to o
                        if (Game1.player.friendshipData != null && Game1.player.friendshipData.ContainsKey(O))
                        {
                            Friendship fsTmp = Game1.player.friendshipData[O];
                            if (fsTmp.Status == FriendshipStatus.Married) return false;
                        }
                        break;
                    case "q":   // dialog answer
                        if (Game1.player.DialogueQuestionsAnswered == null) return false;

                        foreach (var iAnswer in q)
                        {
                            if (!Game1.player.dialogueQuestionsAnswered.Contains(iAnswer))
                                return false;
                        }
                        break;
                }
            }

            return true;
        }

        private bool PlayerHasLetter(string sLetterId)
        {
            return Game1.player.mailReceived.Contains(sLetterId);
        }
    }
}

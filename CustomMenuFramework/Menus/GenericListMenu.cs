using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomMenuFramework.Menus
{
    internal class GenericListMenu:DialogueBox
    {
        protected List<KeyValuePair<string, string>> _PagedResponses = new List<KeyValuePair<string, string>>();
        public string lastQuestionKey;
        protected int _PagedResponsePage = 0;
        protected int _PagedResponseItemsPerPage;
        public bool _PagedResponseAddCancel;
        protected string _PagedResponsePrompt;
        private static int instanceCount = 0;
        protected Action<string> _OnPagedResponse;

        public GenericListMenu(string prompt, List<KeyValuePair<string, string>> responses, Action<string> on_response, bool auto_select_single_choice = true, bool addCancel = true, int itemsPerPage = 5, int width = 1200) : base(prompt, responses.Select(p => new Response(p.Key, p.Value)).ToArray(), width)
        {
            _PagedResponses.Clear();
            _PagedResponses.AddRange(responses);
            _PagedResponsePage = 0;
            _PagedResponseAddCancel = addCancel;
            _PagedResponseItemsPerPage = itemsPerPage;
            _PagedResponsePrompt = prompt;
            _OnPagedResponse = on_response;
            if (_PagedResponses.Count == 1 && auto_select_single_choice)
            {
                on_response(_PagedResponses[0].Key);
            }
            else if (_PagedResponses.Count > 0)
            {
                _ShowPagedResponses(_PagedResponsePage);
            }
        }
        public bool answerDialogue(Response answer)
        {
            switch (answer.responseKey)
            {
                case "pagedResponse_cancel":
                    _CleanupPagedResponses();
                    break;
                case "pagedResponse_nextPage":
                    _ShowPagedResponses(_PagedResponsePage + 1);
                    break;
                case "pagedResponse_previousPage":
                    _ShowPagedResponses(_PagedResponsePage - 1);
                    break;
                default:
                    instanceCount--;
                    if (instanceCount == 0)
                    {
                        Game1.activeClickableMenu.exitThisMenu();
                        Game1.dialogueUp = false;
                        Game1.player.CanMove = true;
                    }
                    if (_OnPagedResponse != null)
                    {
                        _OnPagedResponse(answer.responseKey);
                    }
                    break;
            }

            return true;
        }
        protected void _ShowPagedResponses(int page = -1)
        {
            _PagedResponsePage = page;
            int itemsPerPage = _PagedResponseItemsPerPage;
            int pages = (_PagedResponses.Count - 1) / itemsPerPage;
            int itemsOnCurPage = itemsPerPage;
            if (_PagedResponsePage == pages - 1 && _PagedResponses.Count % itemsPerPage == 1)
            {
                itemsOnCurPage++;
                pages--;
            }
            List<Response> locationResponses = new List<Response>();
            for (int i = 0; i < itemsOnCurPage; i++)
            {
                int index = i + _PagedResponsePage * itemsPerPage;
                if (index < _PagedResponses.Count)
                {
                    KeyValuePair<string, string> response = _PagedResponses[index];
                    locationResponses.Add(new Response(response.Key, response.Value));
                }
            }
            if (_PagedResponsePage < pages)
            {
                locationResponses.Add(new Response("pagedResponse_nextPage", Game1.content.LoadString("Strings\\UI:NextPage")));
            }
            if (_PagedResponsePage > 0)
            {
                locationResponses.Add(new Response("pagedResponse_previousPage", Game1.content.LoadString("Strings\\UI:PreviousPage")));
            }
            if (_PagedResponseAddCancel)
            {
                locationResponses.Add(new Response("pagedResponse_cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
            }
            createQuestionDialogue(_PagedResponsePrompt, locationResponses.ToArray(), "pagedResponse");
        }
        public void createQuestionDialogue(string question, Response[] answerChoices, string dialogKey)
        {
            instanceCount++;
            lastQuestionKey = dialogKey;
            //Game1.activeClickableMenu = this;
            //Game1.dialogueUp = true;
            Game1.player.CanMove = false;
        }

        protected virtual void _CleanupPagedResponses()
        {
            _PagedResponses.Clear();
            _OnPagedResponse = null;
            _PagedResponsePrompt = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Chadder.Droid.Adapters;
using ChadderLib.Util;
using System.Threading.Tasks;
using Chadder.Droid.Util;
using Chadder.Droid.Views.Media;
using Chadder.Droid.Views.Account;
using Android.Content.PM;
using Android.Util;
using Chadder.Data.Util;
using Chadder.Client.Data;
using Android.Support.V7.App;

namespace Chadder.Droid.Views.Contacts
{

    public class ChatFragment : BaseFragment
    {
        private static string EXTRA_CONVERSATION_ID = "EXTRA_CONVERSATION_ID";
        private static string EXTRA_CONTACT_ID = "EXTRA_CONTACT_ID";
        private EditText _editText;
        private Button _btnSend;
        private ImageButton _btnCapture;

        private ContextMenuManager<ChadderMessage> _menuManager;
        private ListView _listView;
        private ChatAdapter _adapter;
        private ChadderConversation _conversation;

        private View _view;

        private bool loaded = false;
        public override bool CustomActionBar { get { return false; } }
        public ChatFragment()
        {

        }

        static public ChatFragment OpenChat(ChadderConversation conversation)
        {
            var result = new ChatFragment();
            result.Arguments = new Bundle();
            result.Arguments.PutInt(EXTRA_CONVERSATION_ID, conversation.recordId);
            return result;
        }

        static public ChatFragment OpenChat(ChadderContact contact)
        {
            var result = new ChatFragment();
            result.Arguments = new Bundle();
            result.Arguments.PutString(EXTRA_CONTACT_ID, contact.UserId);
            return result;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            HasOptionsMenu = true; // It must be true in order to toggleAction call onItemsSelected()
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Arguments.GetString(EXTRA_CONTACT_ID, null) == null)
                _conversation = ChadderUI.Source.db.GetConversation(Arguments.GetInt(EXTRA_CONVERSATION_ID, 0));
            else
                _conversation = ChadderUI.Source.db.GetContactConversation(Arguments.GetString(EXTRA_CONTACT_ID));
            EventHandlers.Add(_conversation, (s, e) =>
            {
                InvalidateData();
            });
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            _view = inflater.Inflate(Resource.Layout.chat_fragment, container, false);
            SetupListView();
            SetupButtonClicks();
            SetupMenu();

            return _view;
        }


        private void SetupMenu()
        {
            _menuManager = new ContextMenuManager<ChadderMessage>(this, _listView);

            _menuManager.InsertItem("ConversationMessageDelete".t(),
                async (ChadderMessage msg) => await ChadderUI.DeleteMessage(msg, _conversation));

            _menuManager.InsertItem("ConversationMessageDeleteRemote".t(),
                async (ChadderMessage msg) => await ChadderUI.TakeBack(msg, _conversation),
                msg => msg.MyMessage == true && (msg.Status == ChadderMessage.MESSAGE_STATUS.SENT));

            _menuManager.InsertItem("ConversationMessageCopy".t(), (ChadderMessage msg) =>
            {
                ClipboardManager clipboard = (ClipboardManager)Activity.GetSystemService(Context.ClipboardService);
                ClipData clip = ClipData.NewPlainText("Chadder Message", msg.Body);
                clipboard.PrimaryClip = clip;
                DisplayToast("Copied");
            }, msg => msg.Type == ChadderMessage.MESSAGE_TYPE.TEXT);
        }
        private void SetupListView()
        {
            _listView = _view.FindViewById<ListView>(Resource.Id.chat_listview);
            _editText = _view.FindViewById<EditText>(Resource.Id.chat_edittext_msg);

            if (_conversation == null)
            {
                throw new Exception("Conversation not found!");
            }

            _adapter = new ChatAdapter(Activity, _conversation, _listView);

            _listView.Adapter = _adapter;

        }

        private void SetupButtonClicks()
        {
            _btnSend = _view.FindViewById<Button>(Resource.Id.chat_btn_send);
            _btnSend.Click += (object sender, EventArgs e) =>
            {
                if (ChadderUI.SendMessage(_editText.Text, _conversation))
                    _editText.Text = "";
            };

            var chadderRed = Resources.GetColor(Resource.Color.chadderred);


            _btnCapture = _view.FindViewById<ImageButton>(Resource.Id.chat_btn_capture);
            _btnCapture.SetColorFilter(chadderRed, Android.Graphics.PorterDuff.Mode.SrcIn);
            _btnCapture.Click += (object sender, EventArgs e) =>
            {
                ChadderImagePicker.MultiSourcePick(this);
            };


        }

        protected override void InvalidateData()
        {
            base.InvalidateData();
            Activity.InvalidateOptionsMenu();
            (Activity as AppCompatActivity).SupportActionBar.Title = _conversation.DisplayName;
        }
        public override void OnResume()
        {
            base.OnResume();
            if (loaded == false)
            {
                _editText.RequestFocus();
                Activity.ShowKeyboard();
                loaded = true;
            }
        }
        public override async void OnPause()
        {
            base.OnPause();
            //++await ChadderUI.Source.ResetNumberOfMessages(_conversation);
        }
        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            try
            {
                var image = ChadderImagePicker.OnPickImageResult(requestCode, resultCode, data, Activity);
                if (image != null)
                {
                    image.DefaultResize();
                    ChadderUI.SendPicture(image.CompressToJPEG(), _conversation);
                }

            }
            catch (Exception e)
            {
                Toast.MakeText(Activity, e.Message, ToastLength.Long).Show();
                Insight.Report(e);
                Console.WriteLine(e);
            }
        }
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.chat_menu, menu);
            var status = menu.FindItem(Resource.Id.chat_key_status);

            if (_conversation.KeyStatus == Data.PublicKeyStatus.CHANGED)
                status.SetIcon(Resource.Drawable.key_invalid);
            else
                status.SetVisible(false);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.chat_profile)
            {
                SupportFragmentManager.BeginTransaction()
                       .Replace(Resource.Id.content_frame, ViewProfileFragment.ViewProfile(_conversation.ContactUserId))
                       .AddToBackStack(null)
                       .Commit();

                return true;
            }
            else if (item.ItemId == Resource.Id.chat_key_status)
            {
                //++ChadderUI.KeyStatusWarning(_conversation.Contact);
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
            var msg = _adapter[(int)info.Id];
            _menuManager.CreateMenu(menu, msg);
        }
        public override bool OnContextItemSelected(IMenuItem item)
        {
            return _menuManager.Selected(item);

        }
    }
}
using System;
using System.Text;
using Android.App;
using Android.OS;
using Android.Widget;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MqttXamarinAndroid
{
    [Activity(Label = "MqttXamarinAndroid", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private EditText edtBroker, edtPort, edtUser, edtPass, edtTopic, edtMessage;
        private Button bttConnect, bttSubscribe, bttPublish;
        private CheckBox cbxUser;
        private Spinner spnQOS;
        private TextView txtResult;
        private MqttClient mqttClient;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            //
            InitWidget();
            InitControl();
        }

        private void InitWidget()
        {
            txtResult = FindViewById<TextView>(Resource.Id.result);
            edtBroker = FindViewById<EditText>(Resource.Id.edtBroker);
            edtPort = FindViewById<EditText>(Resource.Id.edtPort);
            edtUser = FindViewById<EditText>(Resource.Id.edtUser);
            edtPass = FindViewById<EditText>(Resource.Id.edtPassword);
            edtTopic = FindViewById<EditText>(Resource.Id.edtTopic);
            edtMessage = FindViewById<EditText>(Resource.Id.edtMessage);
            bttConnect = FindViewById<Button>(Resource.Id.bttConnect);
            bttSubscribe = FindViewById<Button>(Resource.Id.bttSubTopic);
            bttPublish = FindViewById<Button>(Resource.Id.bttPublishMes);
            cbxUser = FindViewById<CheckBox>(Resource.Id.cbxUser);
            spnQOS = FindViewById<Spinner>(Resource.Id.spnQOS);

            var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.qos, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spnQOS.Adapter = adapter;

            cbxUser.CheckedChange += (object sender, CompoundButton.CheckedChangeEventArgs e) =>
            {
                if (e.IsChecked)
                {
                    edtUser.Enabled = true;
                    edtPass.Enabled = true;
                }
                else
                {
                    edtUser.Enabled = false;
                    edtPass.Enabled = false;
                }
            };
        }

        private void InitControl()
        {
            bttConnect.Click += (object sender, EventArgs e) => ConnectServer(); 
            bttSubscribe.Click += (object sender, EventArgs e) => Subscribe();
            bttPublish.Click += (object sender, EventArgs e) => Publish();
        }

        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            RunOnUiThread(() =>
            {
                txtResult.Text = $"Receiver message: {Encoding.UTF8.GetString(e.Message)}";
            });
        }

        private void Client_ConnectionClosedEvent(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                txtResult.Text = "Connection lost";
            });
        }

        private void ConnectServer()
        {
            try
            {
                if (edtBroker.Text == null || edtBroker.Text.Length == 0 || edtPort.Text == null || edtPort.Text.Length == 0) Toast.MakeText(this, "Broken or port wrong", ToastLength.Short).Show();
                else
                {
                    mqttClient = new MqttClient(edtBroker.Text);
                    mqttClient.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                    mqttClient.ConnectionClosed += Client_ConnectionClosedEvent;
                    if (cbxUser.Checked) mqttClient.Connect("", edtUser.Text, edtPass.Text);
                    else mqttClient.Connect("");
                    //mqttClient.Connect (customerDB.customerId+"", Mqtt_Username, Mqtt_Password, false, KeepAlives);
                    if (mqttClient.IsConnected) txtResult.Text = "Connect OK -- let's sub topic";
                }
            }
            catch (Exception ex)
            {
                txtResult.Text = "Connect ERROR";
                Console.WriteLine($"ConnectServer err {ex.Message}");
            }
        }

        private void Subscribe()
        {
            try
            {
                if (edtTopic.Text != null || edtTopic.Text.Length > 0)
                {
                    if (mqttClient != null && mqttClient.IsConnected)
                    {
                        mqttClient.Subscribe(new string[] { edtTopic.Text }, new byte[] { (byte)spnQOS.SelectedItemPosition });
                        txtResult.Text = $"Subcribe topic {edtTopic.Text} ok";
                    }
                }
                else Toast.MakeText(this, "topic wrong", ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Subcribe topic err: {ex.Message}");
            }
        }

        private void Publish()
        {
            try
            {
                if (edtTopic.Text != null || edtTopic.Text.Length > 0 || edtMessage.Text != null || edtMessage.Text.Length > 0)
                {
                    if (mqttClient != null && mqttClient.IsConnected)
                    {
                        mqttClient.Publish(edtTopic.Text, Encoding.UTF8.GetBytes(edtMessage.Text));
                        txtResult.Text = $"publish message {edtTopic.Text} to topic {edtTopic.Text} ok";
                    }
                }
                else Toast.MakeText(this, "topic or message wrong", ToastLength.Short).Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Publish err: {ex.Message}");
            }
        }
    }
}

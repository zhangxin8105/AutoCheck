using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;

namespace iWay.AutoCheck
{
    public partial class Window : Form
    {
        public Window()
        {
            InitializeComponent();
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void Window_Load(object sender, EventArgs e)
        {
            this.Text += " For " + Settings.CHECK_NAME;
        }

        private Thread mCheckThread;

        private void WaitAndCheck()
        {
            bool notified = false;
            while (true)
            {
                DateTime currentTime = DateTime.Now;
                DateTime notifyTime = currentTime.AddHours(2);

                if (notified == false &&
                    notifyTime.Year == dateTimePicker.Value.Year &&
                    notifyTime.Month == dateTimePicker.Value.Month &&
                    notifyTime.Day == dateTimePicker.Value.Day &&
                    notifyTime.Hour == dateTimePicker.Value.Hour &&
                    notifyTime.Minute == dateTimePicker.Value.Minute &&
                    notifyTime.Second == dateTimePicker.Value.Second)
                {
                    Invoke(new Action(SendNotifyMail));
                    notified = true;
                }

                if (currentTime.Year == dateTimePicker.Value.Year &&
                    currentTime.Month == dateTimePicker.Value.Month &&
                    currentTime.Day == dateTimePicker.Value.Day &&
                    currentTime.Hour == dateTimePicker.Value.Hour &&
                    currentTime.Minute == dateTimePicker.Value.Minute &&
                    currentTime.Second == dateTimePicker.Value.Second)
                {
                    Invoke(new Action(LoadWebPage));
                    Thread.Sleep(5000);
                    Invoke(new Action(DoMyCheck));
                    Thread.Sleep(5000);
                    Invoke(new Action(SendCheckMail));
                    Thread.Sleep(5000);
                    Invoke(new EventHandler(SwitchState), null, null);
                    break;
                }

                Thread.Sleep(100);
            }
        }

        private MailSender GetMailSender()
        {
            MailSender mailSender = new MailSender();
            mailSender.SetServerInfo(Settings.MAIL_SERVER);
            mailSender.SetLoginInfo(Settings.MAIL_ACCOUNT, Settings.MAIL_PASSWORD, Settings.MAIL_SENDER);
            mailSender.SetReceivers(Settings.MAIL_RECEIVER);
            return mailSender;
        }

        private void SendNotifyMail()
        {
            MailSender mailSender = GetMailSender();
            string title = "Check Notify";
            string timeString = dateTimePicker.Value.ToString("yyyy-MM-dd HH:mm:ss");
            string body = "Will Start Check At " + timeString;
            mailSender.SetContent(title, body);
            mailSender.Send();
        }

        private void LoadWebPage()
        {
            webBrowser.Navigate("http://doc-server");
        }

        private string GetUserNameControlId(string html)
        {
            int index = html.IndexOf("userName");
            return html.Substring(index, 11);
        }

        private void DoMyCheck()
        {
            HtmlDocument document = webBrowser.Document;
            string userNameControlId = GetUserNameControlId(document.Forms[0].InnerHtml);
            document.GetElementById(userNameControlId).InnerText = Settings.CHECK_NAME;
            document.GetElementById("password").InnerText = Settings.CHECK_PASSWORD;
            document.GetElementById("Submit2").InvokeMember("click");
        }

        private void SendCheckMail()
        {
            MailSender mailSender = GetMailSender();
            if (webBrowser.Document.Title.Equals("Check"))
            {
                string htmlDocument = webBrowser.Document.Body.InnerHtml;
                string checkTimeTag = "check time: ";
                int checkTimeTagIndex = htmlDocument.IndexOf(checkTimeTag);
                int chectTimeStringLength = 31;
                string checkTimeString = htmlDocument.Substring(checkTimeTagIndex, chectTimeStringLength);
                string checkTime = checkTimeString.Substring(checkTimeTag.Length);
                mailSender.SetContent("Check Succeed", "Check Time : " + checkTime);
            }
            else
            {
                mailSender.SetContent("Check Failed", "Come quickly and see why.");
            }
            mailSender.Send();
        }

        private void SwitchState(object sender, EventArgs e)
        {
            if (dateTimePicker.Enabled)
            {
                if (dateTimePicker.Value < DateTime.Now)
                {
                    MessageBox.Show("时间不能早于当前时间。", "AutoCheck");
                }
                else
                {
                    btnStart.Text = "取消";
                    dateTimePicker.Enabled = false;
                    mCheckThread = new Thread(WaitAndCheck);
                    mCheckThread.IsBackground = true;
                    mCheckThread.Start();
                }
            }
            else
            {
                btnStart.Text = "开始";
                dateTimePicker.Enabled = true;
                if (sender != null)
                {
                    mCheckThread.Abort();
                }
            }
        }

        private void Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mCheckThread != null && mCheckThread.IsAlive)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void On_NotifyIcon_Click(object sender, EventArgs e)
        {
            Show();
        }
    }
}

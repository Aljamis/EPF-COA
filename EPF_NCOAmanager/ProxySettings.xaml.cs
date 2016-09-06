using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EPF_NCOAmanager
{
    /// <summary>
    /// Interaction logic for ProxySettings.xaml
    /// </summary>
    public partial class ProxySettings : Window
    {
        private EPFController myCNTLr;
        private ArrayList missingSettings = new ArrayList();



        /// <summary>
        /// Pass in the Controller
        /// Try making it a Singleton
        /// </summary>
        /// <param name="cntlr"></param>
        public ProxySettings(EPFController cntlr)
        {
            InitializeComponent();
            this.txtProxyHost.Text = "proxy.aetna.com";
            this.txtProxyPort.Text = "9119";
            this.myCNTLr = cntlr;
        }



        /// <summary>
        /// Validate all fields have been input before performing the HTTP request.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVersion_Click(object sender, RoutedEventArgs e)
        {
            this.missingSettings.Clear();       // Reset all the messages in the collection
            if (!proxySettingProvided())
            {
                myCNTLr.displayErrorMsg("Missing Proxy Settings" , missingSettings);
                return;
            }
            JSONversion jVer = myCNTLr.getVersion();
            if (jVer != null)
            {
                lblVersion.Content = "OK - Current Version : " + jVer.version;
                btnVersion.Visibility = Visibility.Hidden;
            }
        }



        /// <summary>
        /// Validation is ONLY for input text boxes and NOT verification
        /// </summary>
        /// <returns></returns>
        private bool proxySettingProvided()
        {
            if (this.txtProxyHost.Text == null || this.txtProxyHost.Text == "")
                missingSettings.Add("- Proxy Host");

            if (this.txtProxyPort.Text == null || this.txtProxyPort.Text == "")
                missingSettings.Add("- Proxy Port");

            if (this.txtProxyUser.Text == null || this.txtProxyUser.Text == "")
                missingSettings.Add("- Proxy User");

            if (this.pwdProxy.Password == null || this.pwdProxy.Password == "")
                missingSettings.Add("- Proxy Password");

            if (missingSettings.Count > 0)
                return false;

            myCNTLr.setupProxy(txtProxyHost.Text
                , Int32.Parse(txtProxyPort.Text)
                , txtProxyUser.Text
                , pwdProxy.Password);
            return true;
        }






        /// <summary>
        /// Make sure any input in Proxy Port is NUMERIC only
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtProxyPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = isNumeric(e.Text);
        }



        /// <summary>
        /// Validate input is ONLY numeric
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        private static bool isNumeric(string txt)
        {
            Regex r = new Regex("[^0-9.-]");
            return r.IsMatch(txt);
        }
    }
}

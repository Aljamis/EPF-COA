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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace EPF_NCOAmanager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EPFController myCNTLr = EPFController.getInstance();
        private ArrayList missingSettings = new ArrayList();

        //  Does this work for binding
        List<JSONfiles> myFileList;

        public MainWindow()
        {
            InitializeComponent();
            resetScreen();
            // populate ComboBox with Product Code/ID combos
            List<EPFProductCodeIDs> cmbxPairs = new List<EPFProductCodeIDs>();
            cmbxPairs.Add (new EPFProductCodeIDs("NCOA Weekly file", "NCAW", "NCL18H") );
            cmbxPairs.Add(new EPFProductCodeIDs("Daily Delete file", "NCAD", "TEXTFILE"));
            cmbxPairs.Add(new EPFProductCodeIDs("NCOA Weekly file 256", "NCAW1", "NCL18H"));
            cmbxPairs.Add(new EPFProductCodeIDs("Daily Delete file 256", "NCADX", "TEXTFILE"));
            cmbxEPFproduct.DisplayMemberPath = "Name";
            cmbxEPFproduct.ItemsSource = cmbxPairs;
        }




        /// <summary>
        /// Return screen to its intial settings - hiding most fields
        /// </summary>
        private void resetScreen()
        {
            btnLogin.Visibility = Visibility.Visible;
            btnLogout.Visibility = Visibility.Collapsed;
            btnSearchEPF.Visibility = Visibility.Collapsed;
            cmbxEPFproduct.Visibility = Visibility.Collapsed;
            cbxNew.Visibility = Visibility.Collapsed;
            cbxStarted.Visibility = Visibility.Collapsed;
            cbxComplete.Visibility = Visibility.Collapsed;
            dGrdFileList.Visibility = Visibility.Collapsed;
            txtEPFUser.Text = "";
            pwdEPF.Password = "";
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



        /// <summary>
        /// Display Proxy Settings window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProxySettingsSetup(object sender, RoutedEventArgs e)
        {
            ProxySettings s = new ProxySettings(myCNTLr);
            s.Show();
        }

        private void btnLogin_Click1(object sender, RoutedEventArgs e)
        {
            missingSettings.Clear();        //  Clear all the messages
            if (!loginProvided())
            {
                myCNTLr.displayErrorMsg("Missing Login :", missingSettings);
                return;
            }

            if (myCNTLr.epfLogin(txtEPFUser.Text, pwdEPF.Password))
            {
                btnLogin.Visibility = Visibility.Collapsed;         // Hide the login button
                btnLogout.Visibility = Visibility.Visible;          // Display the logout button
                cmbxEPFproduct.Visibility = Visibility.Visible;     // Display the Product combobox
                btnSearchEPF.Visibility = Visibility.Visible;       // Display the Search button
                cbxNew.Visibility = Visibility.Visible;
                cbxStarted.Visibility = Visibility.Visible;
                cbxComplete.Visibility = Visibility.Visible;
            }
        }


        /// <summary>
        /// both User and PWD have been entered
        /// </summary>
        /// <returns></returns>
        private bool loginProvided()
        {
            if (txtEPFUser.Text == null || txtEPFUser.Text == "")
                missingSettings.Add("- EPR User Id");

            if (missingSettings.Count > 0)
                return false;
            return true;
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (myCNTLr.epfLogout())
            {
                resetScreen();
            }
            else
            {
                MessageBox.Show("Umm.. I had trouble logging out.");
            }
        }

        private void btnSearchEPF_Click(object sender, RoutedEventArgs e)
        {
            EPFProductCodeIDs prod = cmbxEPFproduct.SelectedItem as EPFProductCodeIDs;
            if (prod == null)
            {
                MessageBox.Show("Please select a Product");
                return;
            }
            myFileList = myCNTLr.epfGetFiles(prod , fileStatus() );
            dGrdFileList.ItemsSource = myFileList;
            dGrdFileList.Visibility = Visibility.Visible;
        }




        /// <summary>
        /// From the ContextMenu user has selected to change the status of the file
        /// to NEW
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeStatusNew(object sender, RoutedEventArgs e)
        {
            JSONfiles obj = (JSONfiles)dGrdFileList.SelectedItem;
            myCNTLr.changeStatus(obj, "N");

            //  Refresh the table
            btnSearchEPF_Click(sender, e);
        }




        /// <summary>
        /// From the ContextMenu user has selected to change the status of the file
        /// to COMPLETED
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeStatusCompleted(object sender, RoutedEventArgs e)
        {
            JSONfiles obj = (JSONfiles)dGrdFileList.SelectedItem;
            myCNTLr.changeStatus(obj, "C");

            //  Refresh the table
            btnSearchEPF_Click(sender, e);
        }


        /// <summary>
        /// Based on the CheckBoxes selected return a String representation
        /// </summary>
        /// <returns></returns>
        private string fileStatus()
        {
            string retVal = "";
            if (cbxNew.IsChecked.Value)
                retVal += "N";
            if (cbxStarted.IsChecked.Value)
                retVal += "S";
            if (cbxComplete.IsChecked.Value)
                retVal += "C";
            return retVal;
        }

        #region The Help Button replacing Min/Max buttons 

        private const uint WS_EX_CONTEXTHELP = 0x00000400;
        private const uint WS_MINIMIZEBOX = 0x00020000;
        private const uint WS_MAXIMIZEBOX = 0x00010000;
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_FRAMECHANGED = 0x0020;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_CONTEXTHELP = 0xF180;


        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, uint newStyle);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            uint styles = GetWindowLong(hwnd, GWL_STYLE);
            styles &= 0xFFFFFFFF ^ (WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
            SetWindowLong(hwnd, GWL_STYLE, styles);
            styles = GetWindowLong(hwnd, GWL_EXSTYLE);
            styles |= WS_EX_CONTEXTHELP;
            SetWindowLong(hwnd, GWL_EXSTYLE, styles);
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HelpHook);
        }

        private IntPtr HelpHook(IntPtr hwnd,
                int msg,
                IntPtr wParam,
                IntPtr lParam,
                ref bool handled)
        {
            if (msg == WM_SYSCOMMAND &&
                    ((int)wParam & 0xFFF0) == SC_CONTEXTHELP)
            {
                MessageBox.Show("help");
                System.Diagnostics.Process.Start("EPFsupport.htm");
                handled = true;
            }
            return IntPtr.Zero;
        }

        #endregion The Help Button replacing Min/Max buttons
    }
}

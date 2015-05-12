using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Forms;
using MouseKeyboardActivityMonitor;
using System.Runtime.InteropServices;
using MouseKeyboardActivityMonitor.WinApi;
using System.Drawing;

namespace MouseTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private MouseHookListener m_mouseListener;
        private bool mouseDown = false;
        private bool recording = false;
        private long dragDistance = 0;
        private System.Drawing.Point startPoint;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Drawing.Point lastPoint;
        private int numberOfStrokes = 0;
        private double currentStrokeTravel = 0;
        private double totalStrokesTravel = 0;



        public MainWindow()
        {
            InitializeComponent();
            //this.MouseUp += new MouseButtonEventHandler(OnMouseup); 
            // Note: for an application hook, use the AppHooker class instead
            m_mouseListener = new MouseHookListener(new GlobalHooker());

            // The listener is not enabled by default
            m_mouseListener.Enabled = true;
           

            // Set the event handler
            // recommended to use the Extended handlers, which allow
            // input suppression among other additional information
            m_mouseListener.MouseDownExt += MyMouseDown;
            m_mouseListener.MouseUp += MyMouseUp;
            
            m_mouseListener.MouseMoveExt += MyMouseMoveExt;

            xTextBox.Text = "0";
            yTextBox.Text = "0";
            distanceTextBox.Text = "0";
            limitTextBox.Text = "100";
            strokeTravelTextBox.Text = "0";
            totalStrokeTravelTextBox.Text = "0";
            
            errorProvider = new System.Windows.Forms.ErrorProvider();

            errorProvider.BlinkRate = 1000;
            errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.AlwaysBlink;
        }

       
        private void MyMouseDown(object sender, MouseEventExtArgs e)
        {
            if (e.IsMouseKeyDown)
            {
                if (!this.mouseDown)
                {
                    this.startPoint = e.Location;
                    this.mouseDown = true;
                }
                distanceTextBox.Text = DistanceTo(this.startPoint, e.Location).ToString();
            }
            else
            {
                this.mouseDown = false;
            }
        }


        private void MyMouseUp(object sender, EventArgs e)
        {
            this.mouseDown = false;
            if (recording)
            {
                this.totalStrokesTravel += this.currentStrokeTravel;
                this.currentStrokeTravel = 0;
                totalStrokeTravelTextBox.Text = this.totalStrokesTravel.ToString();
                
            }
        }

        private void MyMouseMoveExt(object sender, MouseEventExtArgs e)
        {

            try
            {
                yTextBox.Text = e.X.ToString();
                xTextBox.Text = e.Y.ToString();

                if (this.mouseDown)
                {
                    double distance = DistanceTo(this.startPoint, e.Location);
                    distanceTextBox.Text = distance.ToString();

                    if (this.lastPoint != null)
                    {
                        if (recording)
                        {
                            Double stroke =  Math.Abs(DistanceTo(this.lastPoint, e.Location));
                            if (stroke > 1)
                            {
                                this.currentStrokeTravel += stroke;
                                strokeTravelTextBox.Text = this.currentStrokeTravel.ToString();
                                totalStrokeTravelTextBox.Text = (this.totalStrokesTravel + this.currentStrokeTravel).ToString();
                                this.lastPoint = e.Location;
                            }

                         
                            
                        }
                    }
                    else
                    {
                        this.lastPoint = e.Location;
                    }

                    

                    double limit;
                    if (double.TryParse(limitTextBox.Text, out limit))
                    {
                        double percent = distance / limit;
                        if( percent > 1){
                            distanceTextBox.Background = System.Windows.Media.Brushes.Red;
                        }
                        else if (percent > 0.9)
                        {
                            distanceTextBox.Background = System.Windows.Media.Brushes.OrangeRed;
                        }
                        else if (percent > 0.8)
                        {
                            distanceTextBox.Background = System.Windows.Media.Brushes.Orange;
                        }
                        else if (percent > 0.7)
                        {
                            distanceTextBox.Background = System.Windows.Media.Brushes.Orange;
                        }
                        else if (percent > 0.6)
                        {
                            distanceTextBox.Background = System.Windows.Media.Brushes.Yellow;
                        }
                        else
                        {
                            distanceTextBox.Background = System.Windows.Media.Brushes.White;
                        }
                        
                    }
                }
               
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception!!");
            } 
        }

        public double DistanceTo(System.Drawing.Point point1, System.Drawing.Point point2)
        {
            var a = (double)(point2.X - point1.X);
            var b = (double)(point2.Y - point1.Y);
            return Math.Sqrt(a * a + b * b);
        }

        private void recordButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button clickedButton = (System.Windows.Controls.Button) sender;
            this.recording = !this.recording;
            clickedButton.Content = this.recording ? "Stop Recording" : "Start Recording";
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            strokeTravelTextBox.Text = "0";
            totalStrokeTravelTextBox.Text = "0";
            this.currentStrokeTravel = 0;
            this.totalStrokesTravel = 0; 
        }

/*
        private void limitTextBox_Validating(object sender,
                System.ComponentModel.CancelEventArgs e)
        {
            string errorMsg;
      
            if (!ValidNumber(limitTextBox.Text, out errorMsg))
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                limitTextBox.Select(0, limitTextBox.Text.Length);

                // Set the ErrorProvider error with the text to display.  
                this.errorProvider.SetError(limitTextBox, errorMsg);
            }
        }
        private void limitTextBox_Validated(object sender, System.EventArgs e)
        {
            // If all conditions have been met, clear the ErrorProvider of errors.
            errorProvider.SetError(limitTextBox, "");
        }
        public bool ValidEmailAddress(string emailAddress, out string errorMessage)
        {
            // Confirm that the e-mail address string is not empty. 
            if (emailAddress.Length == 0)
            {
                errorMessage = "e-mail address is required.";
                return false;
            }

            // Confirm that there is an "@" and a "." in the e-mail address, and in the correct order.
            if (emailAddress.IndexOf("@") > -1)
            {
                if (emailAddress.IndexOf(".", emailAddress.IndexOf("@")) > emailAddress.IndexOf("@"))
                {
                    errorMessage = "";
                    return true;
                }
            }

            errorMessage = "e-mail address must be valid e-mail address format.\n" +
               "For example 'someone@example.com' ";
            return false;
        }

        public bool ValidNumber(string number, out string errorMessage)
        {
            // Confirm that the e-mail address string is not empty. 
            if (number.Length == 0)
            {
                errorMessage = "Limit is required.";
                return false;
            }
            int n;
            // Confirm that there is an "@" and a "." in the e-mail address, and in the correct order.
            if (int.TryParse(number, out n))
            {

                errorMessage = "";
                return true;
            }

            errorMessage = "Must be a valid number ";
            return false;
        }
        */
    }

   

}

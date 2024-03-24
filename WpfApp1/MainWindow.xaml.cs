using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private CancellationTokenSource primeCts;
        private CancellationTokenSource fibonacciCts;
        private ManualResetEventSlim primeGeneratorEvent = new ManualResetEventSlim(true);
        private ManualResetEventSlim fibonacciGeneratorEvent = new ManualResetEventSlim(true);


        public MainWindow()
        {
           InitializeComponent();
        }

        private void GeneratePrimeButton_Click(object sender, RoutedEventArgs e)
        {
            primeCts?.Cancel();

            primeCts = new CancellationTokenSource();
            CancellationToken cancellationToken = primeCts.Token;

            int lowerBound = string.IsNullOrWhiteSpace(lowerBoundTextBox.Text) ? 2 : int.Parse(lowerBoundTextBox.Text);
            int? upperBound = string.IsNullOrWhiteSpace(upperBoundTextBox.Text) ? null : (int?)int.Parse(upperBoundTextBox.Text);

            Task.Run(() => GeneratePrimeNumbers(lowerBound, upperBound, cancellationToken), cancellationToken);
        }

        private void GenerateFibonacciButton_Click(object sender, RoutedEventArgs e)
        {
            fibonacciCts?.Cancel();
            fibonacciCts = new CancellationTokenSource();
            CancellationToken cancellationToken = fibonacciCts.Token;

            Task.Run(() => GenerateFibonacciNumbers(cancellationToken), cancellationToken);
        }

        private void GeneratePrimeNumbers(int lowerBound, int? upperBound, CancellationToken cancellationToken)
        {
            int number = lowerBound;
            try
            {
                Dispatcher.Invoke(() => primeNumberListBox.Items.Clear());
                while (!upperBound.HasValue || number <= upperBound.Value)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    if (IsPrime(number))
                    {
                        UpdateUi(number.ToString(), primeNumberListBox);
                    }
                    number++;
                    Thread.Sleep(100);
                    primeGeneratorEvent.Wait(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
               
            }
        }


        private void GenerateFibonacciNumbers(CancellationToken cancellationToken)
        {
            int a = 0, b = 1;
            try
            {   Dispatcher.Invoke(()=>fibonacciNumbersListBox.Items.Clear());
                while (!cancellationToken.IsCancellationRequested)
                {
                    fibonacciGeneratorEvent.Wait(cancellationToken);

                    UpdateUiForFibonacci(a, fibonacciNumbersListBox);

                    int temp = a;
                    a = b;
                    b = temp + b;

                    Thread.Sleep(500);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }


        private bool IsPrime(int number)
            {
                if (number <= 1) return false;

                for (int i = 2; i <= Math.Sqrt(number); i++)
                {
                    if (number % i == 0) return false;
                }
                return true;
        }

        private void UpdateUi(string number, ListBox listBox)
        {
            Dispatcher.Invoke(() =>
            {
                listBox.Items.Add(number);
                listBox.ScrollIntoView(number);
            });
        }

        private void UpdateUiForFibonacci(int fibonacciNumber, ListBox listBox)
        {
            Dispatcher.Invoke(() =>
            {
                listBox.Items.Add(fibonacciNumber);
                listBox.ScrollIntoView(fibonacciNumber);
            });
        }

        private void PauseFibonacciButton_Click(object sender, RoutedEventArgs e)
        {
            fibonacciGeneratorEvent.Reset();
        }
        private void ContinueFibonacciButton_Click(object sender, RoutedEventArgs e)
        {
            fibonacciGeneratorEvent.Set();
        }

        private void PausePrimeButton_Click(object sender, RoutedEventArgs e)
        {
            primeGeneratorEvent.Reset();
        }

        private void ContinuePrimeButton_Click(object sender, RoutedEventArgs e)
        {
            primeGeneratorEvent.Set(); 
        }

        private void CancelPrimeButton_Click(object sender, RoutedEventArgs e)
        {
            primeCts?.Cancel();
            MessageBox.Show("Operation canceled.");
            primeGeneratorEvent.Set();
        }

        private void CancelFibonacciButton_Click(object sender, RoutedEventArgs e)
        {
            fibonacciCts?.Cancel();
            MessageBox.Show("Operation canceled.");
            fibonacciGeneratorEvent.Set();
        }
    }
}
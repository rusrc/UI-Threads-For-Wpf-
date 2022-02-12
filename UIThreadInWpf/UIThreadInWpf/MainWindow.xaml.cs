using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http; // HttpClient
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

namespace UIThreadInWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string url = "https://localhost:7103/slow-endpoint";

        public MainWindow()
        {
            InitializeComponent();
        }

        // Sync methods
        private void Sync_remote_call(object sender, RoutedEventArgs e)
        {
            this.textBox1.Text = "Request has been sent...";
         
            var request = this.GenerateRequest(url, "\"What's time?\"");

            using
            var httpClient = new HttpClient();
            var response = httpClient.Send(request);
            var text = this.GetTextFromResponse(response);

            this.textBox1.Text = text;
        }

        // Async methods
        private async void Async_remote_call(object sender, RoutedEventArgs e)
        {
            this.textBox1.Text = "Request has been sent...";

            var request = this.GenerateRequest(url, "\"What's time?\"");

            using
            var httpClient = new HttpClient();
            var response = await httpClient.SendAsync(request);
            var text = this.GetTextFromResponse(response);

            this.textBox1.Text = text;
        }

        // task run no Dispatcher
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.textBox1.Text = "Request has been sent...";
                 
            Task.Run(() =>
            {
                var request = this.GenerateRequest(url, "\"Hi\"");

                using
                var httpClient = new HttpClient();
                var response = httpClient.Send(request);
                var text = this.GetTextFromResponse(response);

                this.textBox1.Text = text;
            });
        }

        // task run with Dispatcher
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.textBox1.Text = "Request has been sent...";
            Task.Run(() =>
            {
                var request = this.GenerateRequest(url, "\"Hi\"");

                using
                var httpClient = new HttpClient();
                var response = httpClient.Send(request);
                var text = this.GetTextFromResponse(response);

                this.Dispatcher.Invoke(() =>
                {
                    this.textBox1.Text = text;
                });

            });
        }

        #region BackgfoundWorker
        // run with BackgroundWorker
        private BackgroundWorker _backgroundWorker;
        private void Button_Click_BackgroundWorker(object sender, RoutedEventArgs e)
        {
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += (obj, ar) => this.TaskAsync();
            _backgroundWorker.RunWorkerCompleted += (obj, ar) => this.textBox1.Text = "Ended";
            _backgroundWorker.RunWorkerAsync();
        }

        private async void TaskAsync()
        {
            for (int i = 0; i < 5; i++)
            {
                System.Threading.Thread.Sleep(1000);

                System.Diagnostics.Debug.WriteLine("BackgroundWorker: " + i);
            }
        } 
        #endregion

        // Using "async await" for non async external calls
        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.textBox1.Text = "Request has been sent...";

              var result = await this.CalcSomethingHard();

            this.textBox1.Text = "Text3: " + result;
        }

        private async Task<string> CalcSomethingHard()
        {
            var result = 0;
            var task = Task.Run(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    System.Threading.Thread.Sleep(1000);

                    System.Diagnostics.Debug.WriteLine("BackgroundWorker: " + i);
                    
                    result += i;
                }
            });

            return await Task.FromResult(result.ToString());
        }

        // Call several external services the good way
        private async void Button_Click_Good_Way_RPC(object sender, RoutedEventArgs e)
        {
            this.textBox1.Text = "Request has been sent...";

            var request2 = this.GenerateRequest(url, "\"Hi 2\"");

            var task = Task.Run(() =>
            {
                var request1 = this.GenerateRequest(url, "\"Hi 1\"");

                using
                var httpClient = new HttpClient();
                var response1 = httpClient.Send(request1);
                var text1 = this.GetTextFromResponse(response1);

                return text1;
            })
             .ContinueWith(firstTaskWithString =>
            {
                var request1 = this.GenerateRequest(url, "\"Hi 2\"");
                var text1 = firstTaskWithString.Result;

                using
                var httpClient = new HttpClient();
                var response2 = httpClient.Send(request2);
                var text2 = this.GetTextFromResponse(response2);

                // Do something with intermediate data
            

                return $"{text1}\n{text2}";
            })
             .ContinueWith(taskWithStringResult =>
             {
                 var finalText = taskWithStringResult.Result;

                 this.Dispatcher.Invoke(() =>
                 {
                     this.textBox1.Text = finalText;
                 });
             });
        }

        // Call several external services the best way
        private async void Button_Click_Best_Way_RPC(object sender, RoutedEventArgs e)
        {
            this.textBox1.Text = "Request has been sent...";

            var request1 = this.GenerateRequest(url, "\"Hi 1\"");
            var request2 = this.GenerateRequest(url, "\"Hi 2\"");

            using
            var httpClient = new HttpClient();
            var response1 = await httpClient.SendAsync(request1);
            var response2 = await httpClient.SendAsync(request2);

            var text1 = this.GetTextFromResponse(response1);
            var text2 = this.GetTextFromResponse(response2);

            this.textBox1.Text = $"{text1}\n{text2}";
        }

        private HttpRequestMessage GenerateRequest(string url, string body)
        {
            var request = new HttpRequestMessage();

            request.Headers.Add("Accept", "*/*");
            // request.Headers.Add("Content-Type", "application/json");

            request.Method = HttpMethod.Post;
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            request.RequestUri = new Uri(url);

            return request;
        }

        private string GetTextFromResponse(HttpResponseMessage httpResponseMessage)
        {
            using var stream = httpResponseMessage.Content.ReadAsStream();
            using var streamReader = new StreamReader(stream);
            var text = streamReader.ReadToEnd();
            return text;
        }

        private int _counter;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _counter++;
            this.textBlock1.Text = _counter.ToString();
        }

    }
}

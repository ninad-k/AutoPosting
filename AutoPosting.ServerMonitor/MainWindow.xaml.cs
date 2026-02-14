using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace AutoPosting.ServerMonitor
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<AccountConfig> Accounts { get; set; } = new ObservableCollection<AccountConfig>();

        private AccountConfig _selectedAccount;
        public AccountConfig SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                _selectedAccount = value;
                OnPropertyChanged();
            }
        }

        private System.Windows.Threading.DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadConfiguration();
            InitializeScheduler();
        }

        private AppConfig _currentConfig;

        private void InitializeScheduler()
        {
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += Scheduler_Tick;
            _timer.Start();
        }

        private async void Scheduler_Tick(object sender, EventArgs e)
        {
            bool configChanged = false;
            foreach (var account in Accounts)
            {
                // Init NextRunTime
                if (account.NextRunTime == DateTime.MinValue)
                {
                    account.NextRunTime = DateTime.Now.AddMinutes(1);
                    configChanged = true;
                }

                if (account.IsScheduleEnabled && DateTime.Now >= account.NextRunTime)
                {
                    await SendMessageAsync(account);
                    
                    // Update stats
                    account.LastRunTime = DateTime.Now;
                    account.NextRunTime = DateTime.Now.AddHours(24);
                    // account.MessagesSentCount is incremented in SendMessageAsync on success
                    
                    configChanged = true;
                }
            }

            if (configChanged)
            {
                SaveConfiguration();
                OnPropertyChanged(nameof(Accounts));
            }
        }

        private async void SendImageNow_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAccount != null)
            {
                await SendMessageAsync(SelectedAccount);
                
                // For manual send, we verify stats are updated
                SelectedAccount.LastRunTime = DateTime.Now;
                SelectedAccount.LastSync = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                SaveConfiguration();
                
                OnPropertyChanged(nameof(Accounts));
                OnPropertyChanged(nameof(SelectedAccount));
                
                MessageBox.Show($"Image sent for {SelectedAccount.Name}!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task SendMessageAsync(AccountConfig account)
        {
            try
            {
                account.Status = "Capturing...";
                
                var screenshotService = new ScreenshotService();
                byte[] imageBytes = await screenshotService.CaptureAsync(account.TargetUrl);
                
                account.Status = "Sending...";
                
                var notificationService = new NotificationService(_currentConfig);
                
                if (string.Equals(account.Type, "Email", StringComparison.OrdinalIgnoreCase))
                {
                     await notificationService.SendEmailAsync(account.Recipient, $"Screenshot Report: {account.Name}", imageBytes);
                }
                else if (string.Equals(account.Type, "WhatsApp", StringComparison.OrdinalIgnoreCase))
                {
                     // Assuming Recipient contains the phone number
                     await notificationService.SendWhatsAppAsync(account.Recipient, imageBytes);
                }
                
                account.MessagesSentCount++;
                account.Status = "Active";
                account.LastSync = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                account.Status = "Error";
                System.Diagnostics.Debug.WriteLine($"Error sending message: {ex.Message}");
                // Ideally log this
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();

                _currentConfig = new AppConfig();
                configuration.Bind(_currentConfig);

                Accounts.Clear();
                if (_currentConfig.Accounts != null)
                {
                    foreach (var account in _currentConfig.Accounts)
                    {
                        Accounts.Add(account);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveConfiguration()
        {
             try
            {
                var appConfig = new AppConfig { Accounts = new List<AccountConfig>(Accounts) };
                string json = System.Text.Json.JsonSerializer.Serialize(appConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("appsettings.json", json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving config: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
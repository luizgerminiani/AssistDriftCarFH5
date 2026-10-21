using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using JSE.Core;
using JSE.Infrastructure;

namespace JSE
{
    public partial class MainWindow : Window
    {
        private readonly AssistSettings _settings = SettingsStore.Load();
        private readonly DriftAssist _assist = new DriftAssist();
        private readonly XInputReader _input = new XInputReader();
        private VirtualGamepad _virtual;
        private TelemetryReceiver _telemetry;
        private readonly DispatcherTimer _loop = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromMilliseconds(8) };
        private readonly Stopwatch _clock = new Stopwatch();
        private bool _assistEnabled = true;
        private bool _connected;
        private bool _loading = true;
        private double _lastOutput;
        private long _previousPacketCount;
        private DateTime _packetRateTime = DateTime.UtcNow;
        private long _packetsPerSecond;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettingsIntoUi();
            _loading = false;
            _loop.Tick += Loop_Tick;
            Closing += (_, __) => ShutdownServices();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_connected) { ShutdownServices(); return; }
            try
            {
                if (!_input.FindController()) throw new InvalidOperationException("Nenhum controle XInput físico foi encontrado.");
                _telemetry = new TelemetryReceiver(20777);
                _virtual = new VirtualGamepad();
                _telemetry.Start();
                _virtual.Connect();
                _assist.Reset();
                _clock.Restart();
                _connected = true;
                _loop.Start();
                ConnectButton.Content = "Desconectar";
                FooterStatus.Text = "JSE ativo. Use HidHide para ocultar o controle físico do FH5 e evitar entrada dupla.";
            }
            catch (Exception ex)
            {
                ShutdownServices();
                MessageBox.Show(ex.Message + "\n\nConfirme se o ViGEmBus está instalado e se o controle foi conectado antes do JSE.", "Não foi possível conectar", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Loop_Tick(object sender, EventArgs e)
        {
            double dt = Math.Max(0.001, _clock.Elapsed.TotalSeconds);
            _clock.Restart();
            if (!_input.TryRead(out GamepadState state))
            {
                ControllerStatus.Text = "Desconectado";
                _virtual?.Submit(default, 0);
                return;
            }
            ControllerStatus.Text = $"Conectado (XInput {_input.ControllerIndex})";
            double raw = state.NormalizedSteer;
            TelemetryFrame freshTelemetry = _telemetry != null && _telemetry.IsReceiving ? _telemetry.Latest : null;
            _lastOutput = _assist.Update(raw, freshTelemetry, _settings, dt, _assistEnabled);
            _virtual?.Submit(state, _lastOutput);
            UpdateDashboard(raw);
        }

        private void UpdateDashboard(double raw)
        {
            TelemetryFrame frame = _telemetry?.Latest;
            if ((DateTime.UtcNow - _packetRateTime).TotalSeconds >= 1 && _telemetry != null)
            {
                long count = _telemetry.PacketCount;
                _packetsPerSecond = count - _previousPacketCount;
                _previousPacketCount = count;
                _packetRateTime = DateTime.UtcNow;
            }
            TelemetryStatus.Text = _telemetry != null && _telemetry.IsReceiving ? $"Recebendo ({_packetsPerSecond} pkt/s)" : "Aguardando :20777";
            if (frame != null)
            {
                SpeedValue.Text = $"{frame.SpeedKmh:F1} km/h";
                YawValue.Text = $"{frame.YawRate:F3} rad/s";
                SlipValue.Text = $"{frame.BodySlipRadians:F3} rad";
            }
            BlendValue.Text = $"{_assist.LastBlend * 100:F0}%";
            RawSteerValue.Text = raw.ToString("F3");
            OutputSteerValue.Text = _lastOutput.ToString("F3");
            RawSteerBar.Value = raw;
            OutputSteerBar.Value = _lastOutput;
        }

        private void AssistButton_Click(object sender, RoutedEventArgs e)
        {
            _assistEnabled = !_assistEnabled;
            AssistButton.Content = _assistEnabled ? "Assistência ligada" : "Assistência desligada";
            AssistButton.Opacity = _assistEnabled ? 1.0 : 0.55;
        }

        private void SettingSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading || GyroText == null) return;
            ReadSettingsFromUi();
            RenderSettingValues();
            SettingsStore.Save(_settings);
        }

        private void LoadSettingsIntoUi()
        {
            GyroSlider.Value = _settings.GyroStrength;
            CounterSlider.Value = _settings.CounterGain;
            DeadbandSlider.Value = _settings.SlipDeadband;
            InputSmoothingSlider.Value = _settings.InputSmoothing;
            OutputSmoothingSlider.Value = _settings.OutputSmoothing;
            FadeSlider.Value = _settings.AssistFadeRange;
            RateSlider.Value = _settings.MaxSteerRate;
            MinSpeedSlider.Value = _settings.MinSpeedKmh;
            RenderSettingValues();
        }

        private void ReadSettingsFromUi()
        {
            _settings.GyroStrength = GyroSlider.Value;
            _settings.CounterGain = CounterSlider.Value;
            _settings.SlipDeadband = DeadbandSlider.Value;
            _settings.InputSmoothing = InputSmoothingSlider.Value;
            _settings.OutputSmoothing = OutputSmoothingSlider.Value;
            _settings.AssistFadeRange = FadeSlider.Value;
            _settings.MaxSteerRate = RateSlider.Value;
            _settings.MinSpeedKmh = MinSpeedSlider.Value;
            _settings.Clamp();
        }

        private void RenderSettingValues()
        {
            GyroText.Text = _settings.GyroStrength.ToString("F3");
            CounterText.Text = _settings.CounterGain.ToString("F3");
            DeadbandText.Text = _settings.SlipDeadband.ToString("F3");
            InputSmoothingText.Text = _settings.InputSmoothing.ToString("F3");
            OutputSmoothingText.Text = _settings.OutputSmoothing.ToString("F3");
            FadeText.Text = _settings.AssistFadeRange.ToString("F3");
            RateText.Text = _settings.MaxSteerRate.ToString("F3");
            MinSpeedText.Text = _settings.MinSpeedKmh.ToString("F0");
        }

        private void ShutdownServices()
        {
            _loop.Stop();
            _connected = false;
            _virtual?.Dispose();
            _virtual = null;
            _telemetry?.Dispose();
            _telemetry = null;
            if (ConnectButton != null) ConnectButton.Content = "Conectar";
            if (ControllerStatus != null) ControllerStatus.Text = "Desconectado";
            if (FooterStatus != null) FooterStatus.Text = "Desconectado com segurança.";
        }
    }
}

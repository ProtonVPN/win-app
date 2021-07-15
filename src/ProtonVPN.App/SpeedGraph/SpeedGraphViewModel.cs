/*
 * Copyright (c) 2020 Proton Technologies AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Series;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Vpn;

namespace ProtonVPN.SpeedGraph
{
    internal class SpeedGraphViewModel : ViewModel, IVpnStateAware, ILogoutAware
    {
        private ViewResolvingPlotModel _plotModel;
        private readonly LineSeries _downloadAreaSeries = new();
        private List<DataPoint> _downloadDataPoints = new();
        private readonly LineSeries _uploadAreaSeries = new();
        private List<DataPoint> _uploadDataPoints = new();
        private int _secondsPassed;
        private readonly int _maxDataPoints = 60;
        private readonly VpnConnectionSpeed _speedTracker;
        private readonly DispatcherTimer _timer;
        private readonly ILogger _logger;

        private double _totalBytesDownloaded;
        private double _totalBytesUploaded;
        private double _currentDownloadSpeed;
        private double _currentUploadSpeed;
        private double _maxBandwidth;

        public SpeedGraphViewModel(VpnConnectionSpeed speedTracker, ILogger logger)
        {
            _logger = logger;
            _speedTracker = speedTracker;

            InitPlotModel();
            InitPlotController();
            InitDownloadSeries();
            InitUploadSeries();

            _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _timer.Tick += DrawSpeedLines;
        }

        public string Title { get; set; }
        public IList<DataPoint> Points { get; set; }
        public PlotController PlotController { get; set; }

        public int SecondsPassed
        {
            get => _secondsPassed;
            set => Set(ref _secondsPassed, value);
        }

        public double TotalBytesDownloaded
        {
            get => _totalBytesDownloaded;
            set => Set(ref _totalBytesDownloaded, value);
        }

        public double TotalBytesUploaded
        {
            get => _totalBytesUploaded;
            set => Set(ref _totalBytesUploaded, value);
        }

        public double CurrentDownloadSpeed
        {
            get => _currentDownloadSpeed;
            set => Set(ref _currentDownloadSpeed, value);
        }

        public double CurrentUploadSpeed
        {
            get => _currentUploadSpeed;
            set => Set(ref _currentUploadSpeed, value);
        }

        public double MaxBandwidth
        {
            get => _maxBandwidth;
            set => Set(ref _maxBandwidth, value);
        }

        public ViewResolvingPlotModel PlotModel
        {
            get => _plotModel;
            set => Set(ref _plotModel, value);
        }

        public void OnUserLoggedOut()
        {
            if (_timer.IsEnabled)
            {
                _logger.Info("Session graph stopped due to logout");
                ResetGraph();
            }
        }

        public void InitDownloadSeries()
        {
            _downloadAreaSeries.ItemsSource = _downloadDataPoints;
            _downloadAreaSeries.Color = OxyColor.Parse("#369d5b");
            _downloadAreaSeries.CanTrackerInterpolatePoints = false;
            PlotModel.Series.Add(_downloadAreaSeries);
        }

        public void InitUploadSeries()
        {
            _uploadAreaSeries.ItemsSource = _uploadDataPoints;
            _uploadAreaSeries.Color = OxyColor.Parse("#56b39c");
            _uploadAreaSeries.CanTrackerInterpolatePoints = false;
            PlotModel.Series.Add(_uploadAreaSeries);
        }

        public Task OnVpnStateChanged(VpnStateChangedEventArgs e)
        {
            if (e.State.Status.Equals(VpnStatus.Connected))
            {
                PopulateInitialValues(_downloadDataPoints);
                PopulateInitialValues(_uploadDataPoints);

                _downloadAreaSeries.ItemsSource = _downloadDataPoints;
                _uploadAreaSeries.ItemsSource = _uploadDataPoints;

                PlotModel.InvalidatePlot(true);

                _logger.Info("Session graph started.");
                _timer.Start();
            }
            else if (_timer.IsEnabled && e.State.Status != VpnStatus.ActionRequired)
            {
                _logger.Info("Session graph stopped due to VPN status change: " + e.State.Status);
                ResetGraph();
            }

            return Task.CompletedTask;
        }

        private void ResetGraph()
        {
            _timer.Stop();
            ClearSeries();
            SecondsPassed = 0;

            TotalBytesDownloaded = 0;
            TotalBytesUploaded = 0;

            CurrentDownloadSpeed = 0;
            CurrentUploadSpeed = 0;
            MaxBandwidth = 0;
        }

        private void InitPlotModel()
        {
            PlotModel = new ViewResolvingPlotModel {PlotAreaBorderThickness = new OxyThickness(0)};
            PlotModel.Updated += HideAxis;
        }

        private void ClearSeries()
        {
            _downloadDataPoints = new List<DataPoint>();
            _uploadDataPoints = new List<DataPoint>();

            _downloadAreaSeries.ItemsSource = _downloadDataPoints;
            _uploadAreaSeries.ItemsSource = _uploadDataPoints;

            PlotModel.InvalidatePlot(true);
        }

        private void InitPlotController()
        {
            PlotController = new PlotController();
            PlotController.UnbindAll();
        }

        private void DrawSpeedLines(object sender, EventArgs e)
        {
            SecondsPassed++;
            DrawDownloadSpeed();
            DrawUploadSpeed();
            MaxBandwidth = GetMaxBandwidthValue();
            PlotModel.InvalidatePlot(true);
        }

        private double GetMaxBandwidthValue()
        {
            double max = _uploadDataPoints.Select(point => point.Y).Prepend(0).Max();
            return _downloadDataPoints.Select(point => point.Y).Prepend(max).Max();
        }

        private void HideAxis(object sender, EventArgs args)
        {
            _downloadAreaSeries.XAxis.IsZoomEnabled = false;
            _downloadAreaSeries.XAxis.IsAxisVisible = false;
            _downloadAreaSeries.YAxis.IsAxisVisible = false;
            _downloadAreaSeries.YAxis.FilterMinValue = 0;
            _downloadAreaSeries.YAxis.IsZoomEnabled = false;
            _uploadAreaSeries.XAxis.IsAxisVisible = false;
            _uploadAreaSeries.YAxis.IsAxisVisible = false;
            _uploadAreaSeries.XAxis.IsZoomEnabled = false;
            _uploadAreaSeries.YAxis.FilterMinValue = 0;
            _uploadAreaSeries.YAxis.IsZoomEnabled = false;
        }

        private void DrawDownloadSpeed()
        {
            VpnSpeed speed = _speedTracker.Speed();
            TotalBytesDownloaded = _speedTracker.TotalDownloaded();
            CurrentDownloadSpeed = speed.DownloadSpeed;

            if (_downloadDataPoints.Count > _maxDataPoints)
            {
                _downloadDataPoints.RemoveAt(0);
            }

            _downloadDataPoints.Add(new DataPoint(
                _secondsPassed + _maxDataPoints,
                speed.DownloadSpeed >= 1 ? speed.DownloadSpeed : 1));
        }

        private void DrawUploadSpeed()
        {
            VpnSpeed speed = _speedTracker.Speed();
            TotalBytesUploaded = _speedTracker.TotalUploaded();
            CurrentUploadSpeed = speed.UploadSpeed;

            if (_uploadDataPoints.Count > _maxDataPoints)
            {
                _uploadDataPoints.RemoveAt(0);
            }

            _uploadDataPoints.Add(new DataPoint(
                _secondsPassed + _maxDataPoints,
                speed.UploadSpeed >= 1 ? speed.UploadSpeed : 1));
        }

        private void PopulateInitialValues(List<DataPoint> list)
        {
            for (int i = 0; i < _maxDataPoints; i++)
            {
                list.Add(new DataPoint(i, 1));
            }
        }
    }
}
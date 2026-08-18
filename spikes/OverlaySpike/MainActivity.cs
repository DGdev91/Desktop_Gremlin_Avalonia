using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Widget;

namespace OverlaySpike;

[Activity(Label = "OverlaySpike", MainLauncher = true, Exported = true)]
public class MainActivity : Activity
{
    private TextView _status = null!;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
        layout.SetPadding(40, 80, 40, 40);

        _status = new TextView(this);
        layout.AddView(_status);

        var grantButton = new Button(this) { Text = "Grant overlay permission" };
        grantButton.Click += (_, _) => RequestOverlayPermission();
        layout.AddView(grantButton);

        var startButton = new Button(this) { Text = "Start overlay spike" };
        startButton.Click += (_, _) => StartOverlayService();
        layout.AddView(startButton);

        var stopButton = new Button(this) { Text = "Stop overlay spike" };
        stopButton.Click += (_, _) => StopService(new Intent(this, typeof(SpikeOverlayService)));
        layout.AddView(stopButton);

        SetContentView(layout);
    }

    protected override void OnResume()
    {
        base.OnResume();
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        bool granted = Settings.CanDrawOverlays(this);
        _status.Text = $"Overlay permission granted: {granted}";
    }

    private void RequestOverlayPermission()
    {
        if (Settings.CanDrawOverlays(this))
        {
            RefreshStatus();
            return;
        }

        var intent = new Intent(Settings.ActionManageOverlayPermission, Android.Net.Uri.Parse("package:" + PackageName));
        StartActivity(intent);
    }

    private void StartOverlayService()
    {
        if (!Settings.CanDrawOverlays(this))
        {
            RequestOverlayPermission();
            return;
        }

        var intent = new Intent(this, typeof(SpikeOverlayService));
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            StartForegroundService(intent);
        }
        else
        {
            StartService(intent);
        }
    }
}

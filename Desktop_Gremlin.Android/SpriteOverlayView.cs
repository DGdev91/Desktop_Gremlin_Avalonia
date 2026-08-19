using Android.Content;
using Android.Graphics;
using Android.Views;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DesktopGremlin.Droid;

/// <summary>
/// Draws the current animation frame with real per-pixel transparency. A plain Android View
/// composites normally (alpha and all) against a translucent window - unlike Avalonia's own
/// Android rendering surface, which currently can't (see PetOverlayService for why).
///
/// Sheets are decoded frame-by-frame via BitmapRegionDecoder rather than as a single big Bitmap:
/// some sheets (e.g. Companion idle.png, or Mambo's own 3000x10200 idle.png) are single-column
/// strips well past Android's hardware canvas texture size limit ("Canvas: trying to draw too
/// large bitmap"), even though any individual cropped frame is small. The decoder is cached per
/// sheet - but Skia's PNG region decoder still costs tens to ~100ms per call regardless of caching
/// the decoder itself (unlike JPEG, PNG has no cheap seek), so each decoded frame is also cached
/// and reused: every distinct frame of a sheet is decoded at most once, not once per tick.
/// </summary>
public class SpriteOverlayView : View
{
    private readonly Dictionary<global::Avalonia.Media.Imaging.Bitmap, BitmapRegionDecoder> _decoderCache = new();
    private readonly Dictionary<(global::Avalonia.Media.Imaging.Bitmap sheet, int x, int y, int w, int h), Bitmap> _frameCache = new();
    private readonly HashSet<(global::Avalonia.Media.Imaging.Bitmap sheet, int x, int y, int w, int h)> _pendingDecodes = new();
    private readonly Paint _paint = new(PaintFlags.AntiAlias) { FilterBitmap = true };
    private readonly Paint _hotspotPaint = new();

    // Background decodes don't finish in request order. Comparing against only the *latest*
    // requested frame is too strict during a cache-miss burst: newer frames would keep getting requested 
    // faster than any one decode finishes, so nothing is ever still "the latest" by the time it completes 
    // and the view freezes on one stale frame for the whole warm-up instead of animating. 
    // A monotonic sequence number fixes this: any decode that is newer than what's currently on screen gets shown immediately, 
    // even if a still newer one is already in flight - frames display roughly in order, out-of-order arrivals just
    // get skipped, and nothing ever needs to wait for a specific one to "win".
    private long _nextRequestSeq;
    private long _displayedSeq;

    // Same regions and colors as MainWindow.axaml's hotspot Borders, in the same fixed 300x300
    // canvas as the hit-testing rects in PetOverlayService - scaled to the view's actual size in
    // OnDraw the same way the sprite frame itself is.
    private static readonly (RectF Rect, Color Color)[] HotspotOverlays =
    {
        (new RectF(50, 25, 90, 165), Color.Red),
        (new RectF(49, 165, 90, 295), Color.Yellow),
        (new RectF(225, 25, 264, 170), Color.Blue),
        (new RectF(225, 170, 264, 295), new Color(255, 165, 0)),
        (new RectF(115, 30, 205, 80), Color.Purple),
    };

    private Bitmap? _currentBitmap;
    private bool _currentBitmapIsCached;

    private bool _showHotspots;

    /// <summary>Mirrors desktop's "Show Hotspots" tray toggle - a purely visual overlay, separate
    /// from whether hotspots actually respond to taps.</summary>
    public bool ShowHotspots
    {
        get => _showHotspots;
        set { _showHotspots = value; Invalidate(); }
    }

    public SpriteOverlayView(Context context) : base(context)
    {
    }

    public void SetFrame(global::Avalonia.Media.Imaging.CroppedBitmap? cropped)
    {
        if (cropped?.Source is not global::Avalonia.Media.Imaging.Bitmap sheet)
        {
            SetBitmap(null, cached: false);
            return;
        }

        var rect = cropped.SourceRect;
        var key = (sheet, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        long seq = ++_nextRequestSeq;

        if (_frameCache.TryGetValue(key, out var cachedFrame))
        {
            // Synchronous cache hit for this call - always the most recent thing requested so far.
            _displayedSeq = seq;
            SetBitmap(cachedFrame, cached: true);
            return;
        }

        // Cache miss: DecodeRegion costs tens to ~100ms for a never-before-seen frame, but doesn't
        // need the UI thread - only the final Canvas.DrawBitmap does. Decode off the UI thread so a
        // slow first-time decode for one sprite (e.g. Companion) doesn't freeze every other active
        // sprite sharing this same dispatcher/UI thread; this view just keeps showing its previous
        // frame for a tick or two while the new one is in flight, instead of blocking.
        if (!_pendingDecodes.Add(key)) return;

        if (!_decoderCache.TryGetValue(sheet, out var decoder))
        {
            using var ms = new MemoryStream();
            sheet.Save(ms);
            ms.Position = 0;
            decoder = BitmapRegionDecoder.NewInstance(ms, false);
            if (decoder == null)
            {
                _pendingDecodes.Remove(key);
                return;
            }
            _decoderCache[sheet] = decoder;
        }

        var srcRect = new Rect((int)rect.X, (int)rect.Y, (int)(rect.X + rect.Width), (int)(rect.Y + rect.Height));
        var capturedDecoder = decoder;
        Task.Run(() =>
        {
            Bitmap? bmp;
            // BitmapRegionDecoder isn't documented as safe for concurrent calls on the same
            // instance - serialize decodes of the *same* sheet, while different sheets (e.g. the
            // main character's vs Companion's) still decode fully in parallel on separate cores.
            lock (capturedDecoder)
            {
                bmp = capturedDecoder.DecodeRegion(srcRect, null);
            }

            Post(() =>
            {
                _pendingDecodes.Remove(key);
                if (bmp != null)
                {
                    _frameCache[key] = bmp;
                    if (seq > _displayedSeq)
                    {
                        _displayedSeq = seq;
                        SetBitmap(bmp, cached: true);
                    }
                }
            });
        });
    }

    /// <summary>For non-animated content (Target/food): draws the whole bitmap as-is.</summary>
    public void SetStaticBitmap(Bitmap? bitmap) => SetBitmap(bitmap, cached: false);

    private void SetBitmap(Bitmap? bitmap, bool cached)
    {
        // Frames held in _frameCache are reused across ticks - recycling one here would leave a
        // dangling native bitmap the next cache hit tries to draw ("using a recycled bitmap").
        if (_currentBitmap != bitmap && !_currentBitmapIsCached)
        {
            _currentBitmap?.Recycle();
        }
        _currentBitmap = bitmap;
        _currentBitmapIsCached = cached;
        Invalidate();
    }

    protected override void OnDraw(Canvas? canvas)
    {
        base.OnDraw(canvas);
        if (canvas == null) return;

        if (_currentBitmap != null)
        {
            var dst = new RectF(0, 0, Width, Height);
            canvas.DrawBitmap(_currentBitmap, null, dst, _paint);
        }

        if (_showHotspots)
        {
            float scale = Width / 300f;
            foreach (var (rect, color) in HotspotOverlays)
            {
                _hotspotPaint.Color = color;
                canvas.DrawRect(rect.Left * scale, rect.Top * scale, rect.Right * scale, rect.Bottom * scale, _hotspotPaint);
            }
        }
    }
}

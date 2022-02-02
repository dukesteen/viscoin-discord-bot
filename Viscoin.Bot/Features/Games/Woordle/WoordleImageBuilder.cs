using SkiaSharp;
using Viscoin.Bot.Features.Games.Woordle.Types;

namespace Viscoin.Bot.Features.Games.Woordle;

public class WoordleImageBuilder : IDisposable
{
    private SKSurface _surface;
    private SKImageInfo _imageInfo;
    private SKCanvas _canvas;
    private SKData _data = null!;

    private int _currentRow = 0;

    public SKImageInfo ImageInfo => _imageInfo;
    public SKSurface Surface => _surface;

    private static readonly SKColor BgColor = SKColor.Parse("121213");
    private static readonly SKColor WrongCharacterColor = SKColor.Parse("3A3A3C");
    private static readonly SKColor WrongPositionColor = SKColor.Parse("B59F3B");
    private static readonly SKColor RightPositionColor = SKColor.Parse("538D4E");

    private static readonly int TileSize = 64;
    private static readonly int TileMarginSides = 20;
    private static readonly int TileMarginBetween = 10;

    public WoordleImageBuilder(SKImageInfo imageInfo)
    {
        _imageInfo = imageInfo;
        _surface = SKSurface.Create(imageInfo);
        _canvas = _surface.Canvas;
        
        _canvas.Clear(BgColor);
    }
    
    public Stream BuildStream()
    {
        using SKImage image = _surface.Snapshot();
        _data = image.Encode(SKEncodedImageFormat.Png, 100);

        return _data.AsStream();
    }

    public WoordleImageBuilder DrawEmptyRow()
    {
        using (SKPaint paint = new SKPaint())
        {
            paint.Color = WrongCharacterColor;
            paint.IsAntialias = true;
            paint.StrokeWidth = 3;
            paint.Style = SKPaintStyle.Fill;
            paint.TextAlign = SKTextAlign.Center;

            for (int i = 0; i < 5; i++)
            {
                var newX = TileMarginSides + i * (TileSize + TileMarginBetween);
                if (i == 0)
                {
                    newX = TileMarginSides;
                }
                _canvas.DrawRect(newX, _currentRow * (TileSize + TileMarginBetween) + TileMarginSides, TileSize, TileSize, paint);
            }
        }

        _currentRow++;
        return this;
    }

    public WoordleImageBuilder DrawRows(List<WoordleChoice> choices)
    {
        foreach (var word in choices)
        {
            var counter = 0;
            foreach (var character in word.Characters)
            {
                using (SKPaint paint = new SKPaint())
                {
                    paint.Color = character.Status.GetColor();
                    paint.IsAntialias = true;
                    paint.StrokeWidth = 3;
                    paint.Style = SKPaintStyle.Fill;
                    paint.TextAlign = SKTextAlign.Center;
                    
                    var newX = TileMarginSides + counter * (TileSize + TileMarginBetween);
                    var newY = _currentRow * (TileSize + TileMarginBetween) + TileMarginSides;
                    _canvas.DrawRect(newX, newY, TileSize, TileSize, paint);
                    
                    paint.TextSize = 40;
                    paint.Color = SKColors.White;
                    paint.Typeface = SKTypeface.FromFamilyName(
                        "Arial",
                        SKFontStyleWeight.Bold, 
                        SKFontStyleWidth.Normal, 
                        SKFontStyleSlant.Upright);

                    SKRect textBounds = SKRect.Empty;
                    paint.MeasureText(character.Character.ToString(), ref textBounds);

                    paint.StrokeWidth = 4;
                    paint.Style = SKPaintStyle.Fill;

                    var textStartY = (TileSize - textBounds.Height) / 2;
                    
                    _canvas.DrawText(character.Character.ToString().ToUpper(), newX + TileSize / 2, newY + TileSize - 15, paint);
                    
                    counter++;
                }
            }

            _currentRow++;
        }

        for (var currentRow = _currentRow; currentRow < 6; currentRow++)
        {
            using (SKPaint paint = new SKPaint())
            {
                paint.Color = WrongCharacterColor;
                paint.IsAntialias = true;
                paint.StrokeWidth = 3;
                paint.Style = SKPaintStyle.Fill;
                paint.TextAlign = SKTextAlign.Center;

                for (int i = 0; i < 5; i++)
                {
                    var newX = TileMarginSides + i * (TileSize + TileMarginBetween);
                    if (i == 0)
                    {
                        newX = TileMarginSides;
                    }
                    _canvas.DrawRect(newX, currentRow * (TileSize + TileMarginBetween) + TileMarginSides, TileSize, TileSize, paint);
                }
            }
        }
        
        return this;
    }
    
    public void Dispose()
    {
        _surface.Dispose();
        _data.Dispose();
    }
}
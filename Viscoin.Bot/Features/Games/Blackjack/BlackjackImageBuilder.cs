using Serilog;
using SkiaSharp;
using Viscoin.Bot.Shared;
using Viscoin.Bot.Shared.Types;

namespace Viscoin.Bot.Features.Games.Blackjack;

public class BlackjackImageBuilder : IDisposable
{
    private SKSurface _surface;
    private SKImageInfo _imageInfo;
    private SKCanvas _canvas;
    private SKData _data = null!;
    
    private int _playerStartX = 10;
    private int _dealerStartX = 600;
    private int _playerOffsetX;
    private int _dealerOffsetX;

    public SKImageInfo ImageInfo => _imageInfo;
    public SKSurface Surface => _surface;

    public BlackjackImageBuilder(SKImageInfo imageInfo)
    {
        _imageInfo = imageInfo;
        _surface = SKSurface.Create(imageInfo);
        _canvas = _surface.Canvas;
    }

    public BlackjackImageBuilder DrawHeaders()
    {
        using (SKPaint paint = new SKPaint())
        {
            paint.Color = SKColors.White;
            paint.TextSize = 40;
            paint.Typeface = SKTypeface.FromFamilyName(
                "Arial", 
                SKFontStyleWeight.Bold, 
                SKFontStyleWidth.Normal, 
                SKFontStyleSlant.Upright);
            
            paint.StrokeWidth = 4;
            paint.Style = SKPaintStyle.Fill;

            _canvas.DrawText("Player", _playerStartX + _playerOffsetX, _imageInfo.Height / 2 - DrawingConstants.CardHeight / 2 - 20, paint);
            _canvas.DrawText("Dealer", _dealerStartX + _dealerOffsetX, _imageInfo.Height / 2 - DrawingConstants.CardHeight / 2 - 20, paint);
        }
        return this;
    }

    public Stream BuildStream()
    {
        using SKImage image = _surface.Snapshot();
        _data = image.Encode(SKEncodedImageFormat.Png, 100);

        return _data.AsStream();
    }

    public BlackjackImageBuilder FromCardList(List<Card> cards, CardSide side, bool masked = false)
    {
        foreach (var card in cards)
        {
            if (card == cards.First())
            {
                DrawCard(card.Rank, card.Suit, side);
            }
            else
            {
                DrawCard(card.Rank, card.Suit, side, masked);
            }
        }

        return this;
    }
    
    public BlackjackImageBuilder DrawCard(string rank = "", char suit = '0', CardSide side = CardSide.Dealer, bool masked = false)
    {
        int startX = 0;
        int offsetX = 0;
        
        switch (side)
        {
            case CardSide.Dealer:
                startX = _dealerStartX;
                offsetX = _dealerOffsetX;
                _dealerOffsetX += 75;
                break;
            case CardSide.Player:
                startX = _playerStartX;
                offsetX = _playerOffsetX;
                _playerOffsetX += 75;
                break;
        }

        using (SKPaint paint = new SKPaint())
        {
            paint.Color = SKColors.White;
            paint.IsAntialias = true;
            paint.StrokeWidth = 5;
            paint.Style = SKPaintStyle.Fill;
            paint.TextAlign = SKTextAlign.Center;

            _canvas.DrawRoundRect( startX + offsetX, _imageInfo.Height / 2 - DrawingConstants.CardHeight / 2, DrawingConstants.CardWidth, DrawingConstants.CardHeight, 25, 25, paint);

            paint.Style = SKPaintStyle.Stroke;
            paint.Color = SKColors.Black;
                
            _canvas.DrawRoundRect(startX + offsetX, _imageInfo.Height / 2 - DrawingConstants.CardHeight / 2, DrawingConstants.CardWidth, DrawingConstants.CardHeight, 25, 25, paint);

            if (!masked)
            {
                paint.TextSize = 40;
                paint.Typeface = SKTypeface.FromFamilyName(
                    "Arial", 
                    SKFontStyleWeight.Bold, 
                    SKFontStyleWidth.Normal, 
                    SKFontStyleSlant.Upright);
                
                paint.StrokeWidth = 4;
                paint.Style = SKPaintStyle.Fill;
                
                _canvas.DrawText(rank, startX + offsetX + 40, _imageInfo.Height / 2 - 100, paint);
            }
        }
        
        return this;
    }
    
    public void Dispose()
    {
        Log.Debug("BlackjackImageBuilder disposed");
        _surface.Dispose();
        _data.Dispose();
    }
}

public enum CardSide
{
    Player,
    Dealer
}
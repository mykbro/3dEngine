﻿using System;
using System.Windows.Media.Imaging;

namespace _3dGraphics.Graphics
{
    public class Texture
    {
        private readonly byte[] _data;
        private readonly int _bytesPerPixels;
        private readonly int _width;
        private readonly int _height;
        private readonly int _widthMinusOne;
        private readonly int _heightMinusOne;

        private int Width => _width;
        private int Height => _height;       
        
        public Texture(string filepath)
        {
            BitmapSource bmpSource = new BitmapImage(new Uri(filepath));

            _width = (int) Math.Round(bmpSource.Width);
            _height = (int) Math.Round(bmpSource.Height);
            _widthMinusOne = _width - 1;
            _heightMinusOne = _height - 1;
            _bytesPerPixels = bmpSource.Format.BitsPerPixel / 8;

            _data = new byte[_width * _height * _bytesPerPixels];            
            bmpSource.CopyPixels(_data, (_bytesPerPixels * _width), 0);
        }

        public Color GetColor(int x, int y)
        {            
            int clampedX = Math.Clamp(x, 0, _widthMinusOne);
            int clampedY = Math.Clamp(y, 0, _heightMinusOne);
            
            int pixelNr = (_height * clampedY + clampedX) * _bytesPerPixels;
            return new Color(255, _data[pixelNr+2], _data[pixelNr+1], _data[pixelNr]);
        }

        public Color GetColorNormalizedCoords(float u, float v)
        {
            //int x = ((int)(u * _widthMinusOne)) % _width;     //double modulo too expensive
            //int y = ((int)(v * _heightMinusOne)) % _height;
            int x = (int)(Math.Clamp(u * _widthMinusOne, 0, _widthMinusOne));
            int y = (int)(Math.Clamp(v * _heightMinusOne, 0, _heightMinusOne));


            int pixelNr = (_height * y + x) * _bytesPerPixels;
            return new Color(255, _data[pixelNr + 2], _data[pixelNr + 1], _data[pixelNr]);

            //return GetColor(x, y);
        }

    }
}

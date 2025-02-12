﻿namespace astator.Core.Graphics;
public struct Rect
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }



    public Rect(int left, int top, int right, int bottom)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
        this.Width = right - left;
        this.Height = bottom - top;
    }

    public int GetCenterX()
    {
        return (this.Right - this.Left) / 2 + this.Left;
    }

    public int GetCenterY()
    {
        return (this.Bottom - this.Top) / 2 + this.Top;
    }

    public override string ToString()
    {
        return $"[left: {this.Left}, top: {this.Top}, right: {this.Right}, bottom: {this.Bottom}]";
    }
}
﻿using GameOfLife.RenderEngine.UI.Elements;
using GameOfLife.Storage;
using SlimDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GameOfLife.RenderEngine.UI
{
  class SideBar
  {
    TextureInput inputHandler;

    public int Width = 400;
    public int MinimizedWidth = 20;
    public SideBarState State = SideBarState.Minimized;
    readonly Vector2 offset = new Vector2(0, (int)(0.0185 * Config.Height));
    readonly Color activeTabColor = Color.FromArgb(0, 0, 0, 0);

    List<IDrawable2DElement> sideBarBackground = new List<IDrawable2DElement>();
    List<IDrawable2DElement> leftTab = new List<IDrawable2DElement>();
    List<IDrawable2DElement> rightTab = new List<IDrawable2DElement>();
    Rectangle2D maximize;

    List<DrawableString> leftTabStrings = new List<DrawableString>();
    List<DrawableString> rightTabStrings = new List<DrawableString>();

    DrawableString maximizeString;

    public delegate void ClickEventHandler(Point p, SideBarState s);
    public event ClickEventHandler GotInputClick;

    List<Rectangle2D> birth = new List<Rectangle2D>();
    List<Rectangle2D> death = new List<Rectangle2D>();

    public SideBar(TextureInput iHandler)
    {
      inputHandler = iHandler;

      sideBarBackground.Add(new Rectangle2D(new Vector2(0, 0), Width, Config.Height, Color.FromArgb(200, 200, 200, 200)));
      sideBarBackground.Add(new Rectangle2D(new Vector2(0, 0), Width / 2, (int)(0.074 * Config.Height), Color.DimGray, (s) => State = SideBarState.LeftTab, SideBarState.RightTab)); // left tab
      GotInputClick += sideBarBackground.Last().HandleInput;
      sideBarBackground.Add(new Rectangle2D(new Vector2(Width / 2f, 0), Width / 2, (int)(0.074 * Config.Height), activeTabColor, (s) => State = SideBarState.RightTab, SideBarState.LeftTab)); //right tab
      GotInputClick += sideBarBackground.Last().HandleInput;
      rightTab.Add(new Rectangle2D(new Vector2(0, Config.Height - (int)(0.093 * Config.Height)), Width, (int)(0.093 * Config.Height), Color.DimGray, (s) => State = SideBarState.Minimized, SideBarState.LeftTab | SideBarState.RightTab));
      GotInputClick += rightTab.Last().HandleInput;
      maximize = new Rectangle2D(new Vector2(0, 0), MinimizedWidth, Config.Height, Color.FromArgb(200, 200, 200, 200), (s) => State = SideBarState.RightTab, SideBarState.Minimized);
      GotInputClick += maximize.HandleInput;

      var leftTabString = new DrawableString("Muster", new Vector2((float)0.1625 * Width, 5) + offset, Color.White);
      var rightTabString = new DrawableString("Einstellungen", new Vector2(Width / 2f + (float)0.0625 * Width, 5) + offset, Color.White);
      var minimizeString = new DrawableString("Einklappen", new Vector2((float)0.35 * Width, Config.Height - (int)(0.06 * Config.Height)), Color.White);
      maximizeString = new DrawableString(">", new Vector2((float)0.0125 * Width, Config.Height / 2f), Color.White);

      var pausebtn = new Rectangle2D(new Vector2((float)0.0625 * Width, (int)(0.093 * Config.Height)), (int)(0.25 * Width), (int)(0.074 * Config.Height), Color.DimGray, (s) => Config.Paused = !Config.Paused, SideBarState.RightTab);
      rightTab.Add(pausebtn);
      GotInputClick += pausebtn.HandleInput;
      var clearbtn = new Rectangle2D(new Vector2((float)0.0625 * Width * 2 + (int)(0.25 * Width), (int)(0.093 * Config.Height)), (int)(0.25 * Width), (int)(0.074 * Config.Height), Color.DimGray, (s) => inputHandler.ClearWorld(), SideBarState.RightTab);
      rightTab.Add(clearbtn);
      GotInputClick += clearbtn.HandleInput;
      var closebtn = new Rectangle2D(new Vector2((float)0.0625 * Width * 3 + (int)(0.25 * Width) * 2, (int)(0.093 * Config.Height)), (int)(0.25 * Width), (int)(0.074 * Config.Height), Color.DarkRed, (s) => RenderFrame.Instance.Exit(), SideBarState.RightTab);
      rightTab.Add(closebtn);
      GotInputClick += closebtn.HandleInput;

      //Farben
      int colorSize = (int)(0.0463 * Config.Height);
      rightTab.Add(new Rectangle2D(new Vector2((float)0.1875 * Width, 0) + 16 * offset, colorSize, colorSize, Color.Red, (s) => inputHandler.ChangeColor(new Color4(1, 1, 0, 0)), SideBarState.RightTab));
      GotInputClick += rightTab.Last().HandleInput;
      rightTab.Add(new Rectangle2D(new Vector2((float)0.375 * Width, 0) + 16 * offset, colorSize, colorSize, Color.Green, (s) => inputHandler.ChangeColor(new Color4(1, 0, 1, 0)), SideBarState.RightTab));
      GotInputClick += rightTab.Last().HandleInput;
      rightTab.Add(new Rectangle2D(new Vector2((float)0.5625 * Width, 0) + 16 * offset, colorSize, colorSize, Color.Blue, (s) => inputHandler.ChangeColor(new Color4(1, 0, 0, 1)), SideBarState.RightTab));
      GotInputClick += rightTab.Last().HandleInput;
      rightTab.Add(new Rectangle2D(new Vector2((float)0.75 * Width, 0) + 16 * offset, colorSize, colorSize, Color.Black, (s) => inputHandler.ChangeColor(new Color4(1, 0, 0, 0)), SideBarState.RightTab));
      GotInputClick += rightTab.Last().HandleInput;

      // Birth setting buttons
      for (int i = 0; i < 9; i++)
      {
        birth.Add(new Rectangle2D(new Vector2((float)0.175 * Width, (int)(-0.011 * Config.Height)) + (23 + i * 3) * offset, (int)(0.25 * Width), (int)(0.046 * Config.Height), (Config.BirthRule & 1 << i) > 0 ? Color.Green : Color.DimGray, OnBirthChanged, SideBarState.RightTab, i));

      }
      // Death setting buttons
      for (int i = 0; i < 9; i++)
      {
        death.Add(new Rectangle2D(new Vector2((float)0.625 * Width, (int)(-0.011 * Config.Height)) + (23 + i * 3) * offset, (int)(0.25 * Width), (int)(0.046 * Config.Height), (Config.DeathRule & 1 << i) > 0 ? Color.Green : Color.DimGray, OnDeathChanged, SideBarState.RightTab, i));
      }

      foreach (var r in birth.Concat(death))
      {
        rightTab.Add(r);
        GotInputClick += r.HandleInput;
      }
      var size = DrawableString.Measure("Pause");
      rightTabStrings.Add(new DrawableString("Pause", pausebtn.Location + pausebtn.Size / 2 - size / 2, Color.White));
      size = DrawableString.Measure("Leeren");
      rightTabStrings.Add(new DrawableString("Leeren", clearbtn.Location + clearbtn.Size / 2 - size / 2, Color.White));
      rightTabStrings.Add(new DrawableString("Leben", new Vector2((float)0.2 * Width, 0) + 20 * offset, Color.White));
      rightTabStrings.Add(new DrawableString("Tod", new Vector2((float)0.7 * Width, 0) + 20 * offset, Color.White));
      size = DrawableString.Measure("Beenden");
      rightTabStrings.Add(new DrawableString("Beenden", closebtn.Location + closebtn.Size / 2 - size / 2, Color.White));
      rightTabStrings.Add(new DrawableString("Farbe", new Vector2(10, 10) + 16 * offset, Color.White));
      for (int i = 0; i < 9; i++)
      {
        rightTabStrings.Add(new DrawableString(i.ToString(), new Vector2((float)0.025 * Width, 0) + (23 + 3 * i) * offset, Color.White));
      }
      rightTabStrings.Add(minimizeString);

      rightTabStrings.Add(rightTabString);
      leftTabStrings.Add(rightTabString);

      rightTabStrings.Add(leftTabString);
      leftTabStrings.Add(leftTabString);
      
    }

    private void OnBirthChanged(object sender)
    {
      int index = (int)((Rectangle2D)sender).Data;
      Config.BirthRule ^= (uint)(1 << index);
      ((Rectangle2D)sender).Color = (Config.BirthRule & 1 << index) > 0 ? Color.Green : Color.DimGray;

      if ((Config.BirthRule & 1 << index) > 0 && (Config.DeathRule & 1 << index) > 0)
      {
        Config.DeathRule ^= (uint)(1 << index);
        death[index].Color = Color.DimGray;
      }
    }

    private void OnDeathChanged(object sender)
    {
      int index = (int)((Rectangle2D)sender).Data;
      Config.DeathRule ^= (uint)(1 << index);
      ((Rectangle2D)sender).Color = (Config.DeathRule & 1 << index) > 0 ? Color.Green : Color.DimGray;

      if ((Config.DeathRule & 1 << index) > 0 && (Config.BirthRule & 1 << index) > 0)
      {
        Config.BirthRule ^= (uint)(1 << index);
        birth[index].Color = Color.DimGray;
      }
    }

    public void Draw(SpriteBatch sb)
    {
      if (State == SideBarState.Minimized)
      {
        sb.Draw(maximize);
        sb.DrawString(maximizeString);
        return;
      }

      sb.Draw(sideBarBackground);
      if (State == SideBarState.LeftTab)
      {
        sb.Draw(leftTab);
        sb.DrawString(leftTabStrings);
      }
      else if (State == SideBarState.RightTab)
      {
        sb.Draw(rightTab);
        sb.DrawString(rightTabStrings);
      }


    }

    public bool IsPointInsideSidebar(Point loc)
    {
      return ((loc.X <= Width && State != SideBarState.Minimized) || (loc.X <= MinimizedWidth && State == SideBarState.Minimized));
    }

    public bool HandleMouseMove(Point loc)
    {
      return IsPointInsideSidebar(loc);
    }

    public bool HandleMouseClick(Point loc)
    {
      if (IsPointInsideSidebar(loc))
      {
        GotInputClick?.Invoke(loc, State);
        return true;
      }
      return false;
    }

    internal void Dispose()
    {
      maximize?.Dispose();
      maximizeString?.Dispose();

      foreach (var r in sideBarBackground) r.Dispose();
      foreach (var s in rightTabStrings) s.Dispose();
      foreach (var s in leftTabStrings) s.Dispose();
      foreach (var r in leftTab) r.Dispose();
      foreach (var r in rightTab) r.Dispose();
    }
  }

  [Flags]
  public enum SideBarState
  {
    Minimized = 0,
    LeftTab = 1,
    RightTab = 2,
  }
}
